using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using Serilog;
using Silkstring.Services.Conditions;

namespace Silkstring.Services;

public class CommandHandler
{
    private readonly IFramework _framework;
    private readonly CommandResolver _resolver;
    private readonly ConditionEvaluator _conditions;
    private readonly Func<string, string, bool> _setUserVariable;
    public CommandHandler(CommandResolver resolver, IFramework framework, Func<string, string, bool> setUserVariable)
    {
        _resolver = resolver;
        _framework = framework;
        _conditions = new ConditionEvaluator(_resolver.Resolve);
        _setUserVariable = setUserVariable;
    }

    public async Task ExecuteAsync(IReadOnlyList<string> commands, IReadOnlyList<string> args, int delayMs = 100, CancellationToken cancellationToken = default, Func<string, bool>? shouldSkip = null, int untilTimeoutMs = 30000, bool allowUnsafe = false)
    {
        var (tree, diagnostics) = BlockParser.Parse(commands);
        if (diagnostics.Count > 0)
        {
            Log.Warning("Alias not run: {Count} syntax error(s), first: {Message}", diagnostics.Count, diagnostics[0].Message);
            return;
        }
        var sent = false;

        async Task<bool> RunNodes(IReadOnlyList<BlockNode> nodes)
        {
            foreach (var node in nodes)
            {
                bool halt;
                if (node is IfNode ifn) halt = await RunIf(ifn);
                else if (node is LineNode line) halt = await RunLine(line.Text);
                else halt = false;
                if (halt) return true;
            }
            return false;
        }

        async Task<bool> RunIf(IfNode ifn)
        {
            foreach (var branch in ifn.Branches)
            {
                var take = branch.Condition == null || await _framework.RunOnFrameworkThread(() => EvaluateSafe(branch.Condition!, args));
                if (take) return await RunNodes(branch.Body);
            }
            return false;
        }

        async Task<bool> RunLine(string text)
        {
            var (kind, expression) = BlockInterpreter.Classify(text);
            if (kind == BlockKind.Return) return true;
            if (kind == BlockKind.Set)
            {
                var (name, rawValue) = BlockInterpreter.ParseSet(expression);
                await _framework.RunOnFrameworkThread(() =>
                {
                    if (!_setUserVariable(name, _resolver.Resolve(rawValue, args)))
                        Log.Warning("Unknown user variable in set: {Name}", name);
                });
                return false;
            }
            if (kind == BlockKind.Wait)
            {
                var resolved = await _framework.RunOnFrameworkThread(() => _resolver.Resolve(expression, args));
                if (BlockInterpreter.TryParseDuration(resolved, out var ms)) await Task.Delay(ms, cancellationToken);
                else Log.Warning("Invalid wait duration: {Duration}", resolved);
                return false;
            }
            if (kind == BlockKind.Until)
            {
                var (isUnsafe, condition) = BlockInterpreter.ParseUntil(expression);
                await WaitUntilAsync(condition, args, isUnsafe && allowUnsafe, untilTimeoutMs, cancellationToken);
                return false;
            }
            var cmd = await _framework.RunOnFrameworkThread(() => _resolver.Resolve(text, args));
            if (shouldSkip != null && cmd.StartsWith("/"))
            {
                var parts = cmd.TrimStart('/').Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return false;
                if (shouldSkip(parts[0]))
                {
                    Log.Warning("Skipping recursive command: {Command}", cmd);
                    return false;
                }
            }
            if (sent) await Task.Delay(delayMs, cancellationToken);
            await _framework.RunOnFrameworkThread(() => Chat.SendMessage(cmd));
            sent = true;
            return false;
        }

        await RunNodes(tree);
    }

    private bool EvaluateSafe(string expression, IReadOnlyList<string> args) => _conditions.Evaluate(expression, args);

    private async Task WaitUntilAsync(string condition, IReadOnlyList<string> args, bool isUnsafe, int capMs, CancellationToken token)
    {
        const int pollMs = 100;
        var elapsed = 0;
        while (!await _framework.RunOnFrameworkThread(() => EvaluateSafe(condition, args)))
        {
            await Task.Delay(pollMs, token);
            if (isUnsafe) continue;
            elapsed += pollMs;
            if (elapsed >= capMs) break;
        }
    }
}

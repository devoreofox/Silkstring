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
        var blocks = new BlockInterpreter();
        var sent = false;

        foreach (var line in commands)
        {
            var (kind, expression) = BlockInterpreter.Classify(line);

            if (kind == BlockKind.If)
            {
                var met = blocks.Active && await _framework.RunOnFrameworkThread(() => EvaluateSafe(expression, args));
                blocks.EnterIf(met);
                continue;
            }

            if (kind == BlockKind.Else) { blocks.Else(); continue; }
            if (kind == BlockKind.EndIf) { blocks.EndIf(); continue; }

            if (kind == BlockKind.Set)
            {
                if (blocks.Active)
                {
                    var (name, rawValue) = BlockInterpreter.ParseSet(expression);
                    await _framework.RunOnFrameworkThread(() =>
                    {
                        if (!_setUserVariable(name, _resolver.Resolve(rawValue, args)))
                            Log.Warning("Unknown user variable in :set: {Name}", name);
                    });
                }
                continue;
            }

            if (kind == BlockKind.Wait)
            {
                if (blocks.Active)
                {
                    var resolved = await _framework.RunOnFrameworkThread(() => _resolver.Resolve(expression, args));
                    if (BlockInterpreter.TryParseDuration(resolved, out var ms))
                        await Task.Delay(ms, cancellationToken);
                    else
                        Log.Warning("Invalid :wait duration: {Duration}", resolved);
                }
                continue;
            }

            if (kind == BlockKind.Until)
            {
                if (blocks.Active)
                {
                    var (isUnsafe, condition) = BlockInterpreter.ParseUntil(expression);
                    await WaitUntilAsync(condition, args, isUnsafe && allowUnsafe, untilTimeoutMs, cancellationToken);
                }
                continue;
            }

            if (!blocks.Active) continue;

            var cmd = await _framework.RunOnFrameworkThread(() => _resolver.Resolve(line, args));
            if (shouldSkip != null && cmd.StartsWith("/"))
            {
                var parts = cmd.TrimStart('/').Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;
                if (shouldSkip(parts[0]))
                {
                    Log.Warning("Skipping recursive command: {Command}", cmd);
                    continue;
                }
            }

            if (sent) await Task.Delay(delayMs, cancellationToken);
            await _framework.RunOnFrameworkThread(() => Chat.SendMessage(cmd));
            sent = true;

        }
    }

    private bool EvaluateSafe(string expression, IReadOnlyList<string> args)
    {
        try
        {
            return _conditions.Evaluate(expression, args);
        }
        catch (ConditionException ex)
        {
            Log.Warning(ex, "Invalid condition: {Expression}", expression);
            return false;
        }
    }

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

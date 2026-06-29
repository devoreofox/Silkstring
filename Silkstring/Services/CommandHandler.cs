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

    public CommandHandler(CommandResolver resolver, IFramework framework)
    {
        _resolver = resolver;
        _framework = framework;
        _conditions = new ConditionEvaluator(_resolver.Resolve);
    }

    public async Task ExecuteAsync(IReadOnlyList<string> commands, IReadOnlyList<string> args, int delayMs = 100, CancellationToken cancellationToken = default, Func<string, bool>? shouldSkip = null)
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
}

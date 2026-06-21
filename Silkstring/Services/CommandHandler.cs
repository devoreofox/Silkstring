using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using Serilog;

namespace Silkstring.Services;

public class CommandHandler
{
    private readonly IFramework _framework;
    private readonly CommandResolver _resolver;

    public CommandHandler(CommandResolver resolver, IFramework framework)
    {
        _resolver = resolver;
        _framework = framework;
    }

    public async Task ExecuteAsync(IReadOnlyList<string> commands, int delayMs = 100, CancellationToken cancellationToken = default, Func<string, bool>? shouldSkip = null)
    {
        for (var i = 0; i < commands.Count; i++)
        {
            var cmd = commands[i];
            cmd = _resolver.Resolve(cmd);
            if (shouldSkip != null)
            {
                var parts = cmd.TrimStart('/').Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;
                var commandName = parts[0];
                if (shouldSkip(commandName))
                {
                    Log.Warning("Skipping recursive command: {Command}", cmd);
                    continue;
                }
            }
            await _framework.RunOnFrameworkThread(() => Chat.SendMessage(cmd));
            if (i < commands.Count - 1) await Task.Delay(delayMs, cancellationToken);
        }
    }
}

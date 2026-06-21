using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ECommons.Automation;
using Serilog;

namespace Silkstring.Services;

public static class CommandHandler
{
    public static async Task ExecuteAsync(IReadOnlyList<string> commands, int delayMs = 100, CancellationToken cancellationToken = default, Func<string, bool>? shouldSkip = null)
    {
        for (var i = 0; i < commands.Count; i++)
        {
            var cmd = commands[i];
            cmd = CommandResolver.Resolve(cmd);
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
            await Plugin.Framework.RunOnFrameworkThread(() => Chat.SendMessage(cmd));
            if (i < commands.Count - 1) await Task.Delay(delayMs, cancellationToken);
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ECommons.Automation;

namespace Silkstring.Services;

public static class CommandHandler
{
    public static async Task ExecuteAsync(IReadOnlyList<string> commands, int delayMs = 100, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < commands.Count; i++)
        {
            var cmd = commands[i];
            await Plugin.Framework.RunOnFrameworkThread(() => Chat.SendMessage(cmd));
            if (i < commands.Count - 1) await Task.Delay(delayMs, cancellationToken);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using ECommons.Automation;

namespace Silkstring.Services;

public static class CommandHandler
{
    public static async Task ExecuteAsync(List<string> commands, int delayMs = 100)
    {
        foreach (var command in commands)
        {
            await Plugin.Framework.RunOnFrameworkThread(() =>
            {
                Chat.SendMessage(command);
            });
            await Task.Delay(delayMs);
        }
    }
}

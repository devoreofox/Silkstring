using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.Shell;
using Serilog;

namespace Silkstring.Services;

public sealed unsafe class ChatInterceptor : IDisposable
{
    private readonly Configuration _configuration;
    private readonly CommandHandler _commandHandler;
    private readonly IFramework _framework;

    private CancellationTokenSource _cts = new();
    private readonly HashSet<string> _executingAliases = new(StringComparer.OrdinalIgnoreCase);

    private readonly Hook<ShellCommandModule.Delegates.ExecuteCommandInner> _hook;

    public ChatInterceptor(IGameInteropProvider interop, IFramework framework, Configuration configuration, CommandHandler commandHandler)
    {
        _framework = framework;
        _configuration = configuration;
        _commandHandler = commandHandler;

        _hook = interop.HookFromAddress<ShellCommandModule.Delegates.ExecuteCommandInner>(
            ShellCommandModule.MemberFunctionPointers.ExecuteCommandInner,
            ProcessChatInputDetour);
        _hook.Enable();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();

        _hook?.Disable();
        _hook?.Dispose();
    }

    public bool CancelRunning()
    {
        var wasRunning = _executingAliases.Count > 0;
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        return wasRunning;
    }

    private void ProcessChatInputDetour(ShellCommandModule* shellCommandModule, Utf8String* message, UIModule* uiModule)
    {
        try
        {
            var inputString = message->ToString();
            if (inputString.StartsWith('/'))
            {
                var splitString = inputString.Split(' ');
                var commandName = splitString[0][1..];
                var alias = AliasMatcher.Match(commandName, _configuration.GetAliases());
                if (alias != null)
                {
                    var args = ArgumentParser.Parse(inputString[splitString[0].Length..].TrimStart());
                    var commands = alias.Output
                                        .Where(c => !string.IsNullOrWhiteSpace(c.Command))
                                        .Select(c => c.Command.Trim())
                                        .ToList();
                    var names = alias.Name.Split('|', StringSplitOptions.RemoveEmptyEntries |
                                                      StringSplitOptions.TrimEntries);

                    foreach (var name in names) _executingAliases.Add(name);

                    bool ShouldSkip(string cmd) => _executingAliases.Contains(cmd);

                    _commandHandler.ExecuteAsync(commands, args, _configuration.CommandDelay, _cts.Token,
                                                 shouldSkip: ShouldSkip)
                                   .ContinueWith(t => Log.Error(t.Exception, "Command execution failed"),
                                                 TaskContinuationOptions.OnlyOnFaulted)
                                   .ContinueWith(_ => _framework.RunOnFrameworkThread(() =>
                                   {
                                       foreach (var name in names) _executingAliases.Remove(name);
                                   }));
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while processing command");
        }
        _hook.Original(shellCommandModule, message, uiModule);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System.Threading.Tasks;
using ECommons;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.Shell;
using Serilog;
using Silkstring.Services;
using Silkstring.Windows;

namespace Silkstring;

public sealed unsafe class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static INotificationManager NotificationManager { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;

    private const string CommandName = "/silkstring";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Silkstring");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private HelpWindow HelpWindow { get; init; }

    private Hook<ShellCommandModule.Delegates.ExecuteCommandInner> processChatInputHook;

    private readonly CancellationTokenSource _cts = new();
    private readonly HashSet<string> _executingAliases = new(StringComparer.OrdinalIgnoreCase);

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ECommonsMain.Init(PluginInterface, this);

        processChatInputHook = GameInteropProvider.HookFromAddress<ShellCommandModule.Delegates.ExecuteCommandInner>(
            ShellCommandModule.MemberFunctionPointers.ExecuteCommandInner,
            ProcessChatInputDetour);
        processChatInputHook.Enable();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, ToggleConfigUi);
        HelpWindow = new HelpWindow();

        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(HelpWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "use /silkstring to configure your aliases"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
        Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        ECommonsMain.Dispose();

        _cts.Cancel();
        _cts.Dispose();

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        Framework.Update -= OnFrameworkUpdate;

        processChatInputHook?.Disable();
        processChatInputHook?.Dispose();

        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();
        ConfigWindow.Dispose();
        HelpWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        if (args.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            ToggleHelpUi();
        }
        else MainWindow.Toggle();
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        Configuration.TrySave(TimeSpan.FromMilliseconds(500));
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
    public void ToggleHelpUi() => HelpWindow.Toggle();

    private void ProcessChatInputDetour(ShellCommandModule* shellCommandModule, Utf8String* message, UIModule* uiModule)
    {
        try
        {
            var inputString = message->ToString();
            if (inputString.StartsWith('/'))
            {
                var splitString = inputString.Split(' ');
                var commandName = splitString[0][1..];
                var alias = Configuration.GetAliases().FirstOrDefault(a =>
                        a.Enabled &&
                        a.IsValid() &&
                        a.Name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Any(n => n.Equals(commandName, StringComparison.OrdinalIgnoreCase)));
                if (alias != null)
                {
                    var commands = alias.Output
                                        .Where(c => !string.IsNullOrWhiteSpace(c.Command))
                                        .Select(c => "/" + c.Strip())
                                        .ToList();
                    var names = alias.Name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    foreach (var name in names) _executingAliases.Add(name);

                    bool ShouldSkip(string cmd) => _executingAliases.Contains(cmd);

                    CommandHandler.ExecuteAsync(commands, Configuration.CommandDelay, _cts.Token, shouldSkip: ShouldSkip)
                                  .ContinueWith(t => Log.Error(t.Exception, "Command execution failed"), TaskContinuationOptions.OnlyOnFaulted)
                                  .ContinueWith(_ => Framework.RunOnFrameworkThread(() =>
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
        processChatInputHook.Original(shellCommandModule, message, uiModule);
    }
}

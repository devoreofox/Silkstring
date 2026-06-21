using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ECommons;
using Silkstring.Services;
using Silkstring.Windows;

namespace Silkstring;

public sealed class Plugin : IDalamudPlugin
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

    private readonly CommandResolver _commandResolver;
    private readonly CommandHandler _commandHandler;
    private readonly ChatInterceptor _chatInterceptor;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ECommonsMain.Init(PluginInterface, this);

        _commandResolver = new CommandResolver(PlayerState);
        _commandHandler = new CommandHandler(_commandResolver, Framework);
        _chatInterceptor = new ChatInterceptor(GameInteropProvider, Framework, Configuration, _commandHandler);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, ToggleConfigUi, ToggleHelpUi);
        HelpWindow = new HelpWindow(_commandResolver);

        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(HelpWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "/silkstring → Open the Silkstring alias manager.\n/silkstring help → Open the Silkstring help window."
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
        Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        _chatInterceptor.Dispose();
        ECommonsMain.Dispose();

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        Framework.Update -= OnFrameworkUpdate;

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
}

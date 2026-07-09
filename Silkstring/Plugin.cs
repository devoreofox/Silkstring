using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ECommons;
using Silkstring.Services;
using Silkstring.Services.Variables;
using Silkstring.UI;
using Silkstring.Windows;

namespace Silkstring;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static INotificationManager NotificationManager { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;

    private const string CommandName = "/silkstring";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Silkstring");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private VariablesWindow VariablesWindow { get; init; }
    private HelpWindow HelpWindow { get; init; }
    private ChangelogWindow ChangelogWindow { get; init; }

    private readonly CommandResolver _commandResolver;
    private readonly CommandHandler _commandHandler;
    private readonly ChatInterceptor _chatInterceptor;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Palette.Apply(Configuration.Theme);

        var migration = ConfigMigrator.Migrate(Configuration, PluginInterface);
        if (migration != null)
        {
            var content = string.Join("\n", migration.Messages);
            if (migration.BackupPath != null) content += $"\nA backup was saved as {Path.GetFileName(migration.BackupPath)}.";

            NotificationManager.AddNotification(new Notification
            {
                Title = "Silkstring Updated",
                Content = content,
                Type = NotificationType.Success
            });
        }

        ECommonsMain.Init(PluginInterface, this);

        IVariableProvider[] builtIn =
        [
            new PlayerVariableProvider(PlayerState),
            new VitalsVariablesProvider(ClientState),
            new CombatVariablesProvider(Condition, TargetManager),
            new CurrencyVariablesProvider(),
            new EmoteVariablesProvider(ClientState, DataManager),
        ];

        var reserved = builtIn.SelectMany(p => p.GetVariables()).Select(v => v.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var store = new UserVariableStore(Configuration.UserVariables, reserved, Configuration.MarkDirty);
        IVariableProvider[] providers = [..builtIn, new UserVariableProvider(() => store.Variables)];

        _commandResolver = new CommandResolver(providers);
        _commandHandler = new CommandHandler(_commandResolver, Framework, store.TrySet);
        _chatInterceptor = new ChatInterceptor(GameInteropProvider, Framework, Configuration, _commandHandler);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, ToggleConfigUi, ToggleHelpUi, ToggleChangelogUi);
        VariablesWindow = new VariablesWindow(store, _commandResolver);
        HelpWindow = new HelpWindow(_commandResolver);
        ChangelogWindow = new ChangelogWindow();

        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(VariablesWindow);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(HelpWindow);
        WindowSystem.AddWindow(ChangelogWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "/silkstring → Open the Silkstring alias manager.\n" +
                          "/silkstring help → Open the Silkstring help window.\n" +
                          "/silkstring variables → Open the Silkstring variables window.\n" +
                          "/silkstring changelog → Open the Silkstring changelog window."
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
        Framework.Update += OnFrameworkUpdate;

        var current = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        if (Configuration.LastSeenVersion != current)
        {
            ChangelogWindow.IsOpen = true;
            Configuration.LastSeenVersion = current;
            Configuration.Save();
        }
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
        VariablesWindow.Dispose();
        ConfigWindow.Dispose();
        HelpWindow.Dispose();
        ChangelogWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "help": ToggleHelpUi(); break;
            case "changelog": ToggleChangelogUi(); break;
            case "variables": ToggleVariablesUi(); break;
            case "edit": ToggleConfigUi(); break;
            default: MainWindow.Toggle(); break;
        }
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        Configuration.TrySave(TimeSpan.FromMilliseconds(500));
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
    public void ToggleVariablesUi() => VariablesWindow.Toggle();
    public void ToggleHelpUi() => HelpWindow.Toggle();
    public void ToggleChangelogUi() => ChangelogWindow.Toggle();
}

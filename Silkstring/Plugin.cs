using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.Shell;
using Serilog;
using Silkstring.Windows;

namespace Silkstring;

public sealed unsafe class Plugin : IDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    private const string CommandName = "/silkstring";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Silkstring");
    private EditWindow EditWindow { get; init; }
    private ConfigWindow ConfigWindow { get; init; }

    private Hook<ShellCommandModule.Delegates.ExecuteCommandInner> processChatInputHook;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        processChatInputHook = GameInteropProvider.HookFromAddress<ShellCommandModule.Delegates.ExecuteCommandInner>(
            ShellCommandModule.MemberFunctionPointers.ExecuteCommandInner,
            ProcessChatInputDetour);
        processChatInputHook.Enable();

        EditWindow = new EditWindow(this);
        ConfigWindow = new ConfigWindow(this, EditWindow);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(EditWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "use /silkstring to configure your aliases"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;

        processChatInputHook?.Disable();
        processChatInputHook?.Dispose();

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        EditWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        ConfigWindow.Toggle();
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();

    private void ProcessChatInputDetour(ShellCommandModule* shellCommandModule, Utf8String* message, UIModule* uiModule)
    {
        try
        {
            var inputString = message->ToString();
            if (inputString.StartsWith('/'))
            {
                var splitString = inputString.Split(' ');
                if (splitString.Length > 0)
                {
                    var commandName = splitString[0][1..];
                    var alias = Configuration.Aliases.FirstOrDefault(a =>
                                                                         a.Enabled &&
                                                                         a.IsValid() &&
                                                                         a.Name.Equals(
                                                                             commandName,
                                                                             StringComparison.OrdinalIgnoreCase));
                    if (alias != null)
                    {
                        foreach (var entry in alias.Output)
                        {
                            var commandText = "/" + entry.Command.TrimStart('/');
                            var str = Utf8String.FromString(commandText);
                            processChatInputHook.Original(shellCommandModule, str, uiModule);
                            str->Dtor(true);
                        }

                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
        processChatInputHook.Original(shellCommandModule, message, uiModule);
    }
}

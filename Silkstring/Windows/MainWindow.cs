using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Dalamud.Interface;
using Silkstring.Models;
using Silkstring.Services;
using Silkstring.UI.Panels;

namespace Silkstring.Windows;

public class MainWindow : Window, IDisposable
{
    public event Action<AliasEntry?>? SelectionChanged;

    private readonly AliasSelectPanel _selectPanel;
    private readonly AliasEditPanel   _editPanel;

    private AliasEntry? _selectedAlias;

    internal AliasEntry? SelectedAlias => _selectedAlias;

    public MainWindow(Plugin plugin, Action openSettings, Action openHelp, Action openChangelog, Action openVariables) :  base("Silkstring###ss-main")
    {
        var store = new FolderStore<AliasEntry>(plugin.Configuration.Aliases, plugin.Configuration.Folders, plugin.Configuration.Save);
        _selectPanel = new AliasSelectPanel(store, this);
        _editPanel   = new AliasEditPanel(plugin.Configuration, name => plugin.VariableStore.ValidateName(name), this);

        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.Code,
            Click = _ => openVariables(),
            ShowTooltip = () => ImGui.SetTooltip("Variables")
        });
        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.Cog,
            Click = _ => openSettings(),
            ShowTooltip = () => ImGui.SetTooltip("Settings")
        });

        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.InfoCircle,
            Click = _ => openHelp(),
            ShowTooltip = () => ImGui.SetTooltip("Help")
        });

        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.Scroll,
            Click = _ => openChangelog(),
            ShowTooltip = () => ImGui.SetTooltip("Changelog")
        });
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        var scale = ImGui.GetIO().FontGlobalScale;
        var leftWidth = new Vector2(250 * scale, 0);

        ImGui.BeginChild("###selector", leftWidth, true,
                         ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        _selectPanel.Draw();
        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("###editor", new Vector2(0, 0), true);
        _editPanel.Draw();
        ImGui.EndChild();
    }

    public void SetSelection(AliasEntry? alias)
    {
        _selectedAlias = alias;
        SelectionChanged?.Invoke(alias);
    }

    public void RefreshEditor() => _editPanel.Reload();
}

using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Dalamud.Interface;
using Silkstring.Models;
using Silkstring.UI.Panels;

namespace Silkstring.Windows;

public class MainWindow : Window, IDisposable
{
    public event Action<AliasEntry?, AliasFolder?>? SelectionChanged;

    private readonly AliasSelectPanel _selectPanel;
    private readonly AliasEditPanel   _editPanel;

    private AliasEntry? _selectedAlias;
    private AliasFolder? _selectedFolder;

    internal AliasEntry? SelectedAlias => _selectedAlias;
    internal AliasFolder? SelectedFolder => _selectedFolder;


    public MainWindow(Plugin plugin, Action openSettings, Action openHelp) : base("Silkstring###Main")
    {
        _selectPanel = new AliasSelectPanel(plugin.Configuration, this);
        _editPanel   = new AliasEditPanel(plugin.Configuration, this);

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

    public void SetSelection(AliasEntry? alias, AliasFolder? folder)
    {
        _selectedAlias = alias;
        _selectedFolder = folder;
        SelectionChanged?.Invoke(alias, folder);
    }
}

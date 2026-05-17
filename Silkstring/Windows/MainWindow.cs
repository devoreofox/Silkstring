using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Dalamud.Interface;
using Silkstring.Models;
using Silkstring.Ui;

namespace Silkstring.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly AliasSelectPanel _selectPanel;
    private readonly AliasEditPanel   _editPanel;
    private readonly Action _openSettings;

    internal AliasEntry? SelectedAlias  { get; set; }
    internal AliasFolder? SelectedFolder { get; set; }

    public MainWindow(Plugin plugin, Action openSettings) : base("Silkstring###Main")
    {
        _openSettings = openSettings;
        _selectPanel = new AliasSelectPanel(plugin.Configuration, this, openSettings);
        _editPanel   = new AliasEditPanel(plugin.Configuration, this);

        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.Cog,
            Click = _ => openSettings(),
            ShowTooltip = () => ImGui.SetTooltip("Settings")
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
        var scale= ImGui.GetIO().FontGlobalScale;
        var leftWidth = new Vector2(250 * scale, 0);

        if (ImGui.BeginChild("###selector", leftWidth, true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)) _selectPanel.Draw();
        ImGui.EndChild();

        ImGui.SameLine();

        if (ImGui.BeginChild("###editor", new Vector2(0, 0), true)) _editPanel.Draw();
        ImGui.EndChild();
    }
}

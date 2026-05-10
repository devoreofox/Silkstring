using System;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Silkstring.Models;

namespace Silkstring.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly EditWindow editWindow;
    private readonly ConfigWindow configWindow;
    public MainWindow(Plugin plugin, EditWindow editWindow, ConfigWindow configWindow) : base("Silkstring Aliases###Main")
    {
        configuration = plugin.Configuration;
        this.editWindow = editWindow;
        this.configWindow =  configWindow;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        ImGui.Columns(4);
        var size = ImGui.GetIO().FontGlobalScale;
        ImGui.SetColumnWidth(0, 60 * size);
        ImGui.SetColumnWidth(1, 200 * size);
        ImGui.SetColumnWidth(2,  40 * size);
        ImGui.SetColumnWidth(3,  55 * size);

        ImGui.Text("Enabled");
        ImGui.NextColumn();

        ImGui.Text("Command");
        ImGui.NextColumn();

        ImGui.NextColumn();
        ImGui.NextColumn();

        foreach (var alias in configuration.Aliases)
        {
            if (alias.UniqueId == 0)
            {
                alias.UniqueId = configuration.Aliases.Max(a => a.UniqueId) + 1;
            }

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetColumnWidth() / 2) - (16 * size));
            if (ImGui.Checkbox($"###toggle{alias.UniqueId}", ref alias.Enabled))
            {
                configuration.Save();
            }
            ImGui.NextColumn();

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputText("###name" + alias.UniqueId, ref alias.Name, 100))
            {
                configuration.Save();
            }
            ImGui.NextColumn();

            if (ImGui.Button("Edit###edit" + alias.UniqueId))
            {
                editWindow.Open(alias);
            }
            ImGui.NextColumn();

            var canDelete = ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl;
            ImGui.BeginDisabled(!canDelete);
            if (ImGui.Button("Delete###delete" + alias.UniqueId)) alias.Delete = true;
            ImGui.EndDisabled();
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGui.SetTooltip("Hold Shift + Ctrl to delete");
            ImGui.NextColumn();
        }

        ImGui.Columns(1);

        if (configuration.Aliases.RemoveAll(a => a.Delete) > 0)
        {
            configuration.Save();
        }

        ImGui.Separator();
        if (ImGui.Button("Add Alias"))
        {
            configuration.Aliases.Add(new AliasEntry());
            configuration.Save();
        }

        var availableHeight = ImGui.GetContentRegionAvail().Y;
        var buttonHeight = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y + ImGui.GetStyle().WindowPadding.Y * 2 + 4;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + availableHeight - buttonHeight);
        ImGui.Separator();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 25);
        if (ImGui.Button("Settings"))
        {
            configWindow.Open();
        }

        var closeButtonWidth = ImGui.CalcTextSize("Close").X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.SameLine(ImGui.GetWindowWidth() - closeButtonWidth - ImGui.GetStyle().ItemSpacing.X - ImGui.GetStyle().WindowPadding.X - 16);
        if (ImGui.Button("Close"))
        {
            IsOpen = false;
        }
    }
}

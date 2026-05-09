using System;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Common.Math;
using Silkstring.Models;

namespace Silkstring.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    public ConfigWindow(Plugin plugin) : base("Silkstring Aliases###Config")
    {
        configuration = plugin.Configuration;
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
                // TODO: open EditWindow
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
    }
}

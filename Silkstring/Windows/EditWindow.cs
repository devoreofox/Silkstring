using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Silkstring.Models;

namespace Silkstring.Windows;

public class EditWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private AliasEntry aliasCache;
    private AliasEntry originalAlias;

    public EditWindow(Plugin plugin) : base("Edit Alias###EditWindow")
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
        ImGui.InputText("Alias", ref aliasCache.Name);

        ImGui.Columns(2);
        var size = ImGui.GetIO().FontGlobalScale;

        ImGui.Text("Command");
        ImGui.NextColumn();
        ImGui.NextColumn();

        foreach (var command in aliasCache.Output)
        {
            if (command.UniqueId == 0)
            {
                command.UniqueId = aliasCache.Output.Count == 0 ? 1 : aliasCache.Output.Max(c => c.UniqueId) + 1;
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("###command" + command.UniqueId, ref command.Command, 100);
            ImGui.NextColumn();

            var canDelete = ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl;
            ImGui.BeginDisabled(!canDelete);
            if (ImGui.Button("Delete###delete" + command.UniqueId)) command.Delete = true;
            ImGui.EndDisabled();
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGui.SetTooltip("Hold Shift + Ctrl to delete");
            ImGui.NextColumn();
        }

        ImGui.Columns(1);
        aliasCache.Output.RemoveAll(c => c.Delete);

        ImGui.Separator();
        if (ImGui.Button("Add Command"))
        {
            aliasCache.Output.Add(new CommandEntry());
        }

        ImGui.Separator();
        if (ImGui.Button("Save"))
        {
            originalAlias.Name = aliasCache.Name;
            originalAlias.Enabled = aliasCache.Enabled;
            originalAlias.Output = aliasCache.Output;
            configuration.Save();
            IsOpen = false;
        }
        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            IsOpen = false;
        }
    }

    public void Open(AliasEntry alias)
    {
        aliasCache = alias.Clone();
        originalAlias = alias;
        IsOpen = true;
    }
}

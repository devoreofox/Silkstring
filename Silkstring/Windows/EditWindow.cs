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
    private AliasEntry? aliasCache;
    private AliasEntry? originalAlias;
    private string multilineBuffer = string.Empty;

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
        if (aliasCache == null || originalAlias == null) return;
        ImGui.InputText("Alias", ref aliasCache.Name);

        if (configuration.MultilineCommands)
        {
            DrawMultilineView();
        }
        else
        {
            ImGui.Columns(2);

            ImGui.Text("Command");
            ImGui.NextColumn();
            ImGui.NextColumn();

            foreach (var command in aliasCache.Output)
            {
                if (command.UniqueId == 0)
                    command.UniqueId = aliasCache.Output.Count == 0 ? 1 : aliasCache.Output.Max(c => c.UniqueId) + 1;
                DrawListView(command);
            }
            aliasCache.Output.RemoveAll(c => c.Delete);
        }

        ImGui.Columns(1);

        if (!configuration.MultilineCommands)
        {
            ImGui.Separator();
            if (ImGui.Button("Add Command"))
            {
                aliasCache.Output.Add(new CommandEntry());
            }
        }

        var availableHeight = ImGui.GetContentRegionAvail().Y;
        var buttonHeight = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y + ImGui.GetStyle().WindowPadding.Y * 2 + 4;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + availableHeight - buttonHeight);
        ImGui.Separator();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 25);
        if (ImGui.Button("Save"))
        {
            if (configuration.MultilineCommands)
            {
                aliasCache.Output = multilineBuffer
                                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(c => new CommandEntry { Command = c.Trim() })
                                    .ToList();
            }

            originalAlias.Name = aliasCache.Name;
            originalAlias.Enabled = aliasCache.Enabled;
            originalAlias.Output = aliasCache.Output;
            configuration.Save();
            IsOpen = false;
        }

        var closeButtonWidth = ImGui.CalcTextSize("Close").X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.SameLine(ImGui.GetWindowWidth() - closeButtonWidth - ImGui.GetStyle().ItemSpacing.X - ImGui.GetStyle().WindowPadding.X - 16);
        if (ImGui.Button("Cancel"))
        {
            IsOpen = false;
        }
    }

    public void Open(AliasEntry alias)
    {
        aliasCache = alias.Clone();
        originalAlias = alias;
        multilineBuffer = string.Join("\n", aliasCache.Output.Select(c => c.Command));
        IsOpen = true;
    }

    private void DrawListView(CommandEntry command)
    {
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

    private void DrawMultilineView()
    {
        var footerHeight = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y + ImGui.GetStyle().WindowPadding.Y * 2 + 4;
        ImGui.InputTextMultiline("###multilineCommands", ref multilineBuffer, 5000, new Vector2(-1, ImGui.GetContentRegionAvail().Y - footerHeight));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Silkstring.Models;
using Silkstring.Services;
using Silkstring.Windows;

namespace Silkstring.Ui;

public class AliasEditPanel
{
    private readonly Configuration _configuration;

    private AliasEntry? _selectedAlias;
    private List<string>? _detectedCycle;

    private string _multilineBuffer = string.Empty;
    private int _multilineAliasId = -1;

    public AliasEditPanel(Configuration configuration, MainWindow mainWindow)
    {
        mainWindow.SelectionChanged += (alias, _) =>
        {
            _selectedAlias = alias;
            if (alias != null) RefreshCycleCheck();
            else _detectedCycle = null;
        };
        _configuration = configuration;
    }

    public void Draw()
    {
        var alias = _selectedAlias;

        if (alias == null)
        {
            DrawEmptyState();
            return;
        }

        DrawAliasHeader(alias);
        ImGui.Separator();
        DrawCommandList(alias);
    }

    private void DrawEmptyState()
    {
        var placeholder = "Select an alias to edit";
        var size = ImGui.CalcTextSize(placeholder);
        var region = ImGui.GetContentRegionAvail();

        ImGui.SetCursorPos(new Vector2((region.X - size.X) / 2, (region.Y - size.Y) / 2));
        ImGui.TextDisabled(placeholder);
    }

    private void DrawAliasHeader(AliasEntry alias)
    {
        var tooltipText = alias.Enabled ? "Disable this alias" : "Enable this alias";
        if (ImGui.Checkbox($"###enabled{alias.UniqueId}", ref alias.Enabled)) _configuration.Save();
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(tooltipText);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputTextWithHint($"###aliasName{alias.UniqueId}", "activation command", ref alias.Name, 100))
        {
            _configuration.MarkDirty();
            RefreshCycleCheck();
        }
        var inputTooltip = _detectedCycle is { Count: > 0 }
                               ? $"Cycle detected: {string.Join(" → ", _detectedCycle)}"
                               : "Separate multiple aliases with | e.g. mew|meow|mreow";
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(inputTooltip);
    }

    private void DrawCommandList(AliasEntry alias)
    {
        if (_configuration.MultilineCommands)
        {
            DrawMultilineView(alias);
        }
        else
        {
            DrawListView(alias);
        }
    }

    private void DrawMultilineView(AliasEntry alias)
    {
        if (_multilineAliasId != alias.UniqueId)
        {
            _multilineBuffer = string.Join("\n", alias.Output.Select(c => c.Command));
            _multilineAliasId = alias.UniqueId;
        }

        if (ImGui.InputTextMultiline($"###multiline{alias.UniqueId}", ref _multilineBuffer, 5000, new Vector2(-1, ImGui.GetContentRegionAvail().Y)))
        {
            var lines = _multilineBuffer.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            for (var i = 0; i < lines.Count; i++)
            {
                if (i < alias.Output.Count) alias.Output[i].Command = lines[i];
                else alias.Output.Add(new CommandEntry { Command = lines[i] });
            }

            if (alias.Output.Count > lines.Count) alias.Output.RemoveRange(lines.Count, alias.Output.Count - lines.Count);

            _configuration.MarkDirty();
            RefreshCycleCheck();
        }
    }

    private void DrawListView(AliasEntry alias)
    {
        ImGui.Text("Commands:");
        ImGui.BeginChild("###commandList");
        foreach (var command in alias.Output)
        {
            DrawCommandRow(command);
        }

        alias.Output.RemoveAll(c => c.Delete);

        if (ImGuiComponents.IconButton((int)FontAwesomeIcon.Plus, FontAwesomeIcon.Plus))
        {
            alias.Output.Add(new CommandEntry());
            _configuration.Save();
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add Command");
        ImGui.EndChild();
    }

    private void DrawCommandRow(CommandEntry command)
    {
        ImGui.SetNextItemWidth(-60);
        if (ImGui.InputText($"###cmd{command.UniqueId}", ref command.Command, 200))
        {
            _configuration.MarkDirty();
            RefreshCycleCheck();
        }
        ImGui.SameLine();

        var canDelete = ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl;
        ImGui.BeginDisabled(!canDelete);
        if (ImGuiComponents.IconButton(command.UniqueId, FontAwesomeIcon.Trash)) command.Delete = true;
        ImGui.EndDisabled();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGui.SetTooltip("Hold Shift + Ctrl to delete");
    }

    private void RefreshCycleCheck()
    {
        if (_selectedAlias == null) return;
        _detectedCycle = AliasValidator.FindCycle(_selectedAlias, _configuration.GetAliases());
    }
}

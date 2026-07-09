using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiColorTextEditNet;
using Silkstring.Models;
using Silkstring.Services;
using Silkstring.Windows;

namespace Silkstring.UI.Panels;

public class AliasEditPanel
{
    private readonly Configuration _configuration;

    private AliasEntry? _selectedAlias;
    private List<string>? _detectedCycle;
    private string? _blockError;

    private readonly TextEditor _editor;
    private int _editorAliasId = -1;

    public AliasEditPanel(Configuration configuration, MainWindow mainWindow)
    {
        _configuration = configuration;
        _editor = new TextEditor
        {
            SyntaxHighlighter = new SilkstringHighlighter(() => new HashSet<string>(_configuration.UserVariables.Select(v => v.Name), StringComparer.OrdinalIgnoreCase))
        };
        _editor.Renderer.ColorizeEveryFrame = true;
        mainWindow.SelectionChanged += (alias, _) =>
        {
            _selectedAlias = alias;
            if (alias != null) RefreshCycleCheck();
            else { _detectedCycle = null; _blockError = null; }
        };
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
        if (_blockError != null)
        {
            ImGui.TextColored(Palette.Error, _blockError);
            ImGui.Spacing();
        }
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
        ImGuiUtil.Tooltip(tooltipText);
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
        ImGuiUtil.Tooltip(inputTooltip);
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
        if (_editorAliasId != alias.UniqueId)
        {
            _editor.AllText = string.Join("\n", alias.Output.Select(c => c.Command));
            _editorAliasId = alias.UniqueId;
        }

        _editor.Renderer.ShowLineNumbers = _configuration.ShowLineNumbers;
        SilkstringHighlighter.ApplyPalette(_editor);

        if (_editor.Render("###aliasEditor", new Vector2(-1, ImGui.GetContentRegionAvail().Y)))
        {
            ApplyMultiline(alias, _editor.AllText);
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
        ImGuiUtil.Tooltip("Add Command");
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
        ImGuiUtil.Tooltip("Hold Shift + Ctrl to delete", true);
    }

    private void RefreshCycleCheck()
    {
        if (_selectedAlias == null) return;
        _detectedCycle = AliasValidator.FindCycle(_selectedAlias, _configuration.GetAliases());
        var defined = new HashSet<string>(_configuration.UserVariables.Select(v => v.Name), StringComparer.OrdinalIgnoreCase);
        _blockError = AliasValidator.ValidateBlocks(_selectedAlias) ?? AliasValidator.ValidateSets(_selectedAlias, defined) ?? AliasValidator.ValidateWaits(_selectedAlias) ?? AliasValidator.ValidateUntils(_selectedAlias, _configuration.AllowUnsafeWaits);
    }

    private static void ApplyMultiline(AliasEntry alias, string text)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (var i = 0; i < lines.Length; i++)
        {
            if (i < alias.Output.Count) alias.Output[i].Command = lines[i];
            else alias.Output.Add(new CommandEntry { Command = lines[i] });
        }

        if (alias.Output.Count > lines.Length)  alias.Output.RemoveRange(lines.Length, alias.Output.Count - lines.Length);
    }
}

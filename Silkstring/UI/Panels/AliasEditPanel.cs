using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using ImGuiColorTextEditNet;
using Silkstring.Models;
using Silkstring.Services;
using Silkstring.Windows;

namespace Silkstring.UI.Panels;

public class AliasEditPanel
{
    private readonly Configuration _configuration;

    private AliasEntry? _selectedAlias;
    private List<Diagnostic> _diagnostics = new();

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
            RefreshDiagnostics();
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
        foreach (var d in _diagnostics)
        {
            if (d.Line is not null) continue;
            var color = d.Severity == Severity.Error ? Palette.Error : Palette.Warning;
            ImGui.TextColored(color, d.Message);
        }
        if (_diagnostics.Count > 0) ImGui.Spacing();
        DrawCommands(alias);
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
            RefreshDiagnostics();
        }
        ImGuiUtil.Tooltip("Separate multiple aliases with | e.g. mew|meow|mreow");
    }

    private void DrawCommands(AliasEntry alias)
    {
        if (_editorAliasId != alias.UniqueId)
        {
            _editor.AllText = string.Join("\n", alias.Output.Select(c => c.Command));
            _editorAliasId = alias.UniqueId;
        }

        _editor.Renderer.ShowLineNumbers = _configuration.ShowLineNumbers;
        _editor.Renderer.IsShowingWhitespace = false;

        SilkstringHighlighter.ApplyPalette(_editor);

        if (_editor.Render("###aliasEditor", new Vector2(-1, ImGui.GetContentRegionAvail().Y)))
        {
            ApplyMultiline(alias, _editor.AllText);
            _configuration.MarkDirty();
            RefreshDiagnostics();
        }
    }

    private void RefreshDiagnostics()
    {
        if (_selectedAlias == null) { _diagnostics.Clear(); _editor.ErrorMarkers.SetErrorMarkers(new()); return; }
        var defined = new HashSet<string>(_configuration.UserVariables.Select(v => v.Name), StringComparer.OrdinalIgnoreCase);
        _diagnostics = AliasValidator.Validate(_selectedAlias, defined, _configuration.AllowUnsafeWaits, _configuration.GetAliases());
        var markers = new Dictionary<int, object>();
        foreach (var d in _diagnostics)
            if (d.Line is { } ln) markers[ln + 1] = d.Message;
        _editor.ErrorMarkers.SetErrorMarkers(markers);
    }

    private static void ApplyMultiline(AliasEntry alias, string text)
    {
        var lines = text.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            if (i < alias.Output.Count) alias.Output[i].Command = lines[i];
            else alias.Output.Add(new CommandEntry { Command = lines[i] });
        }

        if (alias.Output.Count > lines.Length)  alias.Output.RemoveRange(lines.Length, alias.Output.Count - lines.Length);
    }
}

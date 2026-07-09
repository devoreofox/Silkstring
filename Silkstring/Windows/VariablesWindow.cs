using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Silkstring.Models;
using Silkstring.Services;
using Silkstring.Services.Variables;
using Silkstring.UI;

namespace Silkstring.Windows;

public class VariablesWindow : Window, IDisposable
{
    private readonly UserVariableStore _store;
    private readonly CommandResolver _resolver;
    private string _newName = string.Empty;
    private string? _addError;

    public VariablesWindow(UserVariableStore store, CommandResolver resolver) : base("Silkstring Variables###Variables")
    {
        _store = store;
        _resolver = resolver;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        DrawAddRow();
        ImGui.Separator();
        DrawList();
    }

    private void DrawAddRow()
    {
        ImGui.SetNextItemWidth(200);
        if(ImGui.InputTextWithHint("###newVar", "new variable name", ref _newName, 100)) _addError = null;
        ImGui.SameLine();
        if (ImGuiComponents.IconButton((int)FontAwesomeIcon.Plus, FontAwesomeIcon.Plus))
        {
            _addError = _store.ValidateName(_newName);
            if (_addError == null)
            {
                _store.TryAdd(_newName);
                _resolver.Refresh();
                _newName = string.Empty;
            }
        }
        ImGuiUtil.Tooltip("Add Variable");
        if (_addError != null) ImGui.TextColored(Palette.Error, _addError);
    }

    private void DrawList()
    {
        if (_store.Variables.Count == 0)
        {
            ImGuiUtil.TextWrappedDisabled("No variables yet. Add one above, then use it with {name} in any alias.");
            return;
        }

        UserVariable? toRemove = null;
        foreach (var variable in _store.Variables)
        {
            ImGui.TextUnformatted(variable.Name);
            ImGui.SameLine(150);
            ImGui.SetNextItemWidth(-60);
            if (ImGui.InputText($"###val{variable.UniqueId}", ref variable.Value, 200)) _store.MarkChanged();
            ImGui.SameLine();
            var canDelete = ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl;
            ImGui.BeginDisabled(!canDelete);
            if (ImGuiComponents.IconButton(variable.UniqueId, FontAwesomeIcon.Trash)) toRemove = variable;
            ImGui.EndDisabled();
            ImGuiUtil.Tooltip("Hold Shift + Ctrl to delete", true);
            ImGui.Indent(150);
            ImGui.SetNextItemWidth(-60);
            if (ImGui.InputTextWithHint($"###desc{variable.UniqueId}", "description (optional)", ref variable.Description, 200)) _store.MarkChanged();
            if (ImGui.IsItemDeactivatedAfterEdit()) _resolver.Refresh();
            ImGui.Unindent(150);
            ImGui.Spacing();
        }

        if (toRemove != null)
        {
            _store.Remove(toRemove);
            _resolver.Refresh();
        }
    }
}

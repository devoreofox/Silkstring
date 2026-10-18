using System;
using System.Linq;
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
    private readonly FolderTree<UserVariable> _tree;
    private readonly InlineRename<UserVariable> _rename = new();
    private string _newName = string.Empty;
    private string? _addError;

    public VariablesWindow(UserVariableStore store, CommandResolver resolver) : base("Silkstring Variables###ss-variables")
    {
        _store = store;
        _resolver = resolver;
        _tree = new FolderTree<UserVariable>(store, "VARIABLE", v => v.Name, v => [v.Name, v.Value, v.Description], DrawVariableRow);
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
        if (!_store.All.Any() && _store.Folders.Count == 0)
        {
            ImGuiUtil.TextWrappedDisabled("No variables yet. Add one above, then use it with {name} in any alias.");
            return;
        }
        _tree.DrawFilter();
        ImGui.BeginChild("###varList", Vector2.Zero);
        _tree.Draw();
        ImGui.EndChild();
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
        ImGui.SameLine();
        if (ImGuiComponents.IconButton((int)FontAwesomeIcon.FolderPlus, FontAwesomeIcon.FolderPlus)) _tree.NewFolder();
        ImGuiUtil.Tooltip("Add Folder");
        if (_addError != null) ImGui.TextColored(Palette.Error, _addError);
    }

    private void DrawVariableRow(UserVariable variable, Folder<UserVariable>? _)
    {
        if (_rename.IsRenaming(variable))
        {
            if (_rename.Draw($"###rename{variable.UniqueId}", out var newName)) Rename(variable, newName.Trim());
            return;
        }
        var deleteWidth = ImGui.GetFrameHeight();
        ImGui.Selectable($"{variable.Name}###name{variable.UniqueId}", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X - deleteWidth - ImGui.GetStyle().ItemSpacing.X, 0));
        if (ImGui.BeginPopupContextItem($"###context{variable.UniqueId}"))
        {
            if (ImGui.MenuItem("Rename")) _rename.Begin(variable, variable.Name);
            ImGui.EndPopup();
        }
        _tree.DragSource(variable, $"Moving {variable.Name}...");
        ImGui.SameLine();
        var canDelete = ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl;
        ImGui.BeginDisabled(!canDelete);
        if (ImGuiComponents.IconButton(variable.UniqueId, FontAwesomeIcon.Trash)) Remove(variable);
        ImGui.EndDisabled();
        ImGuiUtil.Tooltip("Hold Shift + Ctrl to delete", true);
        var labelWidth = ImGui.CalcTextSize("Description:").X + ImGui.GetStyle().ItemSpacing.X;
        ImGui.Indent();
        if (LabeledInput("Value:", $"###val{variable.UniqueId}", ref variable.Value, string.Empty, labelWidth)) _store.MarkChanged();
        if (LabeledInput("Description:", $"###desc{variable.UniqueId}", ref variable.Description, "optional", labelWidth)) _store.MarkChanged();
        if (ImGui.IsItemDeactivatedAfterEdit()) _resolver.Refresh();
        ImGui.Unindent();
        ImGui.Spacing();
    }

    private void Rename(UserVariable variable, string newName)
    {
        if (_store.ValidateName(newName, variable) != null) return;
        variable.Name = newName;
        _store.MarkChanged();
        _resolver.Refresh();
    }

    private void Remove(UserVariable variable)
    {
        _store.Remove(variable);
        _resolver.Refresh();
    }

    private static bool LabeledInput(string label, string id, ref string value, string hint, float labelWidth)
    {
        var start = ImGui.GetCursorPosX();
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        ImGui.SetCursorPosX(start + labelWidth);
        ImGui.SetNextItemWidth(-1);
        return ImGui.InputTextWithHint(id, hint, ref value, 200);
    }
}

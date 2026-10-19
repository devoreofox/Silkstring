using System;
using System.Collections.Generic;
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

    private readonly Configuration _configuration;
    private readonly Action _aliasesChanged;
    private PendingRename? _pendingRename;
    private bool _openRenamePopup;

    private sealed record PendingRename(string OldName, string NewName, List<AliasEntry> Aliases);

    public VariablesWindow(UserVariableStore store, CommandResolver resolver, Configuration configuration, Action aliasesChanged) : base("Silkstring Variables###ss-variables")
    {
        _store = store;
        _resolver = resolver;
        _configuration = configuration;
        _aliasesChanged = aliasesChanged;
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
        DrawRenamePopup();
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
        var oldName = variable.Name;
        variable.Name = newName;
        _store.MarkChanged();
        _resolver.Refresh();
        _aliasesChanged();
        if (oldName.Equals(newName, StringComparison.OrdinalIgnoreCase)) return;
        var uses = VariableRenamer.FindUses(_configuration.GetAliases(), oldName);
        if (uses.Count == 0) return;
        _pendingRename = new PendingRename(oldName, newName, uses);
        _openRenamePopup = true;
    }

    private void DrawRenamePopup()
    {
        if (_openRenamePopup)
        {
            ImGui.OpenPopup("Update aliases?###renameVariable");
            _openRenamePopup = false;
        }
        if (_pendingRename == null || !ImGui.BeginPopupModal("Update aliases?###renameVariable", ImGuiWindowFlags.AlwaysAutoResize)) return;
        ImGui.TextUnformatted($"These aliases use {{{_pendingRename.OldName}}}. Update them to use {{{_pendingRename.NewName}}}?");
        foreach (var alias in _pendingRename.Aliases) ImGui.BulletText(alias.EffectiveName);
        if (ImGui.Button("Update"))
        {
            VariableRenamer.Rename(_pendingRename.Aliases, _pendingRename.OldName, _pendingRename.NewName);
            _configuration.Save();
            _aliasesChanged();
            _pendingRename = null;
            ImGui.CloseCurrentPopup();
        }
        ImGui.SameLine();
        if (ImGui.Button("Leave them"))
        {
            _pendingRename = null;
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndPopup();
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

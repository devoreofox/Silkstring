using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Silkstring.Models;
using Silkstring.Windows;

namespace Silkstring.Ui;

public class AliasSelectPanel
{
    private readonly Configuration _configuration;
    private readonly MainWindow _mainWindow;
    private readonly Action _openSettings;

    private string _filter = string.Empty;
    private bool MatchesFilter(AliasEntry alias) => string.IsNullOrWhiteSpace(_filter) || alias.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase);

    private AliasEntry? _draggedAlias;
    private AliasFolder? _draggedFromFolder;

    private AliasFolder? _renamingFolder;
    private string _renameBuffer = string.Empty;
    private string _preRenameName = string.Empty;
    private bool _focusRename = false;

    private AliasEntry? _renamingAlias;
    private string _renameAliasBuffer = string.Empty;
    private bool _focusRenameAlias = false;

    private const string DragDropType = "ALIAS";

    private static readonly Vector4 FolderColor = new(0.7f, 0.5f, 1.0f, 1.0f);

    public AliasSelectPanel(Configuration configuration, MainWindow mainWindow, Action openSettings)
    {
        _configuration = configuration;
        _mainWindow = mainWindow;
        _openSettings = openSettings;
    }

    public void Draw()
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("###filter", "Filter...", ref _filter, 100);

        var frameHeight = ImGui.GetFrameHeight();
        var listHeight = ImGui.GetContentRegionAvail().Y - frameHeight - ImGui.GetStyle().ItemSpacing.Y;

        if (ImGui.BeginChild("###aliasList", new Vector2(0, listHeight)))
        {
            DrawFolders();
            DrawUnsorted();
        }
        ImGui.EndChild();

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        if (ImGui.BeginChild("###footer", new Vector2(-1, frameHeight))) DrawFooter();
        ImGui.EndChild();
        ImGui.PopStyleVar(2);
    }

    private void DrawFolders()
    {
        foreach (var folder in _configuration.Folders.ToList())
        {
            var filtered = folder.Aliases.Where(a => MatchesFilter(a))
                                 .OrderBy(a => string.IsNullOrWhiteSpace(a.DisplayName) ? a.Name : a.DisplayName)
                                 .ToList();

            if (!string.IsNullOrEmpty(_filter) && filtered.Count == 0) continue;

            bool open;
            bool needsTreePop = false;

            if (_renamingFolder == folder)
            {
                if (_focusRename)
                {
                    ImGui.SetKeyboardFocusHere();
                    _focusRename = false;
                }

                ImGui.SetNextItemWidth(-1);
                ImGui.InputText($"###rename{folder.GetHashCode()}", ref _renameBuffer, 100);

                if (ImGui.IsItemDeactivated())
                {
                    var isDuplicate = _configuration.Folders.Any(f => f != folder && f.Name.Equals(_renameBuffer, StringComparison.OrdinalIgnoreCase));

                    if (string.IsNullOrWhiteSpace(_renameBuffer) || isDuplicate)
                    {
                        if (string.IsNullOrWhiteSpace(_preRenameName)) _configuration.Folders.Remove(folder);
                    }
                    else folder.Name = _renameBuffer;
                    _renamingFolder = null;
                    _configuration.Save();
                }

                open = true;
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, FolderColor);
                open = ImGui.TreeNodeEx($"{folder.Name}###{folder.GetHashCode()}folder",
                                        ImGuiTreeNodeFlags.SpanAvailWidth);
                ImGui.PopStyleColor();
                needsTreePop = open;

                if (ImGui.BeginPopupContextItem($"###folderContext{folder.GetHashCode()}"))
                {
                    if (ImGui.MenuItem("Rename"))
                    {
                        _renamingFolder = folder;
                        _renameBuffer = folder.Name;
                        _preRenameName = folder.Name;
                        _focusRename = true;
                    }

                    if (ImGui.MenuItem("Delete"))
                    {
                        foreach (var alias in folder.Aliases)
                        {
                            _configuration.Aliases.Add(alias);
                        }

                        _configuration.Folders.Remove(folder);
                        _configuration.Save();
                    }

                    ImGui.EndPopup();
                }
            }

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload(DragDropType);
                if (!payload.IsNull && payload.IsDelivery() && _draggedAlias != null)
                    MoveAlias(_draggedAlias, _draggedFromFolder, toFolder: folder);
                ImGui.EndDragDropTarget();
            }

            if (open)
            {
                foreach (var alias in filtered)
                {
                    DrawAliasRow(alias, owningFolder: folder);
                }

                if (needsTreePop) ImGui.TreePop();
            }
        }
    }

    private void DrawUnsorted()
    {
        var filtered = _configuration.Aliases.Where(a => MatchesFilter(a)).OrderBy(a => string.IsNullOrWhiteSpace(a.DisplayName) ? a.Name : a.DisplayName).ToList();

        foreach (var alias in filtered)
        {
            DrawAliasRow(alias, owningFolder: null);
        }

        var remaining = ImGui.GetContentRegionAvail();
        if (remaining.Y > 0)
        {
            ImGui.InvisibleButton("###dropTarget", new Vector2(-1, remaining.Y));
            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload(DragDropType);
                if (!payload.IsNull && payload.IsDelivery() && _draggedAlias != null) MoveAlias(_draggedAlias, _draggedFromFolder, toFolder: null);
                ImGui.EndDragDropTarget();
            }
        }
    }

    private void DrawAliasRow(AliasEntry alias, AliasFolder? owningFolder)
    {
        if (alias.UniqueId == 0)
        {
            var allAliases = _configuration.Aliases.Concat(_configuration.Folders.SelectMany(f => f.Aliases));
            alias.UniqueId = allAliases.Any() ? allAliases.Max(a => a.UniqueId) + 1 : 1;
        }

        if (_renamingAlias == alias)
        {
            if (_focusRenameAlias)
            {
                ImGui.SetKeyboardFocusHere();
                _focusRenameAlias = false;
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputText($"###renameAlias{alias.UniqueId}", ref _renameAliasBuffer, 100);

            if (ImGui.IsItemDeactivated())
            {
                alias.DisplayName = _renameAliasBuffer.Trim();
                _renamingAlias = null;
                _configuration.Save();
            }

            return;
        }

        var displayName = string.IsNullOrWhiteSpace(alias.DisplayName) ? alias.Name : alias.DisplayName;
        var label = owningFolder == null ? $"  {displayName}###{alias.UniqueId}" : $"{displayName}###{alias.UniqueId}";
        var isSelected = _mainWindow.SelectedAlias == alias;
        if (ImGui.Selectable(label, isSelected))
        {
            _mainWindow.SelectedAlias = alias;
            _mainWindow.SelectedFolder = owningFolder;
        }

        if (ImGui.BeginPopupContextItem($"###aliasContext{alias.UniqueId}"))
        {
            if (ImGui.MenuItem("Rename"))
            {
                _renamingAlias = alias;
                _renameAliasBuffer = alias.DisplayName;
                _focusRenameAlias = true;
            }

            if (!string.IsNullOrWhiteSpace(alias.DisplayName) && ImGui.MenuItem("Clear Display Name"))
            {
                alias.DisplayName = string.Empty;
                _configuration.Save();
            }

            if (ImGui.MenuItem("Export to Clipboard"))
            {
                var json = JsonSerializer.Serialize(alias, new JsonSerializerOptions { IncludeFields = true });
                ImGui.SetClipboardText(json);
            }

            ImGui.EndPopup();
        }

        if (ImGui.BeginDragDropSource())
        {
            _draggedAlias = alias;
            _draggedFromFolder = owningFolder;
            ImGui.SetDragDropPayload(DragDropType, ReadOnlySpan<byte>.Empty, 0);
            ImGui.Text($"Moving {displayName}...");
            ImGui.EndDragDropSource();
        }
    }

    private void MoveAlias(AliasEntry alias, AliasFolder? from, AliasFolder? toFolder)
    {
        if (from == toFolder) return;

        if (from != null) from.Aliases.Remove(alias);
        else _configuration.Aliases.Remove(alias);

        if (toFolder != null) toFolder.Aliases.Add(alias);
        else _configuration.Aliases.Add(alias);

        _configuration.Save();
        _draggedAlias = null;
        _draggedFromFolder = null;
    }

    private void DrawFooter()
    {
        var available = ImGui.GetContentRegionAvail();
        var buttonCount = 5;
        var buttonSize = new Vector2(MathF.Floor(available.X / buttonCount), available.Y);
        var canDelete = ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

        if (DrawIconButton(FontAwesomeIcon.Plus, buttonSize, "New Alias"))
        {
            var newAlias = new AliasEntry();
            _configuration.Aliases.Add(newAlias);
            _configuration.Save();
            _mainWindow.SelectedAlias = newAlias;
            _mainWindow.SelectedFolder = null;
            _renamingAlias = newAlias;
            _renameAliasBuffer = string.Empty;
            _focusRenameAlias = true;
        }

        ImGui.SameLine();
        if (DrawIconButton(FontAwesomeIcon.FileImport, buttonSize, "Import from Clipboard"))
        {
            try
            {
                var json = ImGui.GetClipboardText();
                var imported = JsonSerializer.Deserialize<AliasEntry>(json, new JsonSerializerOptions { IncludeFields = true });
                if (imported != null)
                {
                    imported.UniqueId = 0;
                    _configuration.Aliases.Add(imported);
                    _configuration.Save();
                    _mainWindow.SelectedAlias = imported;
                    _renamingAlias = imported;
                    _renameAliasBuffer = imported.DisplayName;
                    _focusRenameAlias = true;
                }
            }
            catch {}
        }

        ImGui.SameLine();
        if(DrawIconButton(FontAwesomeIcon.Clone, buttonSize, "Clone Alias", disabled: _mainWindow.SelectedAlias == null))
        {
            var source = _mainWindow.SelectedAlias!;
            var cloned = source.Clone();
            cloned.UniqueId = 0;

            if (_mainWindow.SelectedFolder != null) _mainWindow.SelectedFolder.Aliases.Add(cloned);
            else _configuration.Aliases.Add(cloned);

            _configuration.Save();
            _mainWindow.SelectedAlias = cloned;
            _renamingAlias = cloned;
            _renameAliasBuffer = cloned.DisplayName;
            _focusRenameAlias = true;
        }

        ImGui.SameLine();
        if (DrawIconButton(FontAwesomeIcon.FolderPlus, buttonSize, "New Folder"))
        {
            var newFolder = new AliasFolder { Name = "New Folder" };
            _configuration.Folders.Add(newFolder);
            _configuration.Save();
            _renamingFolder = newFolder;
            _renameBuffer = string.Empty;
            _preRenameName = string.Empty;
            _focusRename = true;
        }

        ImGui.SameLine();
        if (DrawIconButton(FontAwesomeIcon.Trash, buttonSize,
                           canDelete ? "Delete Selected" : "Hold Shift + Ctrl to delete",
                           disabled: !canDelete || _mainWindow.SelectedAlias == null))
        {
            if (_mainWindow.SelectedFolder != null) _mainWindow.SelectedFolder.Aliases.Remove(_mainWindow.SelectedAlias!);
            else _configuration.Aliases.Remove(_mainWindow.SelectedAlias!);

            _mainWindow.SelectedAlias = null;
            _mainWindow.SelectedFolder = null;
            _configuration.Save();
        }

        ImGui.PopStyleVar();
    }

    private bool DrawIconButton(FontAwesomeIcon icon, Vector2 size, string tooltip, bool disabled = false)
    {
        var framePadding = ImGui.GetStyle().FramePadding;
        var pX = Math.Max(0, (size.X - ImGui.GetFrameHeight()) / 2f + framePadding.X);

        if (disabled) ImGui.BeginDisabled();
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(pX, framePadding.Y));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
        var clicked = ImGuiComponents.IconButton((int)icon, icon);
        ImGui.PopStyleVar(2);
        if (disabled) ImGui.EndDisabled();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGui.SetTooltip(tooltip);
        return clicked;
    }

}

using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Silkstring.Models;
using Silkstring.Windows;

namespace Silkstring.Ui;

public class AliasSelectPanel
{
    private readonly Configuration _configuration;
    private readonly MainWindow _mainWindow;
    private readonly SelectPanelFooter _footer;

    private string _filter = string.Empty;

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

    public AliasSelectPanel(Configuration configuration, MainWindow mainWindow)
    {
        _configuration = configuration;
        _mainWindow = mainWindow;
        _footer = new SelectPanelFooter(configuration, mainWindow, BeginRenameAlias, BeginRenameFolder);
    }

    public void Draw()
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("###filter", "Filter...", ref _filter, 100);

        var frameHeight = ImGui.GetFrameHeight();
        var listHeight = ImGui.GetContentRegionAvail().Y - frameHeight - ImGui.GetStyle().ItemSpacing.Y;

        ImGui.BeginChild("###aliasList", new Vector2(0, listHeight));
        DrawFolders();
        DrawUnsorted();
        ImGui.EndChild();

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.BeginChild("###footer", new Vector2(-1, frameHeight));
        _footer.Draw();
        ImGui.EndChild();
        ImGui.PopStyleVar(2);
    }

    private void DrawFolders()
    {
        foreach (var folder in _configuration.Folders.ToList())
        {
            var filtered = folder.Aliases.Where(MatchesFilter)
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
                ImGui.InputText($"###rename{folder.UniqueId}", ref _renameBuffer, 100);

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
                open = ImGui.TreeNodeEx($"{folder.Name}###{folder.UniqueId}folder",
                                        ImGuiTreeNodeFlags.SpanAvailWidth);
                ImGui.PopStyleColor();
                needsTreePop = open;

                if (ImGui.BeginPopupContextItem($"###folderContext{folder.UniqueId}"))
                {
                    if (ImGui.MenuItem("Rename")) BeginRenameFolder(folder);

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
        var filtered = _configuration.Aliases.Where(MatchesFilter).OrderBy(a => string.IsNullOrWhiteSpace(a.DisplayName) ? a.Name : a.DisplayName).ToList();

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
            _mainWindow.SetSelection(alias, owningFolder);
        }

        if (ImGui.BeginPopupContextItem($"###aliasContext{alias.UniqueId}"))
        {
            if (ImGui.MenuItem("Rename"))
            {
                BeginRenameAlias(alias);
            }

            if (!string.IsNullOrWhiteSpace(alias.DisplayName) && ImGui.MenuItem("Clear Display Name"))
            {
                alias.DisplayName = string.Empty;
                _configuration.Save();
            }

            if (ImGui.MenuItem("Export to Clipboard"))
            {
                var json = JsonSerializer.Serialize(alias, AliasEntry.SerializerOptions);
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

    private void BeginRenameAlias(AliasEntry alias)
    {
        _renamingAlias = alias;
        _renameAliasBuffer = alias.DisplayName;
        _focusRenameAlias = true;
    }

    private void BeginRenameFolder(AliasFolder folder, bool isNew = false)
    {
        _renamingFolder = folder;
        _renameBuffer = isNew ? string.Empty : folder.Name;
        _preRenameName = isNew ? string.Empty : folder.Name;
        _focusRename = true;
    }
    private bool MatchesFilter(AliasEntry alias)
    {
        if (string.IsNullOrWhiteSpace(_filter)) return true;
        return alias.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase) ||
               alias.DisplayName.Contains(_filter, StringComparison.OrdinalIgnoreCase);
    }

}

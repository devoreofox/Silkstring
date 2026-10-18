using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Silkstring.Models;
using Silkstring.Services;

namespace Silkstring.UI;

public class FolderTree<T> where T : class
{
    private readonly FolderStore<T> _store;
    private readonly string _dragType;
    private readonly Func<T, string> _sortKey;
    private readonly Func<T, string[]> _searchText;
    private readonly Action<T, Folder<T>?> _drawItem;
    private readonly InlineRename<Folder<T>> _rename = new();

    private string _filter = string.Empty;
    private T? _dragged;

    private bool Filtering => !string.IsNullOrWhiteSpace(_filter);

    public FolderTree(
        FolderStore<T> store, string dragType, Func<T, string> sortKey, Func<T, string[]> searchText,
        Action<T, Folder<T>?> drawItem)
    {
        _store = store;
        _dragType = dragType;
        _sortKey = sortKey;
        _searchText = searchText;
        _drawItem = drawItem;
    }

    public void NewFolder()
    {
        _filter = string.Empty;
        BeginRename(_store.AddFolder());
    }

    private void BeginRename(Folder<T> folder) => _rename.Begin(folder, folder.Name);

    public void DrawFilter()
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("###filter", "Filter...", ref _filter, 100);
    }

    public void DragSource(T item, string label)
    {
        if (!ImGui.BeginDragDropSource()) return;
        _dragged = item;
        ImGui.SetDragDropPayload(_dragType, ReadOnlySpan<byte>.Empty, 0);
        ImGui.Text(label);
        ImGui.EndDragDropSource();
    }

    public void Draw()
    {
        DrawFolders();
        DrawUnsorted();
    }

    private void DrawFolders()
    {
        foreach (var folder in _store.Folders.ToList())
        {
            var items = Visible(folder.Items);
            if (Filtering && items.Count == 0) continue;

            bool open;
            bool needsTreePop = false;

            if (_rename.IsRenaming(folder))
            {
                if (_rename.Draw($"###renameFolder{folder.UniqueId}", out var newName)) CommitRename(folder, newName);
                open = true;
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Palette.Folder);
                open = ImGui.TreeNodeEx($"{folder.Name}###{folder.UniqueId}folder", ImGuiTreeNodeFlags.SpanAvailWidth);
                ImGui.PopStyleColor();
                needsTreePop = open;

                if (ImGui.BeginPopupContextItem($"###folderContext{folder.UniqueId}"))
                {
                    if (ImGui.MenuItem("Rename")) BeginRename(folder);
                    if (ImGui.MenuItem("Delete")) _store.RemoveFolder(folder);
                    ImGui.EndPopup();
                }
            }

            DrawDropTarget(folder);

            if (open)
            {
                foreach (var item in items) _drawItem(item, folder);
                if (needsTreePop) ImGui.TreePop();
            }
        }
    }

    private void DrawUnsorted()
    {
        foreach (var item in Visible(_store.TopLevel)) _drawItem(item, null);

        var remaining = ImGui.GetContentRegionAvail();
        if (remaining.Y <= 0) return;
        ImGui.InvisibleButton("###dropTarget", new Vector2(-1, remaining.Y));
        DrawDropTarget(null);
    }

    private void DrawDropTarget(Folder<T>? target)
    {
        if (!ImGui.BeginDragDropTarget()) return;
        var payload = ImGui.AcceptDragDropPayload(_dragType);
        if (!payload.IsNull && payload.IsDelivery() && _dragged != null && _store.FolderOf(_dragged) != target)
        {
            _store.MoveTo(_dragged, target);
            _dragged = null;
        }
        ImGui.EndDragDropTarget();
    }

    private void CommitRename(Folder<T> folder, string newName)
    {
        var isDuplicate = _store.Folders.Any(f => f != folder && f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(newName) && !isDuplicate)
        {
            folder.Name = newName;
            _store.MarkChanged();
        }
        else if (string.IsNullOrWhiteSpace(folder.Name)) _store.RemoveFolder(folder);
    }

    private List<T> Visible(IEnumerable<T> items) => items.Where(i => !Filtering || _searchText(i).Any(s => s.Contains(_filter, StringComparison.OrdinalIgnoreCase)))
                                                          .OrderBy(_sortKey)
                                                          .ToList();
}

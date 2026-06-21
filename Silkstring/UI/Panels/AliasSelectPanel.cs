using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Silkstring.Models;
using Silkstring.Windows;

namespace Silkstring.UI.Panels;

public class AliasSelectPanel
{
    private readonly Configuration _configuration;
    private readonly MainWindow _mainWindow;
    private readonly SelectPanelFooter _footer;

    private string _filter = string.Empty;

    private AliasEntry? _draggedAlias;
    private AliasFolder? _draggedFromFolder;

    private readonly InlineRename<AliasFolder> _folderRename = new();
    private readonly InlineRename<AliasEntry> _aliasRename = new();

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
                                 .OrderBy(a => a.EffectiveName)
                                 .ToList();

            if (!string.IsNullOrEmpty(_filter) && filtered.Count == 0) continue;

            bool open;
            bool needsTreePop = false;

            if (_folderRename.IsRenaming(folder))
            {
                if (_folderRename.Draw($"###rename{folder.UniqueId}", out var newName))
                {
                    var isDuplicate = _configuration.Folders.Any(f => f != folder && f.Name.Equals(newName,  StringComparison.OrdinalIgnoreCase));

                    if (string.IsNullOrWhiteSpace(newName) || isDuplicate)
                    {
                        if (string.IsNullOrWhiteSpace(folder.Name)) _configuration.Folders.Remove(folder);
                    }
                    else folder.Name = newName;

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
                    if (ImGui.MenuItem("Delete")) DeleteFolder(folder);
                    ImGui.EndPopup();
                }
            }

            DrawDropTarget(folder);

            if (open)
            {
                foreach (var alias in filtered) DrawAliasRow(alias, owningFolder: folder);
                if (needsTreePop) ImGui.TreePop();
            }
        }
    }

    private void DrawUnsorted()
    {
        var filtered = _configuration.Aliases.Where(MatchesFilter).OrderBy(a => a.EffectiveName).ToList();

        foreach (var alias in filtered) DrawAliasRow(alias, owningFolder: null);

        var remaining = ImGui.GetContentRegionAvail();
        if (remaining.Y > 0)
        {
            ImGui.InvisibleButton("###dropTarget", new Vector2(-1, remaining.Y));
            DrawDropTarget(null);
        }
    }

    private void DrawAliasRow(AliasEntry alias, AliasFolder? owningFolder)
    {
        if (_aliasRename.IsRenaming(alias))
        {
            if (_aliasRename.Draw($"###renameAlias{alias.UniqueId}", out var newName))
            {
                alias.DisplayName = newName.Trim();
                _configuration.Save();
            }
            return;
        }

        var displayName = alias.EffectiveName;
        var label = owningFolder == null ? $"  {displayName}###{alias.UniqueId}" : $"{displayName}###{alias.UniqueId}";
        var isSelected = _mainWindow.SelectedAlias == alias;
        if (ImGui.Selectable(label, isSelected)) _mainWindow.SetSelection(alias, owningFolder);

        if (ImGui.BeginPopupContextItem($"###aliasContext{alias.UniqueId}"))
        {
            if (ImGui.MenuItem("Rename")) BeginRenameAlias(alias);
            if (!string.IsNullOrWhiteSpace(alias.DisplayName) && ImGui.MenuItem("Clear Display Name")) ClearDisplayName(alias);
            if (ImGui.MenuItem("Export to Clipboard")) ExportAlias(alias);
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

    private void DrawDropTarget(AliasFolder? target)
    {
        if (!ImGui.BeginDragDropTarget()) return;
        var payload = ImGui.AcceptDragDropPayload(DragDropType);
        if (!payload.IsNull && payload.IsDelivery() && _draggedAlias != null) MoveAlias(_draggedAlias, _draggedFromFolder, target);
        ImGui.EndDragDropTarget();
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

    private static void ExportAlias(AliasEntry alias)
    {
        var json = JsonSerializer.Serialize(alias, AliasEntry.SerializerOptions);
        ImGui.SetClipboardText(json);
    }

    private void ClearDisplayName(AliasEntry alias)
    {
        alias.DisplayName = string.Empty;
        _configuration.Save();
    }

    private void BeginRenameAlias(AliasEntry alias) => _aliasRename.Begin(alias, alias.DisplayName);

    private void BeginRenameFolder(AliasFolder folder) => _folderRename.Begin(folder, folder.Name);

    private void DeleteFolder(AliasFolder folder)
    {
        _configuration.Aliases.AddRange(folder.Aliases);
        _configuration.Folders.Remove(folder);
        _configuration.Save();
    }

    private bool MatchesFilter(AliasEntry alias)
    {
        if (string.IsNullOrWhiteSpace(_filter)) return true;
        return alias.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase) ||
               alias.DisplayName.Contains(_filter, StringComparison.OrdinalIgnoreCase);
    }

}

using System.Numerics;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Silkstring.Models;
using Silkstring.Services;
using Silkstring.Windows;

namespace Silkstring.UI.Panels;

public class AliasSelectPanel
{
    private readonly FolderStore<AliasEntry> _store;
    private readonly MainWindow _mainWindow;
    private readonly FolderTree<AliasEntry> _tree;
    private readonly SelectPanelFooter _footer;

    private readonly InlineRename<AliasEntry> _aliasRename = new();

    public AliasSelectPanel(FolderStore<AliasEntry> store, MainWindow mainWindow)
    {
        _store = store;
        _mainWindow = mainWindow;
        _tree = new FolderTree<AliasEntry>(store, "ALIAS", a => a.EffectiveName, a => [a.Name, a.DisplayName], DrawAliasRow);
        _footer = new SelectPanelFooter(store, mainWindow, BeginRenameAlias, _tree.NewFolder);
    }

    public void Draw()
    {
        _tree.DrawFilter();

        var frameHeight = ImGui.GetFrameHeight();
        var listHeight = ImGui.GetContentRegionAvail().Y - frameHeight - ImGui.GetStyle().ItemSpacing.Y;

        ImGui.BeginChild("###aliasList", new Vector2(0, listHeight));
        _tree.Draw();
        ImGui.EndChild();

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.BeginChild("###footer", new Vector2(-1, frameHeight));
        _footer.Draw();
        ImGui.EndChild();
        ImGui.PopStyleVar(2);
    }

    private void DrawAliasRow(AliasEntry alias, Folder<AliasEntry>? folder)
    {
        if (_aliasRename.IsRenaming(alias))
        {
            if (_aliasRename.Draw($"###renameAlias{alias.UniqueId}", out var newName))
            {
                alias.DisplayName = newName.Trim();
                _store.MarkChanged();
            }
            return;
        }

        var displayName = alias.EffectiveName;
        var label = folder == null ? $"  {displayName}###{alias.UniqueId}" : $"{displayName}###{alias.UniqueId}";
        var isSelected = _mainWindow.SelectedAlias == alias;
        if (ImGui.Selectable(label, isSelected)) _mainWindow.SetSelection(alias);

        if (ImGui.BeginPopupContextItem($"###aliasContext{alias.UniqueId}"))
        {
            if (ImGui.MenuItem("Rename")) BeginRenameAlias(alias);
            if (!string.IsNullOrWhiteSpace(alias.DisplayName) && ImGui.MenuItem("Clear Display Name")) ClearDisplayName(alias);
            if (ImGui.MenuItem("Export to Clipboard")) ExportAlias(alias);
            ImGui.EndPopup();
        }

        _tree.DragSource(alias, $"Moving {displayName}...");
    }

    private static void ExportAlias(AliasEntry alias)
    {
        var json = JsonSerializer.Serialize(alias, AliasEntry.SerializerOptions);
        ImGui.SetClipboardText(json);
    }

    private void ClearDisplayName(AliasEntry alias)
    {
        alias.DisplayName = string.Empty;
        _store.MarkChanged();
    }

    private void BeginRenameAlias(AliasEntry alias) => _aliasRename.Begin(alias, alias.DisplayName);
}

using System;
using System.Numerics;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiNotification;
using Serilog;
using Silkstring.Models;
using Silkstring.Services;
using Silkstring.Windows;

namespace Silkstring.UI.Panels;

public class SelectPanelFooter
{
    private const int FooterButtonCount = 5;

    private readonly FolderStore<AliasEntry> _store;
    private readonly MainWindow _mainWindow;
    private readonly Action<AliasEntry> _beginRenameAlias;
    private readonly Action _newFolder;

    public SelectPanelFooter(
        FolderStore<AliasEntry> store, MainWindow mainWindow, Action<AliasEntry> beginRenameAlias,
        Action newFolder)
    {
        _store = store;
        _mainWindow = mainWindow;
        _beginRenameAlias = beginRenameAlias;
        _newFolder = newFolder;
    }

    public void Draw()
    {
        var available = ImGui.GetContentRegionAvail();
        var buttonSize = new Vector2(MathF.Floor(available.X / FooterButtonCount), available.Y);
        var canDelete = ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

        if (DrawIconButton(FontAwesomeIcon.Plus, buttonSize, "New Alias"))
        {
            var newAlias = new AliasEntry();
            _store.Add(newAlias);
            _mainWindow.SetSelection(newAlias);
            _beginRenameAlias(newAlias);
        }

        ImGui.SameLine();
        if (DrawIconButton(FontAwesomeIcon.FileImport, buttonSize, "Import from Clipboard"))
        {
            try
            {
                var json = ImGui.GetClipboardText();
                var imported =
                    JsonSerializer.Deserialize<AliasEntry>(json, AliasEntry.SerializerOptions);
                if (imported != null)
                {
                    _store.Add(imported);
                    _mainWindow.SetSelection(imported);
                    _beginRenameAlias(imported);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to import alias from clipboard");
                Plugin.NotificationManager.AddNotification(new Notification
                {
                    Title = "Import Failure",
                    Content = "Could not import alias: Clipboard contents are not valid.",
                    Type = NotificationType.Error
                });

            }
        }

        ImGui.SameLine();
        if(DrawIconButton(FontAwesomeIcon.Clone, buttonSize, "Clone Alias", disabled: _mainWindow.SelectedAlias == null))
        {
            var source = _mainWindow.SelectedAlias!;
            var cloned = source.Clone();
            _store.Add(cloned, _store.FolderOf(source));
            _mainWindow.SetSelection(cloned);
            _beginRenameAlias(cloned);
        }

        ImGui.SameLine();
        if (DrawIconButton(FontAwesomeIcon.FolderPlus, buttonSize, "New Folder")) _newFolder();

        ImGui.SameLine();
        if (DrawIconButton(FontAwesomeIcon.Trash, buttonSize,
                           canDelete ? "Delete Selected" : "Hold Shift + Ctrl to delete",
                           disabled: !canDelete || _mainWindow.SelectedAlias == null))
        {
            _store.Remove(_mainWindow.SelectedAlias!);
            _mainWindow.SetSelection(null);
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

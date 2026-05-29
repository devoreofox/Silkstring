using System;
using System.Numerics;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiNotification;
using Serilog;
using Silkstring.Models;
using Silkstring.Windows;

namespace Silkstring.Ui;

public class SelectPanelFooter
{
    private const int FooterButtonCount = 5;

    private readonly Configuration _configuration;
    private readonly MainWindow _mainWindow;
    private readonly Action<AliasEntry> _beginRenameAlias;
    private readonly Action<AliasFolder, bool> _beginRenameFolder;

    public SelectPanelFooter(
        Configuration configuration, MainWindow mainWindow, Action<AliasEntry> beginRenameAlias,
        Action<AliasFolder, bool> beginRenameFolder)
    {
        _configuration = configuration;
        _mainWindow = mainWindow;
        _beginRenameAlias = beginRenameAlias;
        _beginRenameFolder = beginRenameFolder;
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
            _configuration.Aliases.Add(newAlias);
            _configuration.Save();
            _mainWindow.SetSelection(newAlias, null);
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
                    _configuration.Aliases.Add(imported);
                    _configuration.Save();
                    _mainWindow.SetSelection(imported, null);
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

            if (_mainWindow.SelectedFolder != null) _mainWindow.SelectedFolder.Aliases.Add(cloned);
            else _configuration.Aliases.Add(cloned);

            _configuration.Save();
            _mainWindow.SetSelection(cloned, _mainWindow.SelectedFolder);
            _beginRenameAlias(cloned);
        }

        ImGui.SameLine();
        if (DrawIconButton(FontAwesomeIcon.FolderPlus, buttonSize, "New Folder"))
        {
            var newFolder = new AliasFolder { Name = "New Folder" };
            _configuration.Folders.Add(newFolder);
            _configuration.Save();
            _beginRenameFolder(newFolder, true);
        }

        ImGui.SameLine();
        if (DrawIconButton(FontAwesomeIcon.Trash, buttonSize,
                           canDelete ? "Delete Selected" : "Hold Shift + Ctrl to delete",
                           disabled: !canDelete || _mainWindow.SelectedAlias == null))
        {
            if (_mainWindow.SelectedFolder != null) _mainWindow.SelectedFolder.Aliases.Remove(_mainWindow.SelectedAlias!);
            else _configuration.Aliases.Remove(_mainWindow.SelectedAlias!);

            _mainWindow.SetSelection(null, null);
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

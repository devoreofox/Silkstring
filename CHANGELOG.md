# Changelog

## v0.0.0.3

### Added
- Folder organization groups aliases into collapsible folders with drag and drop
- Display names allow users to set a friendly label for each alias separate from the trigger command
- Filter bar for quickly finding aliases by name
- Export alias to clipboard via right-click context menu for easy sharing
- Import alias from clipboard prompts for a display name on import
- Clone alias duplicates the selected alias into the same folder
- Settings gear in the title bar

### Changed
- UI completely redesigned as a split panel layout - selector on the left, editor on the right
- `EditWindow` removed; editing now happens inline in the main window
- Alphabetical sorting by display name, falling back to trigger name

### Notes
- Existing aliases are preserved on update
- Shift + Ctrl is still required to delete an alias or command

## v0.0.0.2

### Added
- Multiple alias names via `|` separator (e.g. hello|hi|hey)
- Multiline command entry mode - toggle in settings between list view and multiline textbox

### Fixed
- Save and Cancel buttons in the Edit window are now anchored to the bottom and always visible

## v0.0.0.1 - Initial Release

### Added
- Command alias system - define a custom alias that executes one or more chat commands in sequence
- In-game GUI via `/silkstring` for managing aliases
- Edit window for adding, reordering, and removing commands per alias
- Enable/disable toggle per alias without deleting it
- Shift + Ctrl required to delete an alias or command - prevents accidental deletion
- Configurable delay between command execution (0 - 1000ms, default 100ms)
- Protected system commands cannot be used as alias names
- Commands can be entered with or without a leading `/`
- Settings window accessible via the Settings button or the cog icon in `/xlplugins`
# Changelog

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
# Silkstring

A Dalamud plugin for FFXIV that lets you define custom command aliases that expand into one or more chat commands, executed in sequence.

## Features

- Define custom command aliases via an in-game GUI
- Each alias can execute multiple commands in order
- Configurable delay between commands
- Enable/disable aliases without deleting them
- Protected system commands cannot be overwritten

## Installation

1. Open XIVLauncher settings
2. Navigate to **Dalamud** → **Custom Plugin Repositories**
3. Add the following URL:
```
https://raw.githubusercontent.com/devoreofox/Silkstring/main/repo.json
```
4. Open `/xlplugins` in-game and search for **Silkstring**

## Usage

Type `/silkstring` in-game to open the alias manager.

### Managing Aliases

- **Add Alias** - creates a new alias entry
- **Edit** - opens the edit window to add or modify commands for that alias
- **Delete** - hold **Shift + Ctrl** and click Delete to remove an alias
- **Enabled toggle** - enable or disable an alias without deleting it

### Editing an Alias

The edit window lets you define the commands that run when the alias is typed. Each command is a separate entry. Commands run in order from top to bottom with a configurable delay between them.

- Click **+** (Add Command) to add a new command
- Hold **Shift + Ctrl** and click Delete to remove a command
- Click **Save** to apply changes
- Click **Cancel** to discard changes

### Settings

Click **Settings** in the main window or use the cog icon in `/xlplugins` to open the settings panel. You can configure the delay between commands in milliseconds (0 - 1000ms, default 100ms).

### Example

Create an alias named `hello` with the following commands:
```
say Hello!
say How are you?
emote waves
```

Typing `/hello` in chat will execute all three commands in sequence.

## Notes

- Commands can be entered with or without a leading `/`
- The following commands cannot be used as alias names: `silkstring`, `xlplugins`, `xlsettings`, `xldclose`, `xldev`
- Aliases do not work inside macros by design

## License

AGPL-3.0-or-later

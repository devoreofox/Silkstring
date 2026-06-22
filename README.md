# Silkstring

A Dalamud plugin for FFXIV that lets you define custom command aliases. Each alias expands into one or more lines that run in sequence: chat commands, chat messages, or a mix of both.

## Features

- Define custom aliases through an in-game GUI
- Run multiple lines per alias, in order, with a configurable delay between them
- Mix game commands and chat messages in a single alias
- Insert live game state with variables such as `{character}` and `{job}`
- Give an alias several triggers using a `|` separator
- Organize aliases into collapsible folders with drag and drop
- Set a friendly display name for an alias, separate from its trigger
- Filter aliases by name, and import, export, or clone them
- Enable or disable an alias without deleting it
- Built-in help window with a live command tester
- Cycle detection warns you if aliases would trigger each other in a loop

## Installation

1. Open XIVLauncher settings
2. Navigate to **Dalamud**, then **Custom Plugin Repositories**
3. Add the following URL:
```
https://raw.githubusercontent.com/devoreofox/Silkstring/main/repo.json
```
4. Open `/xlplugins` in-game and search for **Silkstring**

## Usage

Type `/silkstring` to open the alias manager, or `/silkstring help` (or the info icon in the title bar) to open the help window.

The manager is a split panel: the list of aliases and folders is on the left, and the editor for the selected alias is on the right.

### Creating and editing aliases

The buttons along the bottom of the left panel let you:

- **New Alias** (plus icon): create a new alias and name it
- **Import from Clipboard**: paste an alias that was exported elsewhere
- **Clone Alias**: duplicate the selected alias
- **New Folder**: create a folder to group aliases
- **Delete**: remove the selected alias (hold **Shift + Ctrl** to enable the button)

Select an alias on the left to edit it on the right. The editor has a checkbox to enable or disable the alias, a field for its trigger, and the list of lines it runs.

- Click **+** to add a line
- Hold **Shift + Ctrl** and click the trash icon to remove a line
- Changes are saved automatically

Right-click an alias for more options: **Rename** (sets a display name), **Clear Display Name**, and **Export to Clipboard**. Right-click a folder to **Rename** or **Delete** it. Deleting a folder keeps its aliases and moves them back to the unsorted list.

### Triggers

A trigger is what you type in chat to fire the alias. Triggers are defined without a leading slash but typed with one. For example, an alias with the trigger `hello` is fired by typing `/hello`.

You can give one alias several triggers by separating them with a pipe:
```
mew|meow|mreow
```
Typing any of `/mew`, `/meow`, or `/mreow` fires the same alias.

Triggers cannot contain spaces, and should not collide with built-in game or Dalamud commands.

### Commands and chat messages

Each line of an alias is sent exactly as written:

- A line that starts with `/` runs as a game command
- A line that does not start with `/` is sent as a chat message to whatever channel you currently have active (say, party, free company, and so on)

```
/say Hello!
/emote waves
good luck, everyone!
```
This runs `/say Hello!`, then `/emote waves`, then sends "good luck, everyone!" to your current channel.

### Variables

Variables let you insert live game values into a line using curly brace syntax. They are case-insensitive and resolved at the moment the alias fires, so they always reflect your current state. If a value cannot be read (for example when you are not logged in) the variable is left as written rather than blanked out.

```
/say I am {character}, a level {level} {job} from {world}!
```

Currently supported variables: `{character}`, `{job}`, `{level}`, `{world}`. The Variables tab of the help window lists them with their current values, and the command tester at the top of the help window shows resolved output as you type.

### Folders

Create a folder with the **New Folder** button, then drag aliases into or out of it. Folders can be collapsed, renamed, and deleted from their right-click menu.

### Multiline entry

By default each line of an alias is its own row. You can switch to a single multiline text box in the settings, with one line per row, which is handy for pasting or editing longer aliases.

### Settings

Open settings with the cog icon in the Silkstring title bar, or the cog next to Silkstring in `/xlplugins`. You can set the delay between lines in milliseconds (0 to 1000, default 100), and toggle multiline command entry.

## Notes

- Lines starting with `/` run as game commands; lines without a `/` are sent to your active chat channel
- Existing aliases are migrated automatically on update, and a backup of your previous configuration is saved beside it before any changes are made
- The following names cannot be used as triggers: `silkstring`, `xlplugins`, `xlsettings`, `xldclose`, `xldev`
- Aliases do not work inside macros by design

## License

AGPL-3.0-or-later

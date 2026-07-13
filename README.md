# Silkstring

A Dalamud plugin for FFXIV that lets you define custom command aliases. Each alias expands into one or more lines that run in sequence: chat commands, chat messages, or a mix of both.

## Features

- Define custom aliases through an in-game GUI
- Run multiple lines per alias, in order, with a configurable delay between them
- Mix game commands and chat messages in a single alias
- Leave notes in an alias with `#` comment lines
- Insert live game state with variables such as `{character}` and `{job}`
- Define your own variables and change them on the fly from inside an alias
- Give an alias several triggers using a `|` separator
- Organize aliases into collapsible folders with drag and drop
- Set a friendly display name for an alias, separate from its trigger
- Filter aliases by name, and import, export, or clone them
- Enable or disable an alias without deleting it
- Built-in help window with a live command tester
- Syntax highlighting in the multiline editor, with customizable colors
- Run commands only when a condition is true with `if` / `else if` / `else` blocks
- Hold an alias until a condition is met with `:until`, and stop running aliases with `/silkstring cancel`
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

Type `/silkstring` to open the alias manager, or `/silkstring help` (or the info icon in the title bar) to open the help window. Use `/silkstring changelog` (or the scroll icon in the title bar) to view release notes; the changelog also appears automatically the first time you launch a new version.

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

### Comments

Any line that starts with `#` is a comment. It is left out when the alias runs and shown in its own color in the editor, so you can label an alias or leave yourself notes:

```
# heal the party if anyone is hurt
if ({hpp} < 50) {
    /ac Medica II
}
```

Only whole lines are comments. A `#` partway through a line is treated as normal text, so `/say I'm #1!` still sends as written. You can change the comment color in the settings.

### Variables

Variables let you insert live game values into a line using curly brace syntax. They are case-insensitive and resolved at the moment the alias fires, so they always reflect your current state. If a value cannot be read (for example when you are not logged in) the variable is left as written rather than blanked out.

```
/say I am {character}, a level {level} {job} from {world}!
```

Silkstring includes variables for your character, job, HP and MP, combat state, target, currency, emote and pose state, the time and date, and more, and the list keeps growing. For the full, always-current list with live values, open the Variables tab of the help window (`/silkstring help`). The command tester at the top of that window shows resolved output as you type.

The time variables come in two flavors: `{time}` (like 3:45 PM) is for showing the time, while `{time24}` (like 15:45), `{date}`, `{hour}`, and `{minute}` are made for comparing, so you can gate lines by the clock (see Conditionals below). There is also `{daypart}`, which is simply morning, afternoon, evening, or night, and `{utc}` and `{utcdate}` for UTC (server) time.

You can also define your own variables with `/silkstring variables` and use them just like the built-in ones (see User Variables below).

### Parameters

Aliases can take arguments. Anything typed after the trigger becomes a numbered argument (starting at `0`) that you insert with curly braces:

```
Alias "greet" line: /wave {0}
Type in chat: /greet Friend
Runs: /wave Friend
```

Wrap a value in quotes to keep multiple words as a single argument: `/greet "Jane Doe"` makes `{0}` equal `Jane Doe`.

You can also pull ranges of arguments. Range ends are exclusive, the same as C# ranges:

| Token | Meaning |
| --- | --- |
| `{0}`, `{1}`, ... | a single argument by position (starts at 0) |
| `{n..}` | argument n through the end |
| `{..n}` | the start up to (not including) argument n |
| `{n..m}` | argument n up to (not including) argument m |
| `{*}` | all arguments |

If you reference an argument that was not supplied, it is left as written (e.g. `{3}` stays `{3}`).

### Conditionals

Aliases can run commands only when a condition is true, using `if` blocks with braces:

```
if ({hpp} < 50) {
    /ac Cure
}
else {
    /say all good
}
```

Everything inside the braces after `if (...)` runs only when the condition holds. Add an `else { }` block for the other case, or chain several checks with `else if (...)`, which runs the first branch whose condition is true:

```
if ({hpp} < 25) {
    /ac Benediction
}
else if ({hpp} < 50) {
    /ac Cure
}
else {
    /say all good
}
```

Blocks can hold multiple lines and can be nested inside each other. Indent with Tab and remove indentation with Shift + Tab to keep nested blocks readable.

A condition compares values with `==`, `!=`, `<`, `>`, `<=`, `>=`, and you can combine comparisons with `&&` (and) and `||` (or). Either side can be a variable, a parameter, or plain text, so conditions can react to game state or to what you typed:

```
if ({incombat} && {hpp} < 50) {
if ({job} == WHM || {job} == SCH) {
if ({0} == on) {
if ({time24} >= 17:00 || {time24} <= 05:00) {
```

That last line reacts to the clock: because `{time24}` and `{date}` are written with leading zeros (like 09:00 and 2026-07-09), they sort in the right order and can be compared with `<`, `>`, `<=`, and `>=`. This is why `{time24}` is the one to compare against, while `{time}` (like 3:45 PM) is just for showing.

Text comparisons are case-insensitive, and numbers compare as numbers. The editor colors a brace or condition red when it is not valid, marks the line, and lists any open block or condition it cannot understand above the editor.

You can stop the rest of an alias early with a `:return` line, which is handy as a guard near the top:

```
if ({incombat}) {
    :return
}
/wave
good luck, everyone!
```

You can also pause between lines with `:wait`, followed by a number of seconds:

```
/ac Raise
:wait 2
/say Up you get!
```

The wait accepts decimals (`:wait 1.5`) and can take its duration from a variable or parameter (`:wait {0}`). Waits are capped at 60 seconds, and a `:wait` inside a condition only pauses when that branch actually runs. Like the other control lines, `:wait` is never sent to chat, and the editor warns you if a duration is invalid.

You can also hold an alias in place until a condition becomes true with `:until`, then let it carry on:

```
/sit
:until {emoting} == false
/say Up you get!
```

`:until` pauses at that line until the condition is true, for example waiting until an emote or pose ends. If the condition never comes true, it gives up after a timeout you can set in the settings and then continues. Add `-unsafe` to wait with no time limit, such as `:until {emoting} == false -unsafe`, but unsafe waits have to be turned on in the settings first. You can stop any running alias at any time with `/silkstring cancel`.

### User Variables

Alongside the built-in variables, you can define your own. Open the Variables window with `/silkstring variables`, type a name, and give it a value. Names can use letters, numbers, and underscores, and cannot reuse a built-in variable name. You can also give each one an optional description, which appears next to it in the Variables tab of the help window.

Your variables work just like the built-in ones: insert them anywhere with `{name}`, and they appear in the Variables tab of the help window.

You can change a variable from inside an alias with `:set`:

```
:set mode raid
:set greeting hi {0}
```

The value is resolved when the line runs, so it can include other variables and parameters, and the result is saved automatically and kept between sessions. A `:set` only affects a variable you have already created. If it names one that does not exist, nothing happens and the alias editor warns you.

Because conditions treat `true` and `false` the same way as the built-in switches, a variable makes a handy on/off flag. Toggle it in one alias with `:set burst true` (or `false`), and check it in another:

```
if ({burst}) {
    /say bursting
}
```

### Folders

Create a folder with the **New Folder** button, then drag aliases into or out of it. Folders can be collapsed, renamed, and deleted from their right-click menu.

### The editor

Aliases are edited in a single multiline text box that highlights your syntax as you type: commands, keywords, variables, parameters, quoted text, and option flags each get their own color, and anything malformed (a bad `:wait`, an unclosed block, an unknown `:set` name, or a broken condition) is shown in red and marked on the line. It shows line numbers, which you can turn off in the settings. Tab indents the current lines and Shift + Tab removes indentation, so you can format blocks quickly.

### Settings

Open settings with the cog icon in the Silkstring title bar, or the cog next to Silkstring in `/xlplugins`. You can set the delay between lines in milliseconds (0 to 1000, default 100), set how long an `:until` waits before giving up, allow unsafe waits that have no time limit, toggle line numbers in the editor, and open the Colors section to recolor the editor and interface to your taste.

## Notes

- Lines starting with `/` run as game commands; lines without a `/` are sent to your active chat channel
- Existing aliases are migrated automatically on update, and a backup of your previous configuration is saved beside it before any changes are made
- The following names cannot be used as triggers: `silkstring`, `xlplugins`, `xlsettings`, `xldclose`, `xldev`
- Aliases do not work inside macros by design

## License

AGPL-3.0-or-later

## Attribution

Silkstring is free software under the AGPL-3.0, and attribution is required. If you fork it, redistribute it, or reuse its code, you must keep the existing copyright and license notices in place and credit the original author (Oreo / devoreofox) with a visible link back to this repository (https://github.com/devoreofox/Silkstring). Please do not remove or hide that attribution.

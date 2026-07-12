# Changelog

## v1.6.2.0 - 2026-07-11
### Added
- Blank lines and indentation in the multiline editor are now kept when you save, so you can space out and indent longer aliases to make them easier to read
- Indented lines are now colored correctly in the editor

### Changed
- The alias editor now lists every problem with an alias at once, each with its line number, instead of stopping at the first one it finds
- Problems are now colored by how serious they are, so warnings stand out from errors
- A cycle warning (an alias that would trigger itself) now shows alongside the other problems, instead of only appearing when you hovered over the trigger box

## v1.6.1.0 - 2026-07-09
### Added
- Time variables: your aliases can now react to the clock. `{time}` shows your local time (like 3:45 PM), `{time24}` shows it in 24-hour form (15:45), and `{date}`, `{day}`, `{hour}`, and `{minute}` fill in the rest. `{daypart}` gives morning, afternoon, evening, or night, so a venue greeter can open with something like `Good {daypart}, welcome in!`
- `{utc}` and `{utcdate}` give the UTC (server) time and date, handy for lining things up with people in other time zones
- The 24-hour and date variables are made to be compared in conditions, so you can gate lines by the clock, for example `:if {time24} >= 17:00` to switch to an evening greeting. Use `{time}` for showing the time and `{time24}` (or `{hour}`) when comparing it

## v1.6.0.0 - 2026-07-09
### Added
- Emote variables: `{emoting}` tells you whether you are holding an emote or pose, `{emote}` gives its name, and `{pose}` gives the pose number. They appear in the Variables tab alongside the other built-ins
- Your own variables can now have a description, which you set in the Variables window and see next to them in the Variables tab of the help window
- Until: add a `:until` line to hold an alias at that point until a condition becomes true, then carry on, for example `:until {emoting} == false`. Handy for waiting on your own state before the next line runs, such as holding a statement until an emote ends
- An `-unsafe` option on `:until` lets it wait with no time limit. It has to be turned on in the settings first, and normal untils use a timeout you can set there too
- A `/silkstring cancel` command stops any aliases that are currently running
- The editor now colors option flags like `-unsafe`, in a color you can change in the settings
- A new Editor tab in the help window shows what each highlight color means

## v1.5.0.0 - 2026-07-02
### Added
- Syntax highlighting in the multiline editor: commands, keywords (`:if`, `:set`, `:wait`), variables, parameters, and quoted text are each given their own color as you type, and anything malformed is shown in red (a bad `:wait` time, an unfinished `:if`, a `:set` for a variable you have not created, a mistyped keyword, or an unclosed `{`)
- Line numbers in the multiline editor, with a setting to turn them off
- A Colors section in the settings to recolor the editor and interface, updating live as you change them

## v1.4.1.0 - 2026-07-01
### Fixed
- Removed TBD and dated changelog (oops)

## v1.4.0.0 - 2026-07-01
### Added
- Waits: add a `:wait` line to pause an alias for a set number of seconds before the next line, for example `:wait 2`. Decimals work too, like `:wait 1.5`
- A `:wait` can take its duration from a variable or parameter, such as `:wait {0}`, and is capped at 60 seconds
- The alias editor warns you when a `:wait` has an invalid duration

## v1.3.0.0 - 2026-06-30
### Added
- Your own variables: create variables in the new Variables window (open it with `/silkstring variables`), give each one a value, and use it in any alias with `{name}`, just like the built-in ones
- Change a variable from inside an alias with `:set`, for example `:set mode raid`. The new value is saved automatically and kept between sessions
- The alias editor warns you when a `:set` points at a variable you have not created yet

## v1.2.0.0 - 2026-06-29
### Added
- Conditionals: run commands only when a condition is true, using `:if` / `:else` / `:endif` blocks. Everything in a block runs only when its condition holds, with an optional `:else` for the false case, and blocks can be nested
- Conditions compare values with `==`, `!=`, `<`, `>`, `<=`, `>=` and combine with `&&` (and) and `||` (or). Either side can be a variable, a parameter, or plain text, for example `:if {hpp} < 50 && {incombat}` or `:if {0} == on`
- The alias editor warns you when a conditional block is left open or a condition cannot be understood

## v1.1.0.0 - 2026-06-27
### Added
- New variables: your HP and MP (including percentages), whether you are in combat, whether you have a target, and your Gil and MGP

## v1.0.1.0 - 2026-06-26
### Fixed
- Pressing Escape while editing in the multiline command box no longer clears what you typed
- Adding a blank command line no longer stops an alias from working

## v1.0.0.0 - 2026-06-23
### Added
- Variables: insert live game info into your commands with `{character}`, `{job}`, `{level}`, `{world}`, and `{homeworld}`. They fill in automatically each time the alias runs, and are left as written if the value can't be read (for example when you are not logged in)
- Parameters: aliases can now take arguments. Type words after the trigger and drop them into your commands with `{0}`, `{1}`, ranges like `{1..}` or `{..2}`, or `{*}` for all of them. Wrap multi-word values in quotes, for example `"Jane Doe"`
- Help window: open it with `/silkstring help` or the info button in the title bar for a live command tester and a reference covering commands, variables, and parameters
- Changelog window: after each update a window shows what is new, with a dropdown to browse previous versions. Reopen it any time with `/silkstring changelog` or the scroll button in the title bar
- Loop protection: Silkstring warns you in the editor when aliases would trigger each other in a loop, and skips any that slip through so they cannot spam commands

### Changed
- Lines that start with `/` run as game commands. Lines without a `/` are now sent as chat messages to whatever channel you currently have active (say, party, free company, and so on)

### Fixed
- Aliases now reliably run every command in order (previously they could repeat the last command)
- Imported aliases no longer clash with the original, so both stay selectable
- Importing invalid clipboard text now shows an error notification instead of doing nothing
- Blank or spaces-only trigger names are no longer accepted
- The filter bar now also matches display names, not just trigger names
- Assorted typos fixed

### Notes
- Your existing aliases are updated automatically so they keep working under the new slash rules
- A backup of your previous configuration is saved first, just in case (for example `Silkstring.v1.backup.json`)

## v0.0.0.3 - 2026-05-16

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
- The separate edit window was removed; editing now happens inline in the main window
- Alphabetical sorting by display name, falling back to trigger name

### Notes
- Existing aliases are preserved on update
- Shift + Ctrl is still required to delete an alias or command

## v0.0.0.2 - 2026-05-10

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

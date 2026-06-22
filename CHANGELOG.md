# Changelog

## v0.0.0.4 - TBD
### Added
- Static cycle detection via a new `AliasValidator` service that performs a depth-first graph traversal across all aliases at authoring time and surfaces the full cycle chain (e.g. `mew → meow → mew`) as a live tooltip on the alias name input in the edit panel
- Runtime recursion guard in `CommandHandler`: a `shouldSkip` predicate checks before each command whether it would trigger an alias currently mid-execution, skipping and logging a warning if so
- `Configuration.GetAliases()` helper that lazily flattens top-level and folder aliases into a single `IEnumerable<AliasEntry>`, replacing duplicated `Concat`/`SelectMany` calls throughout the codebase
- New `CommandResolver` service as the central pre-execution processing pipeline; currently handles variable resolution, with parameters and conditionals planned for future releases. `Resolve` is called in `CommandHandler.ExecuteAsync` before the cycle guard
- Variable substitution using curly brace syntax (e.g. `{job}`, `{level}`): case-insensitive, resolved fresh on every execution, and left as-is if the value cannot be read (e.g. player not logged in). Initial supported variables: `{character}`, `{job}`, `{level}`, `{world}`
- Help window accessible via `/silkstring help` or the new `i` button in the main window title bar, containing:
  - A live command tester that shows resolved output and highlights variable substitutions in green
  - **Commands** section covering alias basics, trigger syntax, command sequencing, variable overview, the macro restriction, and cycle detection behaviour
  - **Variables** section explaining variable syntax and displaying a live reference table of all supported variables with their current resolved values

### Fixed
- Closure capture bug in `CommandHandler`: all scheduled commands were executing the last command in the list due to the loop variable being captured by reference in the async lambda
- Alias validation now trims whitespace and ignores empty entries, preventing inputs like `" "` or `"mew| "` from passing as valid
- Filter bar in the alias select panel now matches against display names in addition to raw command names
- ID collisions on clipboard import: imported aliases no longer share an ID with the original, making both selectable
- Clipboard import failures now surface a toast notification instead of silently failing
- `Log.Error` calls now correctly include the exception object
- Various typos corrected (`occured`, `Seperate`)

### Changed
- Alias lines are now sent exactly as written: lines beginning with `/` run as game commands, while lines without a `/` are sent as chat messages to your currently active channel (say, party, free company, etc.)
- `CommandHandler.ExecuteAsync` now accepts a `shouldSkip` predicate parameter; `ChatInterceptor` tracks currently executing alias trigger names in a `HashSet<string>` and cleans up via a `ContinueWith` continuation on the framework thread
- Folders, aliases, and commands now use a stable `UniqueId` generated via `Interlocked.Increment` instead of `GetHashCode()`
- `Blacklist` switched from `string[]` to `HashSet<string>` with `OrdinalIgnoreCase` for O(1) lookups
- Config saves are now debounced: rapid text input no longer triggers a full save on every keystroke; explicit actions (create, delete, toggle) still save immediately
- `CommandDelay` setter now clamps to `[0, 1000]` regardless of what's in the config file
- `SelectPanelFooter` extracted from `AliasSelectPanel`; footer actions now communicate via callbacks rather than direct state mutation
- `SelectedAlias` in `MainWindow` is now driven by a `SelectionChanged` event rather than a mutable shared property
- Multiline command parser now uses `StringSplitOptions.TrimEntries`
- `CancellationToken` added to command scheduling; unnecessary post-final-command delay removed
- Switched to `IReadOnlyList<T>` in `CommandHandler`

### Removed
- `EditWindow`: no longer wired up anywhere

### Notes
- Existing aliases are automatically migrated on first launch after updating: a leading `/` is added to their commands so they keep behaving as before
- Before migrating, a backup of your previous configuration is saved next to it (e.g. `Silkstring.v1.backup.json`)

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
- `EditWindow` removed; editing now happens inline in the main window
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
using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Silkstring.Services;

namespace Silkstring.Windows;

public class HelpWindow : Window, IDisposable
{
    private enum Tab
    {
        Commands = 0,
        Variables = 1,
    }

    private Tab _selectedTab = Tab.Commands;

    private static readonly Vector4 HeadingColor = new(0.7f, 0.5f, 1.0f, 1.0f);
    private string _testerInput = string.Empty;
    private readonly CommandResolver _resolver;


    public HelpWindow(CommandResolver resolver) : base("Silkstring Help###help")
    {
        _resolver = resolver;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(1000, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        DrawCommandTester();
        ImGui.Separator();
        var scale = ImGui.GetIO().FontGlobalScale;
        var leftWidth = new Vector2(250 * scale, 0);

        ImGui.BeginChild("###helpSelector", leftWidth, true,
                         ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        foreach (Tab t in Enum.GetValues<Tab>())
        {
            if (ImGui.Selectable(t.ToString(), _selectedTab == t))
                _selectedTab = t;
        }

        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("###helpPanel", new Vector2(0, 0), true);

        switch (_selectedTab)
        {
            case Tab.Commands:  DrawCommandsHelp();  break;
            case Tab.Variables: DrawVariablesHelp(); break;
        }

        ImGui.EndChild();
    }

    private void DrawCommandTester()
    {
        ImGui.TextColored(HeadingColor, "Command Tester");
        ImGui.Spacing();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("###testerInput", "Enter a command e.g. /say Hello {character}!", ref _testerInput, 200);
        ImGui.Spacing();

        var resolved = string.IsNullOrWhiteSpace(_testerInput) ? string.Empty : _resolver.Resolve(_testerInput);

        ImGui.Text("Resolved:");
        ImGui.SameLine();

        if (string.IsNullOrWhiteSpace(resolved))
            ImGui.TextDisabled("---");
        else if (resolved == _testerInput)
            ImGui.TextDisabled(resolved);
        else
            ImGui.TextColored(new Vector4(0.4f, 1f, 0.4f, 1f), resolved);
    }

    private void DrawCommandsHelp()
    {
        ImGui.TextColored(HeadingColor, "What is an Alias?");
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextWrapped("An alias lets you create a custom chat command that fires one or more FFXIV commands in sequence. " +
                          "Type your trigger in the chat box just like any other command and Silkstring will intercept it and execute your defined command list.");
        ImGui.Spacing();

        ImGui.TextColored(HeadingColor, "Triggers");
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextWrapped("A trigger is the command you type in chat to activate the alias. Triggers must start with a forward slash when used in chat, " +
                          "but are defined without one in Silkstring.");
        ImGui.Spacing();
        ImGui.BulletText("Triggers cannot contain spaces.");
        ImGui.BulletText("Triggers should not conflict with built-in FFXIV or Dalamud commands.");
        ImGui.Spacing();
        ImGui.TextColored(HeadingColor, "Multiple Triggers");
        ImGui.Spacing();
        ImGui.TextWrapped("You can assign multiple triggers to a single alias by separating them with a pipe character:");
        ImGui.Spacing();
        ImGui.Indent();
        ImGui.TextDisabled("mew|meow|mreow");
        ImGui.Unindent();
        ImGui.Spacing();
        ImGui.TextWrapped("Any of these typed in chat will fire the same alias.");
        ImGui.Spacing();

        ImGui.TextColored(HeadingColor, "Commands");
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextWrapped("Commands are the actions Silkstring executes when your alias is triggered. Each command should be a valid FFXIV chat command including the leading slash.");
        ImGui.Spacing();
        ImGui.BulletText("Commands execute in order from top to bottom.");
        ImGui.BulletText("A configurable delay is applied between each command.");
        ImGui.BulletText("Empty commands are skipped automatically.");
        ImGui.Spacing();
        ImGui.Indent();
        ImGui.TextDisabled("/say Hello!");
        ImGui.TextDisabled("/emote waves");
        ImGui.TextDisabled("/party Ready!");
        ImGui.Unindent();
        ImGui.Spacing();

        ImGui.TextColored(HeadingColor, "Variables");
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextWrapped("Variables let you insert dynamic values into your commands at execution time using curly brace syntax:");
        ImGui.Spacing();
        ImGui.Indent();
        ImGui.TextDisabled("/say Hello, I am {character} and I play {job}!");
        ImGui.Unindent();
        ImGui.Spacing();
        ImGui.TextWrapped("Variables are resolved at the moment the alias fires, so they always reflect your current game state. " +
                          "See the Variables tab for a full list of supported variables.");

        ImGui.TextColored(HeadingColor, "Macros");
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextWrapped("Silkstring aliases are intentionally disabled when called from within FFXIV macros. " +
                          "This is by design to avoid detection by the games systems. " +
                          "Aliases must be typed directly into the chat box to function.");
        ImGui.Spacing();

        ImGui.TextColored(HeadingColor, "Cycles & Recursion");
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextWrapped("A cycle occurs when an alias triggers itself either directly or through a chain of other aliases:");
        ImGui.Spacing();
        ImGui.Indent();
        ImGui.TextDisabled("Direct:   mew → mew");
        ImGui.TextDisabled("Indirect: mew → bark → mew");
        ImGui.Unindent();
        ImGui.Spacing();
        ImGui.TextWrapped("Silkstring detects cycles at authoring time and will warn you in the alias editor if one is found. " +
                          "A runtime guard also prevents cyclic commands from executing even if one slips through, " +
                          "skipping the offending command and continuing with the rest of the sequence.");
    }


    private void DrawVariablesHelp()
    {
        ImGui.TextColored(HeadingColor, "What are Variables?");
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextWrapped("Variables let you insert dynamic values into your commands at execution time. " +
                          "They are resolved at the moment the alias fires, so they always reflect your current game state.");
        ImGui.Spacing();
        ImGui.TextWrapped("Variables use curly brace syntax and are case insensitive:");
        ImGui.Spacing();
        ImGui.Indent();
        ImGui.TextDisabled("/say I am {character} playing as {job} on {world}!");
        ImGui.Unindent();
        ImGui.Spacing();
        ImGui.TextWrapped("If a variable cannot be resolved, for example if you are not logged in-" +
                          "it is left as-is in the command string rather than being replaced with an empty value.");
        ImGui.Spacing();

        ImGui.TextColored(HeadingColor, "Available Variables");
        ImGui.Separator();
        ImGui.Spacing();

        var isLoaded = Plugin.PlayerState.IsLoaded;

        if (ImGui.BeginTable("###variablesTable", 3, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("Variable", ImGuiTableColumnFlags.WidthFixed, 120);
            ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Current Value", ImGuiTableColumnFlags.WidthFixed, 150);
            ImGui.TableHeadersRow();

            DrawVariableRow("{character}", "Your character's name", isLoaded ? Plugin.PlayerState.CharacterName : "Not logged in");
            DrawVariableRow("{job}", "Current job abbreviation", isLoaded ? Plugin.PlayerState.ClassJob.Value.Abbreviation.ToString() : "Not logged in");
            DrawVariableRow("{level}", "Current character level", isLoaded ? Plugin.PlayerState.Level.ToString() : "Not logged in");
            DrawVariableRow("{world}", "Your current world", isLoaded ? Plugin.PlayerState.CurrentWorld.Value.Name.ToString() : "Not logged in");

            ImGui.EndTable();
        }
    }

    private void DrawVariableRow(string variable, string description, string currentValue)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextDisabled(variable);
        ImGui.TableNextColumn();
        ImGui.TextWrapped(description);
        ImGui.TableNextColumn();
        ImGui.Text(currentValue);
    }
}

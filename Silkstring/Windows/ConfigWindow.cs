using System;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace Silkstring.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Silkstring Settings###Config")
    {
        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(250, 150),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        ImGui.Text("Command Delay (ms)");
        var delay = configuration.CommandDelay;
        ImGui.SetNextItemWidth(150);
        if (ImGui.InputInt("###delayInput", ref delay, 10, 100))
        {
            configuration.CommandDelay = delay;
            configuration.MarkDirty();
        }
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        if (ImGui.SliderInt("###delaySlider", ref delay, 0, 1000))
        {
            configuration.CommandDelay = delay;
            configuration.MarkDirty();
        }

        var multiline = configuration.MultilineCommands;
        if (ImGui.Checkbox("Multiline command entry", ref multiline))
        {
            configuration.MultilineCommands = multiline;
            configuration.Save();
        }

        var availableHeight = ImGui.GetContentRegionAvail().Y;
        var buttonHeight = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y + ImGui.GetStyle().WindowPadding.Y * 2 + 4;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + availableHeight - buttonHeight);

        ImGui.Separator();
        var closeButtonWidth = ImGui.CalcTextSize("Close").X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.SetCursorPos(new Vector2(
                               ImGui.GetWindowWidth() - closeButtonWidth - ImGui.GetStyle().WindowPadding.X - 16,
                               ImGui.GetCursorPosY() + 25
                           ));
        if (ImGui.Button("Close"))
        {
            IsOpen = false;
        }
    }
}

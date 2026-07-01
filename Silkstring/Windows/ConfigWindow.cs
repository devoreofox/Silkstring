using System;
using Dalamud.Interface.Windowing;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Silkstring.Models;
using Silkstring.UI;

namespace Silkstring.Windows;


public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private static readonly FieldInfo[] ThemeFields = typeof(ThemeColors).GetFields();
    private static readonly ThemeColors Defaults = new();
    private static string Prettify(string name) => Regex.Replace(name, "(\\B[A-Z])", " $1");

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

        if (ImGui.CollapsingHeader("Colors"))
        {
            void Sync() { Palette.Apply(configuration.Theme); configuration.MarkDirty(); }

            foreach (var field in ThemeFields)
            {
                if (ImGuiComponents.IconButton($"reset{field.Name}", FontAwesomeIcon.Undo))
                {
                    field.SetValue(configuration.Theme, field.GetValue(Defaults));
                    Sync();
                }
                ImGuiUtil.Tooltip("Reset to default");
                ImGui.SameLine();

                var value = (Vector4)field.GetValue(configuration.Theme)!;
                if (ImGui.ColorEdit4($"{Prettify(field.Name)}##{field.Name}", ref value, ImGuiColorEditFlags.NoInputs))
                {
                    field.SetValue(configuration.Theme, value);
                    Sync();
                }
            }

            if (ImGui.Button("Reset colors"))
            {
                configuration.Theme = new ThemeColors();
                Palette.Apply(configuration.Theme);
                configuration.Save();

            }

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

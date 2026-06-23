using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Silkstring.Services;

namespace Silkstring.Windows;

public class ChangelogWindow : Window, IDisposable
{
    private static readonly Vector4 HeadingColor = new(0.7f, 0.5f, 1.0f, 1.0f);

    private readonly IReadOnlyList<ChangelogSection> _sections;
    private readonly string[] _versions;
    private int _selected;

    public ChangelogWindow() : base("Silkstring Changelog###changelog")
    {
        _sections = Changelog.Load();
        _versions = _sections.Select(s => s.Version).ToArray();
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        if (_sections.Count == 0)
        {
            ImGui.TextDisabled("No changelog available.");
            return;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.Combo("###version", ref _selected, _versions, _versions.Length);
        ImGui.Separator();

        ImGui.BeginChild("###changelogBody");
        DrawBody(_sections[_selected].Body);
        ImGui.EndChild();
    }

    private static void DrawBody(string body)
    {
        foreach (var raw in body.Split('\n'))
        {
            var line = raw.Replace("`", "").Replace("**", "").TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                ImGui.Spacing();
            }
            else if (line.StartsWith("### "))
            {
                ImGui.Spacing();
                ImGui.TextColored(HeadingColor, line[4..]);
                ImGui.Separator();
            }
            else if (line.StartsWith("  - "))
            {
                ImGui.Indent();
                ImGui.Bullet();
                ImGui.SameLine();
                ImGui.TextWrapped(line[4..]);
                ImGui.Unindent();
            }
            else if (line.StartsWith("- "))
            {
                ImGui.Bullet();
                ImGui.SameLine();
                ImGui.TextWrapped(line[2..]);
            }
            else
            {
                ImGui.TextWrapped(line);
            }
        }
    }
}

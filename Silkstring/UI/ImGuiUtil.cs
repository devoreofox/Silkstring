using Dalamud.Bindings.ImGui;

namespace Silkstring.UI;

public static class ImGuiUtil
{
    public static void Tooltip(string text, bool allowWhenDisabled = false)
    {
        var flags = allowWhenDisabled ? ImGuiHoveredFlags.AllowWhenDisabled : ImGuiHoveredFlags.None;
        if (ImGui.IsItemHovered(flags)) ImGui.SetTooltip(text);
    }
}

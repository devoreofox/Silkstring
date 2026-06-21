using Dalamud.Bindings.ImGui;

namespace Silkstring.UI;

public sealed class InlineRename<T> where T : class
{
    private T? _target;
    private string _buffer = string.Empty;
    private bool _focus;

    public bool IsRenaming(T item) => _target == item;

    public void Begin(T item, string initial)
    {
        _target = item;
        _buffer = initial;
        _focus = true;
    }

    public bool Draw(string id, out string result)
    {
        if (_focus)
        {
            ImGui.SetKeyboardFocusHere();
            _focus = false;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText(id, ref _buffer, 100);
        result = _buffer;
        if (!ImGui.IsItemDeactivated()) return false;
        _target = null;
        return true;
    }
}

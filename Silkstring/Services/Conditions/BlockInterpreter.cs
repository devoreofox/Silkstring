using System;
using System.Collections.Generic;
using System.Globalization;

namespace Silkstring.Services.Conditions;

internal enum BlockKind
{
    Command,
    If,
    Else,
    EndIf,
    Set,
    Wait,
    Until,
    Comment
}

internal sealed class BlockInterpreter
{
    private readonly Stack<Block> _stack = new();
    public bool Active => _stack.Count == 0 || _stack.Peek().Active;

    public static (BlockKind Kind, string Expression) Classify(string line)
    {
        if (line.StartsWith("#")) return (BlockKind.Comment, line);
        if (line.StartsWith(":if ", StringComparison.OrdinalIgnoreCase)) return (BlockKind.If, line[4..]);
        if (line.Equals(":else", StringComparison.OrdinalIgnoreCase)) return (BlockKind.Else, "");
        if (line.Equals(":endif", StringComparison.OrdinalIgnoreCase)) return (BlockKind.EndIf, "");
        if (line.StartsWith(":set ", StringComparison.OrdinalIgnoreCase)) return (BlockKind.Set, line[5..]);
        if (line.StartsWith(":wait ", StringComparison.OrdinalIgnoreCase)) return (BlockKind.Wait, line[6..]);
        if (line.StartsWith(":until ", StringComparison.OrdinalIgnoreCase)) return (BlockKind.Until, line[7..]);
        return (BlockKind.Command, line);
    }

    public static (bool Unsafe, string Condition) ParseUntil(string expression)
    {
        var trimmed = expression.Trim();
        if (trimmed.EndsWith(" -unsafe", StringComparison.OrdinalIgnoreCase)) return (true, trimmed[..^8].TrimEnd());
        return (false, trimmed);
    }

    public static (string Name, string Value) ParseSet(string expression)
    {
        var trimmed = expression.Trim();
        var space = trimmed.IndexOf(' ');
        return space < 0 ? (trimmed, "") : (trimmed[..space], trimmed[(space + 1)..].Trim());
    }

    public static bool TryParseDuration(string text, out int milliseconds)
    {
        milliseconds = 0;
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) || seconds < 0)
            return false;
        milliseconds = (int)Math.Min(seconds * 1000, 60000);
        return true;
    }

    public void EnterIf(bool conditionMet)
    {
        var parentActive = Active;
        _stack.Push(new Block { ParentActive = parentActive, ConditionTrue = conditionMet, Active = parentActive && conditionMet });
    }

    public void Else()
    {
        if (_stack.Count == 0) return;
        var block = _stack.Peek();
        block.Active = block.ParentActive && !block.ConditionTrue;
    }

    public void EndIf()
    {
        if (_stack.Count > 0) _stack.Pop();
    }

    private sealed class Block
    {
        public bool ParentActive;
        public bool ConditionTrue;
        public bool Active;
    }
}

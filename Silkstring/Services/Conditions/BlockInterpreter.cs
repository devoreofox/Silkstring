using System;
using System.Collections.Generic;

namespace Silkstring.Services.Conditions;

internal enum BlockKind
{
    Command,
    If,
    Else,
    EndIf
}

internal sealed class BlockInterpreter
{
    private readonly Stack<Block> _stack = new();
    public bool Active => _stack.Count == 0 || _stack.Peek().Active;

    public static (BlockKind Kind, string Expression) Classify(string line)
    {
        if (line.StartsWith(":if ", StringComparison.OrdinalIgnoreCase)) return (BlockKind.If, line[4..]);
        if (line.Equals(":else", StringComparison.OrdinalIgnoreCase)) return (BlockKind.Else, "");
        if (line.Equals(":endif", StringComparison.OrdinalIgnoreCase)) return (BlockKind.EndIf, "");
        return (BlockKind.Command, line);
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

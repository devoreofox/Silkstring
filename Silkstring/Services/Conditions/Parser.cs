using System.Collections.Generic;

namespace Silkstring.Services.Conditions;

internal sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;
    private string? _error;
    public Parser(List<Token> tokens) => _tokens = tokens;

    public bool TryParse(out ConditionNode? node, out string? error)
    {
        node = ParseOr();
        if (_error == null && Peek is { } leftover) _error = $"Unexpected '{leftover.Text}'";
        error = _error;
        if (_error != null) node = null;
        return _error == null;
    }

    private Token? Peek => _pos < _tokens.Count ? _tokens[_pos] : null;
    private void Next() => _pos++;
    private bool IsOp(string s) => Peek is { Kind: TokenKind.Op } t && t.Text == s;
    private static bool IsComparison(string s) => s is "==" or "!=" or "<" or ">" or "<=" or ">=";

    private ConditionNode? ParseOr()
    {
        var n = ParseAnd();
        while (_error == null && IsOp("||")) { Next(); n = new OrNode(n!, ParseAnd()!); }
        return n;
    }
    private ConditionNode? ParseAnd()
    {
        var n = ParseCmp();
        while (_error == null && IsOp("&&")) { Next(); n = new AndNode(n!, ParseCmp()!); }
        return n;
    }
    private ConditionNode? ParseCmp()
    {
        if (Peek is { Kind: TokenKind.LParen })
        {
            Next();
            var n = ParseOr();
            if (_error != null) return null;
            if (Peek is not { Kind: TokenKind.RParen }) { _error = "Expected ')'"; return null; }
            Next();
            return n;
        }
        var left = ExpectOperand();
        if (_error != null) return null;
        if (Peek is { Kind: TokenKind.Op } op && IsComparison(op.Text))
        {
            Next();
            var right = ExpectOperand();
            if (_error != null) return null;
            return new CmpNode(left!, op.Text, right!);
        }
        return new BareNode(left!);
    }
    private string? ExpectOperand()
    {
        if (Peek is not { Kind: TokenKind.Operand } t) { _error = "Expected a value"; return null; }
        Next();
        return t.Text;
    }
}

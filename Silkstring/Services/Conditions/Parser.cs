using System.Collections.Generic;

namespace Silkstring.Services.Conditions;

internal sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;

    public Parser(List<Token> tokens) =>  _tokens = tokens;

    private Token? Peek => _pos < _tokens.Count ? _tokens[_pos] : null;
    private void Next() => _pos++;
    private bool IsOp(string s) => Peek is { Kind: TokenKind.Op } t && t.Text == s;
    private static bool IsComparison(string s) => s is "==" or "!=" or "<" or ">" or "<=" or ">=";

    public ConditionNode Parse()
    {
        var n = ParseOr();
        if (Peek is { } leftover) throw new ConditionException($"Unexpected '{leftover.Text}'");
        return n;
    }

    private ConditionNode ParseOr()
    {
        var n = ParseAnd();
        while (IsOp("||")) { Next(); n = new OrNode(n, ParseAnd()); }
        return n;
    }

    private ConditionNode ParseAnd()
    {
        var n = ParseCmp();
        while (IsOp("&&")) { Next(); n = new AndNode(n, ParseCmp()); }
        return n;
    }

    private ConditionNode ParseCmp()
    {
        if (Peek is { Kind: TokenKind.LParen })
        {
            Next();
            var n = ParseOr();
            if (Peek is not { Kind: TokenKind.RParen }) throw new ConditionException("Expected ')'");
            Next();
            return n;
        }

        var left = ExpectOperand();
        if (Peek is { Kind: TokenKind.Op } op && IsComparison(op.Text))
        {
            Next();
            return new CmpNode(left, op.Text, ExpectOperand());
        }
        return new BareNode(left);
    }

    private string ExpectOperand()
    {
        if (Peek is not { Kind: TokenKind.Operand } t) throw new ConditionException("Expected a value");
        Next();
        return t.Text;
    }
}

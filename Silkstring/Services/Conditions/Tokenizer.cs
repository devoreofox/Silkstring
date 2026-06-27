using System.Collections.Generic;

namespace Silkstring.Services.Conditions;

internal enum TokenKind { Operand, Op, LParen, RParen }
internal readonly record struct Token(TokenKind Kind, string Text);
internal static class Tokenizer
{
    public static List<Token> Tokenize(string expr)
    {
        var tokens = new List<Token>();
        var i = 0;
        while (i < expr.Length)
        {
            var c = expr[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (c == '(') { tokens.Add(new(TokenKind.LParen, "(")); i++; continue; }
            if (c == ')') { tokens.Add(new(TokenKind.RParen, ")")); i++; continue; }

            if (i + 1 < expr.Length)
            {
                var two = expr.Substring(i, 2);
                if (two is "&&" or "||" or "==" or "!=" or "<=" or ">=") { tokens.Add(new(TokenKind.Op, two)); i += 2; continue; }
            }

            if (c is '<' or '>') { tokens.Add(new(TokenKind.Op, c.ToString())); i++; continue; }
            if (c is '&' or '|' or '=' or '!') throw new ConditionException($"Unexpected '{c}'");

            if (c == '"')
            {
                var end = expr.IndexOf('"', i + 1);
                if (end < 0) throw new ConditionException("Unterminated Quote");
                tokens.Add(new(TokenKind.Operand, expr[(i + 1)..end]));
                i = end + 1;
                continue;
            }

            var start = i;
            while (i < expr.Length && !char.IsWhiteSpace(expr[i]) && expr[i] is not ('(' or ')' or '"' or '&' or '|' or '=' or '!' or '<' or '>')) i++;
            tokens.Add(new(TokenKind.Operand, expr[start..i]));
        }

        return tokens;
    }
}

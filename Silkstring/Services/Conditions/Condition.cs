namespace Silkstring.Services.Conditions;

internal static class Condition
{
    public static bool TryParse(string expression, out ConditionNode? node, out string? error)
    {
        node = null;
        if (!Tokenizer.TryTokenize(expression, out var tokens, out error)) return false;
        return new Parser(tokens).TryParse(out node, out error);
    }
}

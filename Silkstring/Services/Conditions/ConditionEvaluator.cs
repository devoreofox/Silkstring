using System;
using System.Collections.Generic;
using System.Globalization;
namespace Silkstring.Services.Conditions;

public sealed class ConditionEvaluator
{
    private readonly Func<string, IReadOnlyList<string>, string> _resolve;

    public ConditionEvaluator(Func<string, IReadOnlyList<string>, string> resolve) => _resolve = resolve;

    public bool Evaluate(string expression, IReadOnlyList<string> args)
    {
        var ast = new Parser(Tokenizer.Tokenize(expression)).Parse();
        return Eval(ast, args);
    }

    private bool Eval(ConditionNode node, IReadOnlyList<string> args) => node switch
    {
        OrNode o => Eval(o.L, args) || Eval(o.R, args),
        AndNode a => Eval(a.L, args) && Eval(a.R, args),
        CmpNode c => Compare(_resolve(c.L, args), c.Op, _resolve(c.R, args)),
        BareNode b => string.Equals(_resolve(b.Value, args), "true", StringComparison.OrdinalIgnoreCase),
        _ => false
    };

    private static bool Compare(string left, string op, string right)
    {
        var numeric = double.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out var ln)
                      & double.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out var rn);
        return op switch
        {
            "==" => numeric ? ln == rn : string.Equals(left, right, StringComparison.OrdinalIgnoreCase),
            "!=" => numeric ? ln != rn : !string.Equals(left, right, StringComparison.OrdinalIgnoreCase),
            "<" => numeric ? ln < rn : string.Compare(left, right, StringComparison.OrdinalIgnoreCase) < 0,
            ">" => numeric ? ln > rn : string.Compare(left, right, StringComparison.OrdinalIgnoreCase) > 0,
            "<=" => numeric ? ln <= rn : string.Compare(left, right, StringComparison.OrdinalIgnoreCase) <= 0,
            ">=" => numeric ? ln >= rn : string.Compare(left, right, StringComparison.OrdinalIgnoreCase) >= 0,
            _ => false

        };
    }

}

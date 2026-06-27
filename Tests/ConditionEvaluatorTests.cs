using Silkstring.Services.Conditions;

public class ConditionEvaluatorTests
{
    private static ConditionEvaluator Make()
    {
        var vars = new Dictionary<string, string>
        {
            ["{hpp}"] = "40",
            ["{job}"] = "WHM",
            ["{incombat}"] = "true",
            ["{title}"] = "white mage",
        };
        return new ConditionEvaluator((token, args) =>
        {
            if (vars.TryGetValue(token, out var v)) return v;
            if (token.Length > 2 && token[0] == '{' && token[^1] == '}' && int.TryParse(token[1..^1], out var i) && i < args.Count) return args[i];
            return token;
        });
    }

    [Theory]
    [InlineData("{hpp} < 50", true)]
    [InlineData("{hpp} >= 50", false)]
    [InlineData("{hpp}<50", true)]
    [InlineData("{hpp} == 40", true)]
    [InlineData("{hpp} != 50", true)]
    [InlineData("{job} == whm", true)]
    [InlineData("{job} != BLM", true)]
    [InlineData("{job} == \"WHM\"", true)]
    [InlineData("{title} == \"white mage\"", true)]
    [InlineData("{incombat}", true)]
    [InlineData("{hpp} >= 40 && {hpp} <= 90", true)]
    [InlineData("{hpp} > 50 && {incombat}", false)]
    [InlineData("{hpp} > 50 || {job} == WHM", true)]
    [InlineData("{incombat} || {hpp} > 50 && {hpp} > 90", true)]
    [InlineData("({hpp} < 50 || {hpp} > 90) && {incombat}", true)]
    public void Evaluates(string expr, bool expected) => Assert.Equal(expected, Make().Evaluate(expr, Array.Empty<string>()));

    [Theory]
    [InlineData("on", true)]
    [InlineData("off", false)]
    public void ResolvesParameters(string arg, bool expected) => Assert.Equal(expected, Make().Evaluate("{0} == on", new[] { arg }));

    [Theory]
    [InlineData("{hpp} <=")]
    [InlineData("({hpp} < 50")]
    [InlineData("")]
    [InlineData("&&")]
    [InlineData("{a} {b}")]
    public void ThrowsOnMalformed(string expr) => Assert.Throws<ConditionException>(() => Make().Evaluate(expr, Array.Empty<string>()));
}

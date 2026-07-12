using Silkstring.Services.Conditions;

public class BlockInterpreterTests
{
    [Theory]
    [InlineData("/say hi", "Command", "/say hi")]
    [InlineData(":ifx", "Command", ":ifx")]
    [InlineData(":set foo bar", "Set", "foo bar")]
    [InlineData(":SET foo bar", "Set", "foo bar")]
    [InlineData(":set", "Set", "")]
    [InlineData(":wait 2", "Wait", "2")]
    [InlineData(":WAIT 1.5", "Wait", "1.5")]
    [InlineData(":wait", "Wait", "")]
    [InlineData(":until {x}", "Until", "{x}")]
    [InlineData(":return", "Return", "")]
    public void Classifies(string line, string kind, string expression)
    {
        var (k, e) = BlockInterpreter.Classify(line);
        Assert.Equal(kind, k.ToString());
        Assert.Equal(expression, e);
    }

    [Theory]
    [InlineData("foo bar", "foo", "bar")]
    [InlineData("foo", "foo", "")]
    [InlineData("foo   bar baz", "foo", "bar baz")]
    [InlineData("  foo   bar  ", "foo", "bar")]
    [InlineData("greet hi {0}", "greet", "hi {0}")]
    public void ParsesSet(string expression, string name, string value)
    {
        var (n, v) = BlockInterpreter.ParseSet(expression);
        Assert.Equal(name, n);
        Assert.Equal(value, v);
    }

    [Theory]
    [InlineData("2", true, 2000)]
    [InlineData("1.5", true, 1500)]
    [InlineData("0", true, 0)]
    [InlineData("120", true, 60000)]
    [InlineData("-1", false, 0)]
    [InlineData("abc", false, 0)]
    [InlineData("{0}", false, 0)]
    [InlineData("", false, 0)]
    public void ParsesDuration(string text, bool expected, int milliseconds)
    {
        Assert.Equal(expected, BlockInterpreter.TryParseDuration(text, out var ms));
        Assert.Equal(milliseconds, ms);
    }
}

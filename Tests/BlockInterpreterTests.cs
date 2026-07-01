using Silkstring.Services.Conditions;

public class BlockInterpreterTests
{
    [Theory]
    [InlineData(":if {hp} < 50", "If", "{hp} < 50")]
    [InlineData(":IF {hp}", "If", "{hp}")]
    [InlineData(":else", "Else", "")]
    [InlineData(":endif", "EndIf", "")]
    [InlineData("/say hi", "Command", "/say hi")]
    [InlineData(":ifx", "Command", ":ifx")]
    [InlineData(":set foo bar", "Set", "foo bar")]
    [InlineData(":SET foo bar", "Set", "foo bar")]
    [InlineData(":set", "Command", ":set")]
    [InlineData(":wait 2", "Wait", "2")]
    [InlineData(":WAIT 1.5", "Wait", "1.5")]
    [InlineData(":wait", "Command", ":wait")]
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

    [Fact]
    public void EmptyIsActive() => Assert.True(new BlockInterpreter().Active);

    [Fact]
    public void TrueIfRuns()
    {
        var b = new BlockInterpreter();
        b.EnterIf(true);
        Assert.True(b.Active);
    }

    [Fact]
    public void FalseIfSuppresses()
    {
        var b = new BlockInterpreter();
        b.EnterIf(false);
        Assert.False(b.Active);
    }

    [Fact]
    public void ElseRunsWhenIfFalse()
    {
        var b = new BlockInterpreter();
        b.EnterIf(false);
        b.Else();
        Assert.True(b.Active);
    }

    [Fact]
    public void ElseSkippedWhenIfTrue()
    {
        var b = new BlockInterpreter();
        b.EnterIf(true);
        b.Else();
        Assert.False(b.Active);
    }

    [Fact]
    public void EndIfRestoresOuterScope()
    {
        var b = new BlockInterpreter();
        b.EnterIf(false);
        b.EndIf();
        Assert.True(b.Active);
    }

    [Fact]
    public void NestedIfInactiveWhenParentInactive()
    {
        var b = new BlockInterpreter();
        b.EnterIf(false);
        b.EnterIf(true);
        Assert.False(b.Active);
        b.EndIf();
        Assert.False(b.Active);
        b.EndIf();
        Assert.True(b.Active);
    }

    [Fact]
    public void NestedIfActiveWhenParentActive()
    {
        var b = new BlockInterpreter();
        b.EnterIf(true);
        b.EnterIf(true);
        Assert.True(b.Active);
        b.EndIf();
        Assert.True(b.Active);
    }

    [Fact]
    public void OrphanElseAndEndIfAreIgnored()
    {
        var b = new BlockInterpreter();
        b.Else();
        b.EndIf();
        Assert.True(b.Active);
    }
}

using Silkstring.Services;

public class ArgumentParserTests
{
    [Theory]
    [InlineData("a b c", "a|b|c")]
    [InlineData("\"a b\" c", "a b|c")]
    [InlineData("a   b", "a|b")]
    [InlineData("one \"two three\" four", "one|two three|four")]
    [InlineData("\"ab", "\"ab")]
    public void Parses(string input, string expected) => Assert.Equal(expected.Split('|'), ArgumentParser.Parse(input));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyInputYieldsNoArgs(string input) => Assert.Empty(ArgumentParser.Parse(input));
}

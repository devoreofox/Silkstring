using Silkstring.Models;
using Silkstring.Services.Variables;

public class VariableRenamerTests
{
    [Theory]
    [InlineData("/say {mode} now", "/say {phase} now")]
    [InlineData("/say {MODE} {Mode}", "/say {phase} {phase}")]
    [InlineData("if ({mode}) {", "if ({phase}) {")]
    [InlineData("    :set mode raid", "    :set phase raid")]
    [InlineData(":SET Mode {mode}", ":SET phase {phase}")]
    [InlineData(":set mode", ":set phase")]
    [InlineData(":set modex 1", ":set modex 1")]
    [InlineData("/say {modex} {xmode} mode", "/say {modex} {xmode} mode")]
    [InlineData("/say :set mode 1", "/say :set mode 1")]
    [InlineData("# uses {mode}", "# uses {phase}")]
    public void RewritesReferences(string line, string expected)
        => Assert.Equal(expected, VariableRenamer.RewriteLine(line, "mode", "phase"));

    [Fact]
    public void NewNameStartingWithDigitIsLiteral()
        => Assert.Equal(":set 2x {2x}", VariableRenamer.RewriteLine(":set mode {mode}", "mode", "2x"));

    [Fact]
    public void FindsOnlyAliasesThatUseTheName()
    {
        var uses = new AliasEntry { Name = "a", Output = [new() { Command = "/say {mode}" }] };
        var other = new AliasEntry { Name = "b", Output = [new() { Command = "/say {modex}" }] };
        Assert.Same(uses, Assert.Single(VariableRenamer.FindUses([uses, other], "mode")));
    }

    [Fact]
    public void NumericNameIsNeverRewritten()
    {
        var alias = new AliasEntry { Name = "a", Output = [new() { Command = "/say {1}" }] };
        Assert.Empty(VariableRenamer.FindUses([alias], "1"));
    }
}

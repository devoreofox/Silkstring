using Silkstring.Models;
using Silkstring.Services;

public class AliasValidatorTests
{
    private static AliasEntry Alias(string name, params string[] output)
        => new() { Name = name, Output = output.Select(o => new CommandEntry { Command = o }).ToList() };

    [Fact]
    public void DetectsDirectCycle()
    {
        var a = Alias("a", "/a");
        Assert.NotEmpty(AliasValidator.FindCycle(a, new[] { a }));
    }

    [Fact]
    public void DetectsIndirectCycle()
    {
        var a = Alias("a", "/b");
        var b = Alias("b", "/a");
        Assert.NotEmpty(AliasValidator.FindCycle(a, new[] { a, b }));
    }

    [Fact]
    public void NoCycleForPlainCommands()
    {
        var a = Alias("a", "/say hi");
        Assert.Empty(AliasValidator.FindCycle(a, new[] { a }));
    }

    [Fact]
    public void NoCycleForChainWithoutLoop()
    {
        var a = Alias("a", "/b");
        var b = Alias("b", "/say hi");
        Assert.Empty(AliasValidator.FindCycle(a, new[] { a, b }));
    }

    [Fact]
    public void NonSlashLineIsNotADependency()
    {
        var a = Alias("a", "a");
        Assert.Empty(AliasValidator.FindCycle(a, new[] { a }));
    }

}

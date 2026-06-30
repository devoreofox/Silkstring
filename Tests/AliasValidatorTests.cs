using Silkstring.Models;
using Silkstring.Services;

public class AliasValidatorTests
{
    private static AliasEntry Alias(string name, params string[] output)
        => new() { Name = name, Output = output.Select(o => new CommandEntry { Command = o }).ToList() };

    private static HashSet<string> Defined(params string[] names)
        => new(names, StringComparer.OrdinalIgnoreCase);

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

    [Fact] public void ValidIf() => Assert.Null(AliasValidator.ValidateBlocks(Alias("a", ":if {hp} < 50", "/echo", ":endif")));
    [Fact] public void ValidIfElse() => Assert.Null(AliasValidator.ValidateBlocks(Alias("a", ":if {hp} < 50", "/a", ":else", "/b", ":endif")));
    [Fact] public void NestedValid() => Assert.Null(AliasValidator.ValidateBlocks(Alias("a", ":if {a} == 1", ":if {b} == 2", "/c", ":endif", ":endif")));
    [Fact] public void NoBlocks() => Assert.Null(AliasValidator.ValidateBlocks(Alias("a", "/say hi")));
    [Fact] public void Unclosed() => Assert.NotNull(AliasValidator.ValidateBlocks(Alias("a", ":if {hp} < 50", "/a")));
    [Fact] public void OrphanElse() => Assert.NotNull(AliasValidator.ValidateBlocks(Alias("a", ":else")));
    [Fact] public void OrphanEndIf() => Assert.NotNull(AliasValidator.ValidateBlocks(Alias("a", ":endif")));
    [Fact] public void DuplicateElse() => Assert.NotNull(AliasValidator.ValidateBlocks(Alias("a", ":if {a} == 1", ":else", ":else", ":endif")));
    [Fact] public void BadExpression() => Assert.NotNull(AliasValidator.ValidateBlocks(Alias("a", ":if {hp} <=", ":endif")));

    [Fact] public void SetKnown() => Assert.Null(AliasValidator.ValidateSets(Alias("a", ":set foo bar"), Defined("foo")));
    [Fact] public void SetCaseInsensitive() => Assert.Null(AliasValidator.ValidateSets(Alias("a", ":set FOO bar"), Defined("foo")));
    [Fact] public void SetUnknown() => Assert.NotNull(AliasValidator.ValidateSets(Alias("a", ":set foo bar"), Defined()));
    [Fact] public void SetNoName() => Assert.NotNull(AliasValidator.ValidateSets(Alias("a", ":set "), Defined("foo")));
    [Fact] public void SetOneBadAmongGood() => Assert.NotNull(AliasValidator.ValidateSets(Alias("a", ":set foo a", ":set bar b"), Defined("foo")));
    [Fact] public void SetNonSetLinesIgnored() => Assert.Null(AliasValidator.ValidateSets(Alias("a", "/say hi"), Defined()));
}

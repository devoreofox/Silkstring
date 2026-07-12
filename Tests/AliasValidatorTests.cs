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

    [Fact] public void ValidBraceBlock() => Assert.Empty(AliasValidator.ValidateBlocks(Alias("a", "if (1 == 1) {", "/echo", "}")));
    [Fact] public void UnclosedBraceBlock() => Assert.NotEmpty(AliasValidator.ValidateBlocks(Alias("a", "if (1 == 1) {", "/echo")));

    [Fact] public void TriggerMissing() => Assert.NotEmpty(AliasValidator.ValidateTrigger(Alias("")));
    [Fact] public void TriggerValid() => Assert.Empty(AliasValidator.ValidateTrigger(Alias("greet")));
    [Fact] public void TriggerWithSpace() => Assert.NotEmpty(AliasValidator.ValidateTrigger(Alias("hello world")));
    [Fact] public void TriggerReserved() => Assert.NotEmpty(AliasValidator.ValidateTrigger(Alias("silkstring")));

    [Fact] public void SetKnown() => Assert.Empty(AliasValidator.ValidateSets(Alias("a", ":set foo bar"), Defined("foo")));
    [Fact] public void SetCaseInsensitive() => Assert.Empty(AliasValidator.ValidateSets(Alias("a", ":set FOO bar"), Defined("foo")));
    [Fact] public void SetUnknown() => Assert.NotEmpty(AliasValidator.ValidateSets(Alias("a", ":set foo bar"), Defined()));
    [Fact] public void SetNoName() => Assert.NotEmpty(AliasValidator.ValidateSets(Alias("a", ":set "), Defined("foo")));
    [Fact] public void SetOneBadAmongGood() => Assert.NotEmpty(AliasValidator.ValidateSets(Alias("a", ":set foo a", ":set bar b"), Defined("foo")));
    [Fact] public void SetNonSetLinesIgnored() => Assert.Empty(AliasValidator.ValidateSets(Alias("a", "/say hi"), Defined()));

    [Fact] public void WaitValid() => Assert.Empty(AliasValidator.ValidateWaits(Alias("a", ":wait 2")));
    [Fact] public void WaitDecimal() => Assert.Empty(AliasValidator.ValidateWaits(Alias("a", ":wait 1.5")));
    [Fact] public void WaitToken() => Assert.Empty(AliasValidator.ValidateWaits(Alias("a", ":wait {0}")));
    [Fact] public void WaitOverCapIsValid() => Assert.Empty(AliasValidator.ValidateWaits(Alias("a", ":wait 120")));
    [Fact] public void WaitInvalid() => Assert.NotEmpty(AliasValidator.ValidateWaits(Alias("a", ":wait potato")));
    [Fact] public void WaitNegative() => Assert.NotEmpty(AliasValidator.ValidateWaits(Alias("a", ":wait -1")));
    [Fact] public void WaitEmpty() => Assert.NotEmpty(AliasValidator.ValidateWaits(Alias("a", ":wait ")));
    [Fact] public void WaitNonWaitLinesIgnored() => Assert.Empty(AliasValidator.ValidateWaits(Alias("a", "/say hi")));

    [Fact] public void WaitReportsLine() => Assert.Equal(1, Assert.Single(AliasValidator.ValidateWaits(Alias("a", "/say hi", ":wait potato"))).Line);
    [Fact] public void SetReportsEveryBadLine() => Assert.Equal(2, AliasValidator.ValidateSets(Alias("a", ":set foo a", ":set bar b", ":set baz c"), Defined("foo")).Count());
}

using Silkstring.Models;
using Silkstring.Services;

public class AliasMatcherTests
{
    private static AliasEntry Valid(string name, bool enabled = true)
        => new() { Name = name, Enabled = enabled, Output = new() { new CommandEntry { Command = "/say hi" } } };

    [Fact]
    public void MatchesByTrigger()
    {
        var a = Valid("greet");
        Assert.Same(a, AliasMatcher.Match("greet", new[] { a }));
    }

    [Fact]
    public void MatchesByPipeName()
    {
        var a = Valid("mew|meow");
        Assert.Same(a, AliasMatcher.Match("meow", new[] { a }));
    }

    [Fact]
    public void MatchIsCaseInsensitive()
    {
        var a = Valid("greet");
        Assert.Same(a, AliasMatcher.Match("GREET", new[] { a }));
    }

    [Fact]
    public void DoesNotMatchUnknown() => Assert.Null(AliasMatcher.Match("nope", new[] { Valid("greet") }));

    [Fact]
    public void DoesNotMatchDisabled() => Assert.Null(AliasMatcher.Match("greet", new[] { Valid("greet", false) }));

    [Fact]
    public void DoesNotMatchInvalid() => Assert.Null(AliasMatcher.Match("greet", new[] { new AliasEntry { Name = "greet" } }));
}

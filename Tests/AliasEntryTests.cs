using Silkstring.Models;

public class AliasEntryTests
{
    private static AliasEntry WithName(string name)
        => new() { Name = name, Output = new() { new CommandEntry { Command = "/say hi" } } };

    [Theory]
    [InlineData("greet", true)]
    [InlineData("mew|meow", true)]
    [InlineData("silkstring", false)]
    [InlineData("xlplugins", false)]
    [InlineData("greet|silkstring", false)]
    [InlineData("gr eet", false)]
    [InlineData("gr/eet", false)]
    [InlineData("", false)]
    public void ValidatesName(string name, bool expected) => Assert.Equal(expected, WithName(name).IsValid());

    [Fact]
    public void InvalidWithoutOutput() => Assert.False(new AliasEntry { Name = "greet" }.IsValid());

    [Fact]
    public void ValidWithOneNonBlankCommand()
        => Assert.True(new AliasEntry { Name = "greet", Output = new() { new CommandEntry { Command = "/say" }, new CommandEntry() } }.IsValid());

    [Fact]
    public void InvalidWithOnlyBlankCommands()
        => Assert.False(new AliasEntry { Name = "greet", Output = new() { new CommandEntry() } }.IsValid());

    [Theory]
    [InlineData("Greeting", "greet", "Greeting")]
    [InlineData("", "greet", "greet")]
    [InlineData("   ", "greet", "greet")]
    public void EffectiveNamePrefersDisplayName(string display, string name, string expected)
        => Assert.Equal(expected, new AliasEntry { DisplayName = display, Name = name }.EffectiveName);
}

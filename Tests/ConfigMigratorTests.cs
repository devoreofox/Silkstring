using Silkstring.Models;
using Silkstring.Services;

public class ConfigMigratorTests
{
    [Fact]
    public void MigratesIfElseEndifToBraces()
    {
        var alias = new AliasEntry { Output = new()
        {
            new() { Command = ":if {hp} < 50" },
            new() { Command = "/a" },
            new() { Command = ":else" },
            new() { Command = "/b" },
            new() { Command = ":endif" },
        }};
        ConfigMigrator.MigrateAliasToV3(alias);
        Assert.Equal(
            new[] { "if ({hp} < 50) {", "/a", "}", "else {", "/b", "}" },
            alias.Output.Select(c => c.Command));
    }

    [Fact]
    public void MigratesNestedIf()
    {
        var alias = new AliasEntry { Output = new()
        {
            new() { Command = ":if {a} == 1" },
            new() { Command = ":if {b} == 2" },
            new() { Command = "/c" },
            new() { Command = ":endif" },
            new() { Command = ":endif" },
        }};
        ConfigMigrator.MigrateAliasToV3(alias);
        Assert.Equal(
            new[] { "if ({a} == 1) {", "if ({b} == 2) {", "/c", "}", "}" },
            alias.Output.Select(c => c.Command));
    }

    [Fact]
    public void LeavesNonConditionalLinesAlone()
    {
        var alias = new AliasEntry { Output = new()
        {
            new() { Command = "/say hi" },
            new() { Command = ":wait 2" },
            new() { Command = "good luck" },
        }};
        ConfigMigrator.MigrateAliasToV3(alias);
        Assert.Equal(
            new[] { "/say hi", ":wait 2", "good luck" },
            alias.Output.Select(c => c.Command));
    }
}

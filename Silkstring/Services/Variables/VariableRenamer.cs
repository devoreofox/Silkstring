using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Silkstring.Models;

namespace Silkstring.Services.Variables;

public static class VariableRenamer
{
    private static Regex Token(string name) => new($@"\{{{Regex.Escape(name)}\}}", RegexOptions.IgnoreCase);
    private static Regex SetTarget(string name) => new($@"^(\s*:set\s+){Regex.Escape(name)}(?=\s|$)", RegexOptions.IgnoreCase);

    public static List<AliasEntry> FindUses(IEnumerable<AliasEntry> aliases, string name)
    {
        if (int.TryParse(name, out _)) return [];
        var token = Token(name);
        var set = SetTarget(name);
        return aliases.Where(a => a.Output.Any(c => token.IsMatch(c.Command) || set.IsMatch(c.Command))).ToList();
    }

    public static void Rename(IEnumerable<AliasEntry> aliases, string oldName, string newName)
    {
        foreach (var command in aliases.SelectMany(a => a.Output)) command.Command = RewriteLine(command.Command, oldName, newName);
    }

    internal static string RewriteLine(string line, string oldName, string newName) => SetTarget(oldName).Replace(Token(oldName).Replace(line, $"{{{newName}}}"), $"${{1}}{newName}");

}

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Silkstring.Services;

public static class ArgumentParser
{
    private static readonly Regex TokenPattern = new(@"""([^""]*)""|(\S+)", RegexOptions.Compiled);

    public static List<string> Parse(string input)
    {
        var args = new List<string>();
        if (string.IsNullOrWhiteSpace(input)) return args;

        foreach (Match match in TokenPattern.Matches(input)) args.Add(match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);
        return args;
    }
}

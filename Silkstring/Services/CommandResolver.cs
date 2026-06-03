using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Silkstring.Services;

public static class CommandResolver
{
    private static Dictionary<string, string> BuildVariables()
    {
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        variables.Add("job", Plugin.PlayerState.ClassJob.Value.Abbreviation.ToString());
        variables.Add("level",  Plugin.PlayerState.Level.ToString());
        variables.Add("character", Plugin.PlayerState.CharacterName);
        variables.Add("world", Plugin.PlayerState.CurrentWorld.Value.Name.ToString());

        return variables;
    }
    public static string Resolve(string command)
    {
        if (!Plugin.PlayerState.IsLoaded) return command;
        var variables = BuildVariables();

        return Regex.Replace(command, @"\{(\w+)\}", match =>
        {
            var token = match.Groups[1].Value;
            return variables.TryGetValue(token, out var value) ? value : match.Value;
        }, RegexOptions.IgnoreCase);
    }
}

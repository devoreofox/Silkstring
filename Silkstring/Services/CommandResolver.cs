using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Plugin.Services;
using Serilog;

namespace Silkstring.Services;

public class CommandResolver
{
    private readonly IPlayerState _playerState;

    public CommandResolver(IPlayerState playerState)
    {
        _playerState = playerState;
    }
    private Dictionary<string, string> BuildVariables()
    {
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        variables.Add("job", _playerState.ClassJob.Value.Abbreviation.ToString());
        variables.Add("level",  _playerState.Level.ToString());
        variables.Add("character", _playerState.CharacterName);
        variables.Add("world", _playerState.CurrentWorld.Value.Name.ToString());

        return variables;
    }

    public string Resolve(string command)
    {
        if (!_playerState.IsLoaded) return command;
        try
        {
            var variables = BuildVariables();

            return Regex.Replace(command, @"\{(\w+)\}", match =>
            {
                var token = match.Groups[1].Value;
                return variables.TryGetValue(token, out var value) ? value : match.Value;
            }, RegexOptions.IgnoreCase);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resolve variables for command: {Command}", command);
            return command;
        }
    }
}

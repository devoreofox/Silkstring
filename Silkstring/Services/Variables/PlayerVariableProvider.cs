using System.Collections.Generic;
using Dalamud.Plugin.Services;

namespace Silkstring.Services.Variables;

public sealed class PlayerVariableProvider : IVariableProvider
{
    private readonly IPlayerState _playerState;

    public PlayerVariableProvider(IPlayerState playerState)
    {
        _playerState = playerState;
    }

    public IEnumerable<VariableDescriptor> GetVariables()
    {
        yield return new("character", "Your character's name", "Player", () => _playerState.IsLoaded ? _playerState.CharacterName : null);
        yield return new("homeworld", "Your character's homeworld", "Player", () => _playerState.IsLoaded ? _playerState.HomeWorld.Value.Name.ToString() : null);
        yield return new("job", "Current job abbreviation", "Player", () => _playerState.IsLoaded ? _playerState.ClassJob.Value.Abbreviation.ToString() : null);
        yield return new("level", "Current character level", "Player",  () => _playerState.IsLoaded ? _playerState.Level.ToString() : null);
        yield return new("world", "Your current world", "Player", () => _playerState.IsLoaded ? _playerState.CurrentWorld.Value.Name.ToString() : null);
    }
}

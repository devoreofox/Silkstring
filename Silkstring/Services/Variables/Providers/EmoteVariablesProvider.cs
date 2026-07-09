using System.Collections.Generic;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices.Legacy;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;

namespace Silkstring.Services.Variables.Providers;

public sealed class EmoteVariablesProvider : IVariableProvider
{
    private readonly IClientState _clientState;
    private readonly IDataManager _dataManager;

    public EmoteVariablesProvider(IClientState clientState, IDataManager dataManager)
    {
        _clientState = clientState;
        _dataManager = dataManager;
    }

    public IEnumerable<VariableDescriptor> GetVariables()
    {
        yield return new("emoting", "Whether you are currently emoting or posing", "Emote", () => IsEmoting() ? "true" : "false");
        yield return new ("emote", "Name of the emote you are currently doing", "Emote", EmoteName);
        yield return new ("pose", "Pose of the emote you are currently doing", "Emote", Pose);
    }

    private unsafe Character* Local()
    {
        var player = _clientState.LocalPlayer;
        return player is null ? null : (Character*)player.Address;
    }

    private unsafe bool IsEmoting()
    {
        var character = Local();
        return character is not null && character->Mode is CharacterModes.InPositionLoop or CharacterModes.EmoteLoop;
    }

    private unsafe string? EmoteName()
    {
        var character = Local();
        if (character is null || character->Mode is not CharacterModes.InPositionLoop or CharacterModes.EmoteLoop) return null;
        var mode = _dataManager.GetExcelSheet<EmoteMode>().GetRowOrDefault(character->ModeParam);
        if (mode is null) return null;
        var emote = mode.Value.StartEmote;
        if (!emote.IsValid || emote.RowId == 0) return null;
        return emote.Value.Name.ToString();
    }

    private unsafe string? Pose()
    {
        var character = Local();
        if (character is null || !IsEmoting()) return null;
        return character->EmoteController.CPoseState.ToString();
    }
}

using System.Collections.Generic;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices.Legacy;

namespace Silkstring.Services.Variables.Providers;

public sealed class VitalsVariablesProvider : IVariableProvider
{
    private readonly IClientState _clientState;

    public VitalsVariablesProvider(IClientState clientState) => _clientState = clientState;

    public IEnumerable<VariableDescriptor> GetVariables()
    {
        yield return new("hp", "Your current HP", "Vitals", () => _clientState.LocalPlayer?.CurrentHp.ToString());
        yield return new("maxhp", "Your maximum HP", "Vitals", () => _clientState.LocalPlayer?.MaxHp.ToString());
        yield return new("hpp", "Your HP as a percentage", "Vitals", () => Percent(_clientState.LocalPlayer?.CurrentHp, _clientState.LocalPlayer?.MaxHp));
        yield return new("mp", "Your current MP", "Vitals", () => _clientState.LocalPlayer?.CurrentMp.ToString());
        yield return new("maxmp", "Your maximum MP", "Vitals", () => _clientState.LocalPlayer?.MaxMp.ToString());
        yield return new("mpp", "Your MP as a percentage", "Vitals", () => Percent(_clientState.LocalPlayer?.CurrentMp, _clientState.LocalPlayer?.MaxMp));
    }

    private static string? Percent(uint? current, uint? max) => current is {} c && max is {} m && m > 0 ? (c * 100 / m).ToString() : null;

}

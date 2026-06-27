using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;

namespace Silkstring.Services.Variables;

public sealed class CombatVariablesProvider : IVariableProvider
{
    private readonly ICondition _condition;
    private readonly ITargetManager _targets;

    public CombatVariablesProvider(ICondition condition, ITargetManager targets)
    {
        _condition = condition;
        _targets = targets;
    }

    public IEnumerable<VariableDescriptor> GetVariables()
    {
        yield return new("incombat", "Whether you are in combat", "Combat", () => _condition[ConditionFlag.InCombat] ? "true" : "false");

        yield return new("hastarget", "Whether you have a target", "Combat", () => _targets.Target != null ? "true" : "false");
    }
}

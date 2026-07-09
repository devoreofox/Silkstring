using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Silkstring.Services.Variables.Providers;

public sealed class CurrencyVariablesProvider : IVariableProvider
{
    public IEnumerable<VariableDescriptor> GetVariables()
    {
        yield return new("gil", "Your gil", "Currency", () => Count(1));
        yield return new("mgp", "Your MGP", "Currency", () => Count(29));
    }

    private static unsafe string? Count(uint itemId)
    {
        var inv = InventoryManager.Instance();
        return inv != null ? inv->GetInventoryItemCount(itemId).ToString() : null;
    }

}

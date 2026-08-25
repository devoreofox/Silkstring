using System.Collections.Generic;
using Dalamud.Plugin;
using Penumbra.Api.IpcSubscribers;

namespace Silkstring.Services.Variables.Providers;

public class PenumbraVariablesProvider : IVariableProvider
{
    private readonly GetCollectionForObject _getCollection;
    private readonly GetEnabledState _getEnabled;

    public PenumbraVariablesProvider(IDalamudPluginInterface pi)
    {
        _getCollection = new GetCollectionForObject(pi);
        _getEnabled = new  GetEnabledState(pi);
    }

    public IEnumerable<VariableDescriptor> GetVariables()
    {
        yield return new("collection", "Your current Penumbra collection", "Penumbra", () =>
        {
            try
            {
                var (valid, _, effective) = _getCollection.Invoke(0);
                return valid ? effective.Name : null;
            }
            catch { return null; }
        });

        yield return new("penumbra", "Whether Penumbra is enabled", "Penumbra", () =>
        {
            try { return _getEnabled.Invoke() ? "true" : "false"; }
            catch { return null; }
        });
    }

}

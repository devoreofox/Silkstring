using System;
using System.Collections.Generic;
using System.Linq;
using Silkstring.Models;

namespace Silkstring.Services.Variables.Providers;

public sealed class UserVariableProvider : IVariableProvider
{
    private readonly Func<IReadOnlyList<UserVariable>> _variables;

    public UserVariableProvider(Func<IReadOnlyList<UserVariable>> variables)
    {
        _variables = variables;
    }

    public IEnumerable<VariableDescriptor> GetVariables()
    {
        foreach (var variable in _variables())
        {
            var name = variable.Name;
            yield return new(name, variable.Description, "User", () => Lookup(name));
        }
    }

    private string? Lookup(string name)
        => _variables().FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase))?.Value;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Plugin.Services;
using Serilog;
using Silkstring.Services.Variables;

namespace Silkstring.Services;

public class CommandResolver
{
    private readonly Dictionary<string, VariableDescriptor> _variables;

    public CommandResolver(IEnumerable<IVariableProvider> providers)
    {
        _variables = providers.SelectMany(p => p.GetVariables()).ToDictionary(v => v.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<VariableDescriptor> Variables => _variables.Values;

    public string Resolve(string command)
    {
        try
        {
            return Regex.Replace(command, @"\{(\w+)\}", match =>
            {
                var token = match.Groups[1].Value;
                if (!_variables.TryGetValue(token, out var descriptor)) return match.Value;
                return descriptor.Resolve() ?? match.Value;
            }, RegexOptions.IgnoreCase);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resolve variables for command: {Command}", command);
            return command;
        }
    }
}

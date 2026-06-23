using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

    public string Resolve(string command) => Resolve(command, []);
    public string Resolve(string command, IReadOnlyList<string> args)
    {
        try
        {
            return Regex.Replace(command, @"\{(\d+\.\.\d+|\.\.\d+|\d+\.\.|\w+|\*)\}", match =>
            {
                var token = match.Groups[1].Value;

                if (token == "*") return string.Join(" ", args);

                if (token.Contains(".."))
                {
                    var parts = token.Split("..");
                    var start = parts[0].Length == 0 ? 0 : int.Parse(parts[0]);
                    var end = parts[1].Length == 0 ? args.Count : int.Parse(parts[1]);
                    start = Math.Clamp(start, 0, args.Count);
                    end = Math.Clamp(end, 0, args.Count);
                    return start < end ? string.Join(" ", args.Skip(start).Take(end - start)) : string.Empty;
                }

                if (int.TryParse(token, out var index)) return index >= 0 && index < args.Count ? args[index] : match.Value;
                if (_variables.TryGetValue(token, out var descriptor)) return descriptor.Resolve() ?? match.Value;

                return match.Value;
            }, RegexOptions.IgnoreCase);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resolve variables for command: {Command}", command);
            return command;
        }
    }
}

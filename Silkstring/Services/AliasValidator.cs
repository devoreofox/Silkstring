using System;
using System.Collections.Generic;
using Silkstring.Models;

namespace Silkstring.Services;

public static class AliasValidator
{
    public static List<string> FindCycle(AliasEntry target, IEnumerable<AliasEntry> allAliases)
    {
        var lookup = BuildTriggerLookup(allAliases);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = new List<string>();
        return Dfs(target, lookup, visited, path);
    }

    private static Dictionary<string, AliasEntry> BuildTriggerLookup(IEnumerable<AliasEntry> allAliases)
    {
        var lookup = new Dictionary<string, AliasEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var alias in allAliases)
        {
            var triggers = alias.Name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (var trigger in triggers)
            {
                lookup.TryAdd(trigger, alias);
            }
        }
        return lookup;
    }

    private static IEnumerable<AliasEntry> GetDependencies(AliasEntry alias, Dictionary<string, AliasEntry> lookup)
    {
        foreach (var command in alias.Output)
        {
            var trimmed = command.Command.TrimStart('/');
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            var commandName = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            if (lookup.TryGetValue(commandName, out var dependency)) yield return dependency;
        }
    }

    private static List<string> Dfs(
        AliasEntry current, Dictionary<string, AliasEntry> lookup, HashSet<string> visited, List<string> path)
    {
        var triggers = current.Name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (triggers.Length == 0) return new List<string>();

        var trigger = triggers[0];
        path.Add(trigger);

        foreach (var dependency in GetDependencies(current, lookup))
        {
            var depTrigger = dependency.Name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];

            if (path.Contains(depTrigger))
            {
                var cycle = new List<string>(path) { depTrigger };
                return cycle;
            }

            if (!visited.Contains(depTrigger))
            {
                var result = Dfs(dependency, lookup, visited, path);
                if (result.Count > 0) return result;
            }
        }

        path.Remove(trigger);
        visited.Add(trigger);
        return new List<string>();
    }
}

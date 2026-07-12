using System;
using System.Collections.Generic;
using Silkstring.Models;
using Silkstring.Services.Conditions;

namespace Silkstring.Services;

public static class AliasValidator
{
    public static List<Diagnostic> Validate(AliasEntry alias, ISet<string> definedVariables, bool allowUnsafe, IEnumerable<AliasEntry> allAliases)
    {
        var diagnostics = new List<Diagnostic>();
        diagnostics.AddRange(ValidateBlocks(alias));
        diagnostics.AddRange(ValidateSets(alias, definedVariables));
        diagnostics.AddRange(ValidateWaits(alias));
        diagnostics.AddRange(ValidateUntils(alias, allowUnsafe));
        var cycle = FindCycle(alias, allAliases);
        if (cycle.Count > 0) diagnostics.Add(new($"Cycle detected: {string.Join(" → ", cycle)}"));
        return diagnostics;
    }

    public static List<string> FindCycle(AliasEntry target, IEnumerable<AliasEntry> allAliases)
    {
        var lookup = BuildTriggerLookup(allAliases);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = new List<string>();
        return Dfs(target, lookup, visited, path);
    }

    public static IEnumerable<Diagnostic> ValidateBlocks(AliasEntry alias)
    {
        var elseSeen = new Stack<bool>();
        for (var i = 0; i < alias.Output.Count; i++)
        {
            var (kind, expression) = BlockInterpreter.Classify(alias.Output[i].Command.Trim());
            switch (kind)
            {
                case BlockKind.If:
                    if (!Condition.TryParse(expression, out _, out var error)) { yield return new($"Invalid condition: {error}", i); yield break; }
                    elseSeen.Push(false);
                    break;
                case BlockKind.Else:
                    if (elseSeen.Count == 0) { yield return new(":else without a matching :if", i); yield break; }
                    if (elseSeen.Peek()) { yield return new("Duplicate :else in a block", i); yield break; }
                    elseSeen.Pop();
                    elseSeen.Push(true);
                    break;
                case BlockKind.EndIf:
                    if (elseSeen.Count == 0) { yield return new(":endif without a matching :if", i); yield break; }
                    elseSeen.Pop();
                    break;
            }
        }
        if (elseSeen.Count > 0) yield return new("Unclosed :if (missing :endif)");
    }

    public static IEnumerable<Diagnostic> ValidateSets(AliasEntry alias, ISet<string> defined)
    {
        for (var i = 0; i < alias.Output.Count; i++)
        {
            var (kind, expression) = BlockInterpreter.Classify(alias.Output[i].Command.Trim());
            if (kind != BlockKind.Set) continue;
            var (name, _) = BlockInterpreter.ParseSet(expression);
            if (string.IsNullOrEmpty(name)) { yield return new(":set needs a variable name", i); continue; }
            if (!defined.Contains(name)) yield return new($"Unknown variable in :set: {name}", i);
        }
    }

    public static IEnumerable<Diagnostic> ValidateWaits(AliasEntry alias)
    {
        for (var i = 0; i < alias.Output.Count; i++)
        {
            var (kind, expression) = BlockInterpreter.Classify(alias.Output[i].Command.Trim());
            if (kind != BlockKind.Wait) continue;
            var (value, _) = BlockInterpreter.ParseSet(expression);
            if (string.IsNullOrEmpty(value)) { yield return new(":wait needs a duration", i); continue; }
            if (expression.Contains('{')) continue;
            if (!BlockInterpreter.TryParseDuration(expression, out _)) yield return new($"Invalid :wait duration: {expression}", i);
        }
    }

    public static IEnumerable<Diagnostic> ValidateUntils(AliasEntry alias, bool allowUnsafe)
    {
        for (var i = 0; i < alias.Output.Count; i++)
        {
            var (kind, expression) = BlockInterpreter.Classify(alias.Output[i].Command.Trim());
            if (kind != BlockKind.Until) continue;
            var (isUnsafe, condition) = BlockInterpreter.ParseUntil(expression);
            if (string.IsNullOrWhiteSpace(condition)) { yield return new(":until needs a condition", i); continue; }
            if (!Condition.TryParse(condition, out _, out var error)) { yield return new($"Invalid :until condition: {error}", i); continue; }
            if (isUnsafe && !allowUnsafe) yield return new("This :until uses -unsafe, but unsafe waits are off in settings", i, Severity.Warning);
        }
    }

    private static Dictionary<string, AliasEntry> BuildTriggerLookup(IEnumerable<AliasEntry> allAliases)
    {
        var lookup = new Dictionary<string, AliasEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var alias in allAliases)
        {
            var triggers = alias.Name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (var trigger in triggers) lookup.TryAdd(trigger, alias);
        }
        return lookup;
    }

    private static IEnumerable<AliasEntry> GetDependencies(AliasEntry alias, Dictionary<string, AliasEntry> lookup)
    {
        foreach (var command in alias.Output)
        {
            var text = command.Command.TrimStart();
            if (!text.StartsWith('/')) continue;

            var trimmed = text.TrimStart('/');
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

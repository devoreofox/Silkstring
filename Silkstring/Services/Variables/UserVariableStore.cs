using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Silkstring.Models;

namespace Silkstring.Services.Variables;

public sealed class UserVariableStore
{
    private readonly List<UserVariable> _vars;
    private readonly ISet<string> _reserved;
    private readonly Action _onChanged;

    private static readonly Regex NamePattern = new(@"^\w+$", RegexOptions.Compiled);

    public IReadOnlyList<UserVariable> Variables => _vars;

    public UserVariableStore(List<UserVariable> vars, ISet<string> reserved, Action onChanged)
    {
        _vars = vars;
        _reserved = reserved;
        _onChanged = onChanged;
    }

    public string? ValidateName(string name, UserVariable? excluding = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Enter a name";
        if (!NamePattern.IsMatch(name)) return "Use only letters, numbers, and underscores";
        if (_reserved.Contains(name)) return $"{name} is a built-in variable";
        if (_vars.Any(v => v != excluding && string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase))) return $"{name} is already defined";
        return null;
    }

    public bool TryAdd(string name)
    {
        if (ValidateName(name) != null) return false;
        _vars.Add(new UserVariable { Name = name });
        _onChanged();
        return true;
    }

    public bool TrySet(string name, string value)
    {
        var variable = _vars.FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));
        if (variable == null) return false;
        variable.Value = value;
        _onChanged();
        return true;
    }

    public void MarkChanged() => _onChanged();

    public void Remove(UserVariable variable)
    {
        _vars.Remove(variable);
        _onChanged();
    }
}

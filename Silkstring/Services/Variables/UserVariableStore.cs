using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Silkstring.Models;

namespace Silkstring.Services.Variables;

public sealed class UserVariableStore : FolderStore<UserVariable>
{
    private readonly ISet<string> _reserved;

    private static readonly Regex NamePattern = new(@"^\w+$", RegexOptions.Compiled);

    public UserVariableStore(List<UserVariable> vars, List<Folder<UserVariable>> folders, ISet<string> reserved, Action onChanged) : base(vars, folders, onChanged)
    {
        _reserved = reserved;
    }

    public string? ValidateName(string name, UserVariable? excluding = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Enter a name";
        if (!NamePattern.IsMatch(name)) return "Use only letters, numbers, and underscores";
        if (int.TryParse(name, out _)) return "Names can't be only numbers";
        if (_reserved.Contains(name)) return $"{name} is a built-in variable";
        if (All.Any(v => v != excluding && string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase))) return $"{name} is already defined";
        return null;
    }

    public bool TryAdd(string name)
    {
        if (ValidateName(name) != null) return false;
        Add(new UserVariable { Name = name });
        return true;
    }

    public bool TrySet(string name, string value)
    {
        var variable = All.FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));
        if (variable == null)
        {
            if (ValidateName(name) != null) return false;
            Add(new UserVariable { Name = name, Value = value });
            return true;
        }
        variable.Value = value;
        MarkChanged();
        return true;
    }
}

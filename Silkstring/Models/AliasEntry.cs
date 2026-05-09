using System;
using System.Collections.Generic;
using System.Linq;

namespace Silkstring.Models;

public class AliasEntry
{
    public static readonly string[] Blacklist = ["silkstring", "xlplugins", "xlsettings", "xldclose", "xldev"];

    public bool Enabled = true;
    public string Name = string.Empty;
    public List<string> Output = new();

    [NonSerialized]
    public bool Delete;

    [NonSerialized]
    public int UniqueId;

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (Blacklist.Contains(Name)) return false;
        if (Name.Contains(' ')) return false;
        if (Output.Count  == 0) return false;
        return !Output.Any(command => string.IsNullOrWhiteSpace(command));
    }

    public AliasEntry Clone()
    {
        return new AliasEntry
        {
            Enabled = Enabled,
            Name = Name,
            Output = new List<string>(Output),
        };

    }
}

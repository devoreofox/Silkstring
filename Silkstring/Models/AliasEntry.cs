using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Silkstring.Models;

public class AliasEntry
{
    private static int _nextId = 0;
    public static readonly JsonSerializerOptions SerializerOptions = new() { IncludeFields = true };

    public static readonly HashSet<string> Blacklist = new(StringComparer.OrdinalIgnoreCase)
    {
        "silkstring", "xlplugins", "xlsettings", "xldclose", "xldev"
    };

    public string DisplayName = string.Empty;
    public bool Enabled = true;
    public string Name = string.Empty;
    public List<CommandEntry> Output = new();

    [NonSerialized]
    public bool Delete;

    [NonSerialized]
    [JsonIgnore]
    public int UniqueId;

    public AliasEntry()
    {
        UniqueId = Interlocked.Increment(ref _nextId);
    }

    public bool IsValid()
    {
        var names = Name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (names.Length == 0) return false;

        foreach (var name in names)
        {
            if (Blacklist.Contains(name)) return false;
            if (name.Contains(' ')) return false;
            if (name.Contains('/')) return false;
        }
        if (Output.Count == 0) return false;
        return !Output.Any(command => string.IsNullOrWhiteSpace(command.Command));
    }

    public AliasEntry Clone()
    {
        return new AliasEntry
        {
            DisplayName = DisplayName,
            Enabled = Enabled,
            Name = Name,
            Output = Output.Select(c => c.Clone()).ToList()
        };

    }
}

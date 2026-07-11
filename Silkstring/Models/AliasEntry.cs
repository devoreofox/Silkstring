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
    public string Name = string.Empty;

    [JsonIgnore] public string[] triggers => Name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    [JsonIgnore] public string EffectiveName => string.IsNullOrWhiteSpace(DisplayName) ? Name : DisplayName;
    public bool Enabled = true;
    public List<CommandEntry> Output = new();

    [NonSerialized] [JsonIgnore] public int UniqueId;

    public AliasEntry()
    {
        UniqueId = Interlocked.Increment(ref _nextId);
    }

    public bool IsValid()
    {
        if (triggers.Length == 0) return false;

        foreach (var trigger in triggers)
        {
            if (Blacklist.Contains(trigger)) return false;
            if (trigger.Contains(' ')) return false;
            if (trigger.Contains('/')) return false;
        }
        return Output.Any(command => !string.IsNullOrWhiteSpace(command.Command));
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

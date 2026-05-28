using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;

namespace Silkstring.Models;

public class AliasFolder
{
    private static int _nextId = 0;
    public string Name { get; set; }  = string.Empty;
    public List<AliasEntry> Aliases { get; set; } = new();

    [NonSerialized]
    [JsonIgnore]
    public int UniqueId;

    public AliasFolder()
    {
        UniqueId = Interlocked.Increment(ref _nextId);
    }
}

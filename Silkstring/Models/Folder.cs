using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;

namespace Silkstring.Models;

public class Folder<T>
{
    private static int _nextId = 0;
    public string Name { get; set; } = string.Empty;
    public List<T> Items { get; set; } = new();

    [NonSerialized]
    [JsonIgnore]
    public int UniqueId;

    public Folder()
    {
        UniqueId = Interlocked.Increment(ref _nextId);
    }
}

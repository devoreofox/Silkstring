using System;
using System.Text.Json.Serialization;
using System.Threading;

namespace Silkstring.Models;

public class UserVariable
{
    private static int _nextId = 0;

    public string Name = string.Empty;
    public string Description = string.Empty;
    public string Value = string.Empty;

    [NonSerialized]
    [JsonIgnore]
    public bool Delete;

    [NonSerialized]
    [JsonIgnore]
    public int UniqueId;

    public UserVariable()
    {
        UniqueId = Interlocked.Increment(ref _nextId);
    }
}

using System;
using System.Text.Json.Serialization;
using System.Threading;

namespace Silkstring.Models;

public class CommandEntry
{
    private static int _nextId = 0;

    public string Command = string.Empty;

    [NonSerialized]
    [JsonIgnore]
    public int UniqueId;

    public CommandEntry()
    {
        UniqueId = Interlocked.Increment(ref _nextId);
    }

    public CommandEntry Clone()
    {
        return new CommandEntry()
        {
            Command = Command
        };
    }
}

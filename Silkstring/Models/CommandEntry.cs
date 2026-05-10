using System;

namespace Silkstring.Models;

public class CommandEntry
{
    public string Command = string.Empty;

    [NonSerialized]
    public bool Delete;

    [NonSerialized]
    public int UniqueId;

    public CommandEntry Clone()
    {
        return new CommandEntry()
        {
            Command = Command
        };
    }
}

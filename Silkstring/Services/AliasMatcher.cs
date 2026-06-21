using System;
using System.Collections.Generic;
using System.Linq;
using Silkstring.Models;

namespace Silkstring.Services;

public static class AliasMatcher
{
    public static AliasEntry? Match(string commandName, IEnumerable<AliasEntry> aliases)
    {
        return aliases.FirstOrDefault(a =>
                                          a.Enabled && a.IsValid() &&
                                          a.Name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                           .Any(n => n.Equals(commandName, StringComparison.OrdinalIgnoreCase)));
    }
}

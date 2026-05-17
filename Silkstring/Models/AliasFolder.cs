using System.Collections.Generic;

namespace Silkstring.Models;

public class AliasFolder
{
    public string Name { get; set; }  = string.Empty;
    public List<AliasEntry> Aliases { get; set; } = new();
}

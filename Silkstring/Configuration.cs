using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using Silkstring.Models;

namespace Silkstring;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public List<AliasEntry> Aliases = new();

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}

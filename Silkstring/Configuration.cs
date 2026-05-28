using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using Silkstring.Models;

namespace Silkstring;

[Serializable]
public class Configuration : IPluginConfiguration
{
    private bool _isDirty = false;
    private DateTime _lastDirty = DateTime.MinValue;

    public int Version { get; set; } = 1;
    public List<AliasFolder> Folders = new();
    public List<AliasEntry> Aliases = new();
    public int CommandDelay { get; set; } = 100;
    public bool MultilineCommands { get; set; } = false;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public void MarkDirty()
    {
        _isDirty = true;
        _lastDirty = DateTime.UtcNow;
    }

    public void TrySave(TimeSpan debounce)
    {
        if (_isDirty && DateTime.UtcNow - _lastDirty > debounce)
        {
            Save();
            _isDirty = false;
        }
    }
}

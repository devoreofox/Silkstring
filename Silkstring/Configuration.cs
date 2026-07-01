using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Silkstring.Models;

namespace Silkstring;

[Serializable]
public class Configuration : IPluginConfiguration
{
    [NonSerialized]
    private bool _isDirty = false;
    [NonSerialized]
    private DateTime _lastDirty = DateTime.MinValue;

    private int _commandDelay = 100;
    public int CommandDelay
    {
        get => _commandDelay;
        set => _commandDelay = Math.Clamp(value, 0, 1000);
    }

    public const int CurrentVersion = 2;
    public string? LastSeenVersion { get; set; }
    public int Version { get; set; } = CurrentVersion;

    public List<AliasFolder> Folders = new();
    public List<AliasEntry> Aliases = new();
    public List<UserVariable> UserVariables = new();
    public ThemeColors Theme = new();
    public bool MultilineCommands { get; set; }

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

    public IEnumerable<AliasEntry> GetAliases()
    {
        return Aliases.Concat(Folders.SelectMany(f => f.Aliases));
    }
}

using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Serilog;
using Silkstring.Models;

namespace Silkstring.Windows;

public class EditWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private AliasEntry aliasCache;
    private readonly AliasEntry originalAlias;

    public EditWindow(Plugin plugin, AliasEntry alias) : base("Edit Alias###EditWindow")
    {
        aliasCache = alias.Clone();
        originalAlias = alias;
        configuration = plugin.Configuration;
        Log.Information($"Command Name: "+aliasCache.Name);


    }
    public void Dispose() { }

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
    }
}

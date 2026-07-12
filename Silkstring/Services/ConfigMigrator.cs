using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Plugin;
using Serilog;
using Silkstring.Models;

namespace Silkstring.Services;

public static class ConfigMigrator
{
    public sealed record MigrationResult(IReadOnlyList<string> Messages, string? BackupPath);

    public static MigrationResult? Migrate(Configuration config, IDalamudPluginInterface pi)
    {
        if (config.Version >= Configuration.CurrentVersion) return null;

        var backupPath = Backup(pi, config.Version);

        var messages = new List<string>();
        if (config.Version < 2) messages.Add(MigrateToV2(config));
        if (config.Version < 3) messages.Add(MigrateToV3(config));
        config.Version = Configuration.CurrentVersion;
        config.Save();

        return new MigrationResult(messages, backupPath);
    }

    private static string MigrateToV2(Configuration config)
    {
        var count = 0;
        foreach (var alias in config.GetAliases())
        {
            foreach (var cmd in alias.Output)
            {
                var text = cmd.Command.TrimStart();
                if (text.Length > 0 && !text.StartsWith('/'))
                {
                    cmd.Command = '/' + text;
                    count++;
                }
            }
        }

        return $"Prefixed {count} command line(s) with '/' for the new slash rules.";
    }

    private static string MigrateToV3(Configuration config)
    {
        var count = 0;
        foreach (var alias in config.GetAliases())
        {
            if (!alias.Output.Any(c => IsOldConditional(c.Command))) continue;
            MigrateAliasToV3(alias);
            count++;
        }
        return $"Updated {count} alias(es) to the new if and else block format.";
    }

    private static bool IsOldConditional(string line)
    {
        var t = line.Trim();
        return t.StartsWith(":if ", StringComparison.OrdinalIgnoreCase)
               || t.Equals(":else", StringComparison.OrdinalIgnoreCase)
               || t.Equals(":endif", StringComparison.OrdinalIgnoreCase);
    }

    internal static void MigrateAliasToV3(AliasEntry alias)
    {
        var output = new List<CommandEntry>();
        var depth = 0;
        foreach (var cmd in alias.Output)
        {
            var trimmed = cmd.Command.Trim();
            if (trimmed.StartsWith(":if ", StringComparison.OrdinalIgnoreCase))
            {
                output.Add(Line(depth, $"if ({trimmed[4..].Trim()}) {{"));
                depth++;
            }
            else if (trimmed.Equals(":else", StringComparison.OrdinalIgnoreCase))
            {
                depth = Math.Max(0, depth - 1);
                output.Add(Line(depth, "}"));
                output.Add(Line(depth, "else {"));
                depth++;
            }
            else if (trimmed.Equals(":endif", StringComparison.OrdinalIgnoreCase))
            {
                depth = Math.Max(0, depth - 1);
                output.Add(Line(depth, "}"));
            }
            else
                output.Add(Line(depth, trimmed));
        }
        alias.Output = output;
    }

    private static CommandEntry Line(int depth, string text)
        => new() { Command = new string(' ', depth * 4) + text };

    private static string? Backup(IDalamudPluginInterface pi, int fromVersion)
    {
        try
        {
            var src = pi.ConfigFile;
            if (!src.Exists) return null;

            var dest = Path.Combine(src.DirectoryName!, $"{Path.GetFileNameWithoutExtension(src.Name)}.v{fromVersion}.backup.json");
            File.Copy(src.FullName, dest, overwrite: true);
            return dest;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to backup Silkstring config before migration");
            return null;
        }
    }
}



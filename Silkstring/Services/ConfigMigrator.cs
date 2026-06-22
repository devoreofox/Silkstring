using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Plugin;
using Serilog;

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



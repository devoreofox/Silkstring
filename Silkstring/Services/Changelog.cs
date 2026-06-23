using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Silkstring.Services;

public sealed record ChangelogSection(string Version, string Body);

public static class Changelog
{
    public static IReadOnlyList<ChangelogSection> Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Silkstring.CHANGELOG.md");
        if (stream is null) return [];

        using var read = new StreamReader(stream);
        return Parse(read.ReadToEnd().Replace("\r\n", "\n"));
    }

    private static List<ChangelogSection> Parse(string text)
    {
        var sections = new List<ChangelogSection>();
        string? header = null;
        var body = new StringBuilder();

        foreach (var line in text.Split('\n'))
        {
            if (line.StartsWith("## "))
            {
                if (header != null) sections.Add(new ChangelogSection(header, body.ToString().Trim()));
                header = line[3..].Trim();
                body.Clear();
            }
            else if (header != null)
            {
                body.AppendLine(line);
            }
        }

        if (header != null) sections.Add(new ChangelogSection(header, body.ToString().Trim()));
        return sections;
    }

}

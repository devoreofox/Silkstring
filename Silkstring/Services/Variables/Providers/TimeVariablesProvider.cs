using System;
using System.Collections.Generic;

namespace Silkstring.Services.Variables.Providers;

public sealed class TimeVariablesProvider : IVariableProvider
{
    public IEnumerable<VariableDescriptor> GetVariables()
    {
        yield return new("time", "Current local time, for example 3:45 PM (display only, do not compare)", "Time", () => DateTime.Now.ToString("h:mm tt"));
        yield return new("time24", "Current local time in 24-hour form, for example 15:45 (compares with < <= > >=)", "Time", () => DateTime.Now.ToString("HH:mm"));
        yield return new("hour", "Current hour of the day, 0 to 23", "Time", () => DateTime.Now.Hour.ToString());
        yield return new("minute", "Current minute, 00 to 59", "Time", () => DateTime.Now.Minute.ToString("D2"));
        yield return new("date", "Current local date, for example 2026-07-09 (compares chronologically)", "Time", () => DateTime.Now.ToString("yyyy-MM-dd"));
        yield return new("day", "Current day of the week, for example Thursday", "Time", () => DateTime.Now.DayOfWeek.ToString());
        yield return new("daypart", "morning, afternoon, evening, or night", "Time", DayPart);
        yield return new("utc", "Current UTC (server) time in 24-hour form, for example 19:45", "Time", () => DateTime.UtcNow.ToString("HH:mm"));
        yield return new("utcdate", "Current UTC (server) date, for example 2026-07-09", "Time", () => DateTime.UtcNow.ToString("yyyy-MM-dd"));
    }

    private static string DayPart()
    {
        var h = DateTime.Now.Hour;
        if (h < 5) return "night";
        if (h < 12) return "morning";
        if (h < 17) return "afternoon";
        if (h < 21) return "evening";
        return "night";
    }
}

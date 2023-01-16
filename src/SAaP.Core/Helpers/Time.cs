using System;
using System.Collections.Generic;
using System.Linq;

namespace SAaP.Core.Helpers;

public static class Time
{
    public static DateTime GetTimeRightNow()
    {
#if DEBUG
        return DateTime.Parse("2023/01/15 09:55:56");
#else
        return DateTime.Now;
#endif
    }

    public static DateTimeOffset GetTimeOffsetRightNow()
    {
#if DEBUG
        return DateTimeOffset.Parse("2023/01/15 09:55:56");
#else
        return DateTimeOffset.Now;
#endif
    }

    public static List<DateTime> Mealtimes = GetTradeWallTime();

    public static List<DateTime> GetTradeWallTime()
    {
        var now = GetTimeRightNow().ToString("yyyy/MM/dd");

        string[] walls = { " 09:30", " 11:30", " 13:00", " 15:00" };

        return walls.Select(wall => DateTime.Parse(now + wall)).ToList();
    }
}
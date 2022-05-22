namespace YourGameServer.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ReplacedByTimePicker(this DateTime dateTime, TimeSpan? timePickerValue)
    {
        return dateTime.Date + TimeSpan.FromTicks((timePickerValue ?? TimeSpan.Zero).Ticks / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute)
            + TimeSpan.FromTicks(dateTime.Ticks % TimeSpan.TicksPerMinute);
    }

    public static double GetSecondPart(this DateTime dateTime)
    {
        return dateTime.TimeOfDay.Ticks % TimeSpan.TicksPerMinute / (double)TimeSpan.TicksPerSecond;
    }

    public static DateTime ReplacedSecondPart(this DateTime dateTime, double? seconds)
    {
        return dateTime.Date + TimeSpan.FromTicks(dateTime.TimeOfDay.Ticks / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute) + TimeSpan.FromSeconds(seconds ?? 0.0);
    }

    public static double GetMillisecondPart(this DateTime dateTime)
    {
        return dateTime.TimeOfDay.Ticks % TimeSpan.TicksPerSecond / (double)TimeSpan.TicksPerMillisecond;
    }

    public static DateTime ReplacedMillisecondPart(this DateTime dateTime, double? milliseconds)
    {
        return dateTime.Date + TimeSpan.FromTicks(dateTime.TimeOfDay.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond) + TimeSpan.FromMilliseconds(milliseconds ?? 0.0);
    }

    public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
    {
        if(timeSpan == TimeSpan.Zero) return dateTime;
        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }
}

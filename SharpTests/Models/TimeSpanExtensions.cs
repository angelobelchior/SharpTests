namespace SharpTests.Models;

public static class TimeSpanExtensions
{
    public static string ToWrite(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalMilliseconds < 1000)
            return $"{timeSpan.TotalMilliseconds:0} ms";

        if (timeSpan.TotalSeconds < 60)
            return $"{timeSpan.TotalSeconds:0.###} s";

        if (timeSpan.TotalMinutes < 60)
            return $"{timeSpan.TotalMinutes:0.##} m";

        if (timeSpan.TotalHours < 24)
            return $"{timeSpan.TotalHours:0.##} h";

        return $"{timeSpan.TotalDays:0.##} d";
    }
}
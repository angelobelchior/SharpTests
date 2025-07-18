using System.Text.RegularExpressions;

namespace SharpTests.Models;

public record Time(int Milliseconds)
{
    public static implicit operator Time(string time)
    {
        var regex = GeneratedAmazingRegex.ExtractMillisecondsRegex();
        var match = regex.Match(time);
        if (!match.Success)
            throw new ArgumentException($"Invalid time format: {time}. Expected format: <number><unit>, where unit can be ms, s, m, h, or d.");
        
        var value = int.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Success ? match.Groups[2].Value : "ms";

        return unit switch
        {
            "ms" => new(value),
            "s" => new(value * 1000),
            "m" => new(value * 60 * 1000),
            "h" => new(value * 60 * 60 * 1000),
            "d" => new(value * 24 * 60 * 60 * 1000),
            _ => throw new ArgumentException($"Unknown time unit: {unit}. Expected one of: ms, s, m, h, d.")
        };
    }

    public static implicit operator Time(int milliseconds)
        => new(milliseconds);
}

public partial class GeneratedAmazingRegex
{
    [GeneratedRegex(@"^(\d+)(ms|s|m|h|d)?$")]
    public static partial Regex ExtractMillisecondsRegex();
}
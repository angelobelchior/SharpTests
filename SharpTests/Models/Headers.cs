using System.Net.Http.Headers;

namespace SharpTests.Models;

public class Headers(params (string Key, string Value)[] headers)
{
    internal void SetHeaders(HttpRequestHeaders headers)
    {
        foreach (var (key, value) in headers)
            headers.Add(key, value);
    }

    public static implicit operator Headers(string[] headers)
    {
        if (headers.Length % 2 != 0)
            throw new ArgumentException(
                $"Headers must be provided in key-value pairs. Headers count: {headers.Length}");

        var headerPairs = new List<(string, string)>();
        for (var i = 0; i < headers.Length; i += 2)
        {
            var key = headers.ElementAt(i);
            var value = headers.ElementAt(i + 1);
            headerPairs.Add((key, value));
        }

        return new Headers(headerPairs.ToArray());
    }
}
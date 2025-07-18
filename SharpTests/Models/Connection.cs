namespace SharpTests.Models;

public record Connection(
    TimeSpan ConnectingTime,
    TimeSpan TLSHandshakeTime
);
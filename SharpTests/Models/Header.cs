namespace SharpTests.Models;

/// <summary>
/// Header
/// </summary>
/// <param name="ReasonPhrase">Reason Phrase</param>
/// <param name="ContentType">Content Type</param>
/// <param name="Version">Version</param>
/// <param name="SendingTime">Time to send headers</param>
/// <param name="WaitingTime">Time to first byte (TTFB)</param>
public record Header(
    string ReasonPhrase,
    string ContentType,
    string Version,
    TimeSpan SendingTime,
    TimeSpan WaitingTime);
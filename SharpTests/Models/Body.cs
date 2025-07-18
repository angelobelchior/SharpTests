using Newtonsoft.Json.Linq;

namespace SharpTests.Models;

public record Body(
    long Length,
    string Content,
    TimeSpan ReceivingTime)
{
    public dynamic Payload => !string.IsNullOrEmpty(Content) ? JObject.Parse(Content) : new object();
}
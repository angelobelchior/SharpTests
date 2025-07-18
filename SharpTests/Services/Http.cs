using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text.Json;

namespace SharpTests.Services;

public class Http
{
    private const string DefaultMediaType = "application/json";
    private static readonly System.Text.Encoding DefaultEncoding = System.Text.Encoding.UTF8;

    public Task<Response> Get(string url, Headers? headers = null)
        => Send<object>(HttpMethod.Get, url, headers, null);

    public Task<Response> Post<T>(string url, Headers? headers = null, T? payload = null)
        where T : class
        => Send(HttpMethod.Post, url, headers, payload);

    private async Task<Response> Send<T>(
        HttpMethod method,
        string url,
        Headers? headers,
        T? payload)
        where T : class
    {
        var tcpConnectTime = TimeSpan.Zero;
        var tlsHandshakeTime = TimeSpan.Zero;
        void GetTimeSpanValues(TimeSpan a, TimeSpan b)
        {
            tcpConnectTime = a;
            tlsHandshakeTime = b;
        }
        var handler = CreateSocketsHttpHandler(GetTimeSpanValues);
        var httpClient = new HttpClient(handler);

        var totalRequestStopwatch = Stopwatch.StartNew();

        var request = new HttpRequestMessage(method, url);
        if (payload is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(payload), DefaultEncoding, DefaultMediaType);
        headers?.SetHeaders(request.Headers);

        var sendingStopwatch = Stopwatch.StartNew();
        var response = await httpClient.SendAsync(request);
        sendingStopwatch.Stop();
        var sendingTime = totalRequestStopwatch.Elapsed;

        var stream = await response.Content.ReadAsStreamAsync();
        var firstByteStopwatch = Stopwatch.StartNew();
        var firstByte = stream.ReadByte();
        firstByteStopwatch.Stop();
        var waitingTime = sendingTime + firstByteStopwatch.Elapsed;

        var receivingStopwatch = Stopwatch.StartNew();
        using var reader = new StreamReader(stream);
        var body = (char)firstByte + await reader.ReadToEndAsync();
        receivingStopwatch.Stop();
        var receivingTime = receivingStopwatch.Elapsed;

        totalRequestStopwatch.Stop();
        var totalRequestTime = totalRequestStopwatch.Elapsed;

        return new(
            (int)response.StatusCode,
            response.IsSuccessStatusCode,
            Header: new(
                response.ReasonPhrase ?? string.Empty,
                response.Content.Headers.ContentType?.MediaType ?? string.Empty,
                response.Version.ToString(),
                waitingTime,
                sendingTime),
            Body: new(
                response.Content.Headers.ContentLength.GetValueOrDefault(),
                body,
                receivingTime
            ),
            Connection: new(
                tcpConnectTime,
                tlsHandshakeTime
            ),
            totalRequestTime
        );
    }

    private SocketsHttpHandler CreateSocketsHttpHandler(
        Action<TimeSpan, TimeSpan> getTimeSpanValues)
        => new()
        {
            ConnectCallback = async (context, cancellationToken) =>
            {
                var host = context.DnsEndPoint.Host;
                var port = context.DnsEndPoint.Port;

                var tcpClient = new TcpClient();

                var tcpStopwatch = Stopwatch.StartNew();
                await tcpClient.ConnectAsync(host, port, cancellationToken);
                tcpStopwatch.Stop();
                var tcpConnectTime = tcpStopwatch.Elapsed;

                var networkStream = tcpClient.GetStream();
                var sslStream = new SslStream(networkStream, false);

                var tlsStopwatch = Stopwatch.StartNew();
                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                {
                    TargetHost = host,
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
                }, cancellationToken);
                tlsStopwatch.Stop();
                var tlsHandshakeTime = tlsStopwatch.Elapsed;

                getTimeSpanValues(tcpConnectTime, tlsHandshakeTime);

                return sslStream;
            }
        };
}
namespace SharpTests.Models;

public record Response(
    int StatusCode,
    bool IsSuccessStatusCode,
    Header Header,
    Body Body,
    Connection Connection,
    TimeSpan TotalRequestTime)
{
    public void PrintSummary()
    {
        Console.WriteLine("\n=== Resumo das Estatísticas ===");
        
        Console.WriteLine($"Total Request Time....: {TotalRequestTime.ToWrite()}");
        Console.WriteLine();
        
        Console.WriteLine($"Status Code...........: {StatusCode}");
        Console.WriteLine($"Is Success Status Code: {IsSuccessStatusCode}");
        Console.WriteLine();
        
        Console.WriteLine($"Connecting Time.......: {Connection.ConnectingTime.ToWrite()}");
        Console.WriteLine($"TLS Handshake.........: {Connection.TLSHandshakeTime.ToWrite()}");
        Console.WriteLine();
        
        Console.WriteLine($"Header Sending Time...: {Header.SendingTime.ToWrite()}");
        Console.WriteLine($"Header Waiting Time...: {Header.WaitingTime.ToWrite()}");
        Console.WriteLine();
        
        Console.WriteLine($"Body Receiving Time...: {Body.ReceivingTime.ToWrite()}");
        Console.WriteLine($"Body Length...........: {Body.Length} bytes");
        Console.WriteLine($"Body Content..........: {Body.Content[..10]}");
    }
}

//Falta implementar...
// data_received
// data_sent
// total_requests
// total_iterations

public class ResponseStatistics
{
    public int TotalInteractions { get; }

    public Dictionary<int, double> StatusCodePercentages { get; }
    public double SuccessPercentage { get; }

    public TimeStatistics ConnectingTime { get; }
    public TimeStatistics TLSHandshakeTime { get; }
    public TimeStatistics Request { get; }
    public TimeStatistics HeaderWaitingTime { get; }
    public TimeStatistics HeaderSendingTime { get; }
    public TimeStatistics Body { get; }

    public Dictionary<int, TimeSpan> HeaderSendingTimePercentiles { get; }
    public Dictionary<int, TimeSpan> HeaderWaitingTimePercentiles { get; }
    public Dictionary<int, TimeSpan> BodyReceivingTimePercentiles { get; }
    public Dictionary<int, TimeSpan> ConnectionConnectingTimePercentiles { get; }
    public Dictionary<int, TimeSpan> ConnectionTLSHandshakeTimePercentiles { get; }

    private readonly Options _options;
    private readonly int _totalRequests;

    public ResponseStatistics(Options options, int totalRequests, List<Response> responses)
    {
        _options = options;
        _totalRequests = totalRequests;

        TotalInteractions = responses.Count;
        StatusCodePercentages = responses
            .GroupBy(r => r.StatusCode)
            .ToDictionary(g => g.Key, g => (g.Count() / (double)TotalInteractions) * 100);

        SuccessPercentage = (responses.Count(r => r.IsSuccessStatusCode) / (double)TotalInteractions) * 100;

        ConnectingTime = new(
            Total: TimeSpan.FromTicks(responses.Sum(r => r.Connection.ConnectingTime.Ticks)),
            Average: TimeSpan.FromTicks((long)responses.Average(r => r.Connection.ConnectingTime.Ticks)),
            Min: responses.Min(r => r.Connection.ConnectingTime),
            Max: responses.Max(r => r.Connection.ConnectingTime)
        );

        TLSHandshakeTime = new(
            Total: TimeSpan.FromTicks(responses.Sum(r => r.Connection.TLSHandshakeTime.Ticks)),
            Average: TimeSpan.FromTicks((long)responses.Average(r => r.Connection.TLSHandshakeTime.Ticks)),
            Min: responses.Min(r => r.Connection.TLSHandshakeTime),
            Max: responses.Max(r => r.Connection.TLSHandshakeTime)
        );

        Request = new(
            Total: TimeSpan.FromTicks(responses.Sum(r => r.TotalRequestTime.Ticks)),
            Average: TimeSpan.FromTicks((long)responses.Average(r => r.TotalRequestTime.Ticks)),
            Min: responses.Min(r => r.TotalRequestTime),
            Max: responses.Max(r => r.TotalRequestTime)
        );

        Body = new(
            Total: TimeSpan.FromTicks(responses.Sum(r => r.Body.ReceivingTime.Ticks)),
            Average: TimeSpan.FromTicks((long)responses.Average(r => r.Body.ReceivingTime.Ticks)),
            Min: responses.Min(r => r.Body.ReceivingTime),
            Max: responses.Max(r => r.Body.ReceivingTime)
        );

        HeaderWaitingTime = new(
            Total: TimeSpan.FromTicks(responses.Sum(r => r.Header.WaitingTime.Ticks)),
            Average: TimeSpan.FromTicks((long)responses.Average(r => r.Header.WaitingTime.Ticks)),
            Min: responses.Min(r => r.Header.WaitingTime),
            Max: responses.Max(r => r.Header.WaitingTime)
        );

        HeaderSendingTime = new(
            Total: TimeSpan.FromTicks(responses.Sum(r => r.Header.SendingTime.Ticks)),
            Average: TimeSpan.FromTicks((long)responses.Average(r => r.Header.SendingTime.Ticks)),
            Min: responses.Min(r => r.Header.SendingTime),
            Max: responses.Max(r => r.Header.SendingTime)
        );

        HeaderSendingTimePercentiles = CalculatePercentiles(responses.Select(r => r.Header.SendingTime).ToList());
        HeaderWaitingTimePercentiles = CalculatePercentiles(responses.Select(r => r.Header.WaitingTime).ToList());
        BodyReceivingTimePercentiles = CalculatePercentiles(responses.Select(r => r.Body.ReceivingTime).ToList());
        ConnectionConnectingTimePercentiles =
            CalculatePercentiles(responses.Select(r => r.Connection.ConnectingTime).ToList());
        ConnectionTLSHandshakeTimePercentiles =
            CalculatePercentiles(responses.Select(r => r.Connection.TLSHandshakeTime).ToList());
    }

    private Dictionary<int, TimeSpan> CalculatePercentiles(List<TimeSpan> times)
    {
        var percentiles = new Dictionary<int, TimeSpan>();
        if (!times.Any()) return percentiles;

        var orderedTimes = times.OrderBy(t => t.Ticks).ToList();
        int[] percentilesToCalculate = [95, 96, 97, 98, 99];

        foreach (var percentile in percentilesToCalculate)
        {
            var index = (int)Math.Ceiling((percentile / 100.0) * orderedTimes.Count) - 1;
            percentiles[percentile] = orderedTimes[Math.Max(0, index)];
        }

        return percentiles;
    }

    public void PrintStatistics()
    {
        Console.WriteLine("=== Estatísticas de Respostas ===");
        var text =
        """
        Os percentis (P95, P96, P97, P98 e P99) são medidas estatísticas que indicam o valor abaixo do qual uma certa porcentagem de observações em um conjunto de dados cai. Por exemplo:

        - P98: 98% dos valores estão abaixo desse valor, e apenas 2% estão acima.
        - P99: 99% dos valores estão abaixo desse valor, e apenas 1% estão acima.

        Esses percentis são úteis para identificar valores extremos ou medir o desempenho em cenários de alta carga, como tempos de resposta em sistemas. 
        Por exemplo, o P99 de um tempo de resposta indica o tempo que cobre 99% das requisições, ajudando a entender o comportamento em situações quase no limite.
        """;
        Console.WriteLine(text);
        
        Console.WriteLine("=== === ===");

        Console.WriteLine($"Percentual de Sucesso: {SuccessPercentage:F2}%");
        Console.WriteLine("Percentual por Status Code:");
        foreach (var statusCode in StatusCodePercentages)
            Console.WriteLine($"  Status Code {statusCode.Key}: {statusCode.Value:F2}%");
        Console.WriteLine();

        Console.WriteLine($"Tempo Total de Connecting: {ConnectingTime.Total}");
        Console.WriteLine($"Tempo Médio de Connecting: {ConnectingTime.Average.ToWrite()}");
        Console.WriteLine($"Menor Tempo de Connecting: {ConnectingTime.Min.ToWrite()}");
        Console.WriteLine($"Maior Tempo de Connecting: {ConnectingTime.Max.ToWrite()}");
        Console.WriteLine("Connecting Time:");
        PrintPercentiles(ConnectionConnectingTimePercentiles);
        Console.WriteLine();
        
        Console.WriteLine($"Tempo Total de TLS Handshake: {TLSHandshakeTime.Total}");
        Console.WriteLine($"Tempo Médio de TLS Handshake: {TLSHandshakeTime.Average.ToWrite()}");
        Console.WriteLine($"Menor Tempo de TLS Handshake: {TLSHandshakeTime.Min.ToWrite()}");
        Console.WriteLine($"Maior Tempo de TLS Handshake: {TLSHandshakeTime.Max.ToWrite()}");
        Console.WriteLine("TLS Handshake Time:");
        PrintPercentiles(ConnectionTLSHandshakeTimePercentiles);
        Console.WriteLine();
        
        Console.WriteLine($"Total de Requisições: {_totalRequests}");
        Console.WriteLine($"Tempo Total de Requisições: {Request.Total}");
        Console.WriteLine($"Tempo Médio de Requisição: {Request.Average.ToWrite()}");
        Console.WriteLine($"Menor Tempo de Requisição: {Request.Min.ToWrite()}");
        Console.WriteLine($"Maior Tempo de Requisição: {Request.Max.ToWrite()}");
        Console.WriteLine();

        Console.WriteLine($"Tempo Total de Header Sending: {HeaderSendingTime.Total}");
        Console.WriteLine($"Tempo Médio de Header Sending: {HeaderSendingTime.Average.ToWrite()}");
        Console.WriteLine($"Menor Tempo de Header Sending: {HeaderSendingTime.Min.ToWrite()}");
        Console.WriteLine($"Maior Tempo de Header Sending: {HeaderSendingTime.Max.ToWrite()}");
        Console.WriteLine("Sending Time:");
        PrintPercentiles(HeaderSendingTimePercentiles);
        Console.WriteLine();

        Console.WriteLine($"Tempo Total de Header Waiting: {HeaderWaitingTime.Total}");
        Console.WriteLine($"Tempo Médio de Header Waiting: {HeaderWaitingTime.Average.ToWrite()}");
        Console.WriteLine($"Menor Tempo de Header Waiting: {HeaderWaitingTime.Min.ToWrite()}");
        Console.WriteLine($"Maior Tempo de Header Waiting: {HeaderWaitingTime.Max.ToWrite()}");
        Console.WriteLine("Waiting Time:");
        PrintPercentiles(HeaderWaitingTimePercentiles);
        Console.WriteLine();
    }

    private void PrintPercentiles(Dictionary<int, TimeSpan> percentiles)
    {
        foreach (var percentile in percentiles)
            Console.WriteLine($"  P({percentile.Key}): {percentile.Value.ToWrite()}");
    }
}

public record TimeStatistics(
    TimeSpan Total,
    TimeSpan Average,
    TimeSpan Min,
    TimeSpan Max);
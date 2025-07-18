using System.Collections.Concurrent;

namespace SharpTests.Methods;

public static partial class Global // Test
{
    public static async Task Test(Func<Http, Task<Response>> test)
    {
        var http = new Http();
        var response = await test(http);
        response.PrintSummary();
    }

    public static async Task Test(Options options, Func<Http, Task<Response>> test)
    {
        var responses = new ConcurrentBag<Response>();
        var totalIterations = options.Iterations;
        var chunks = options.Chunks.OrderBy(c => c.Percentual).ToArray();

        var currentVirtualUsers = 0;
        var tasks = new List<Task>();
        var totalTasks = 0;

        Console.WriteLine($"Iniciando teste com {totalIterations} iterações...");

        for (var i = 0; i < totalIterations; i++)
        {
            var progressPercent = (i * 100) / totalIterations;
            foreach (var chunk in chunks)
                if (progressPercent >= chunk.Percentual)
                    currentVirtualUsers = chunk.VirtualUsers;

            Console.WriteLine(
                $"Iteração {i + 1}/{totalIterations} - Progresso: {progressPercent}% - Virtual Users: {currentVirtualUsers}");

            for (var j = 0; j < currentVirtualUsers; j++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var http = new Http();
                    var response = await test(http);
                    responses.Add(response);
                }));
                totalTasks++;
            }

            if (tasks.Count >= currentVirtualUsers)
            {
                Console.WriteLine($"Aguardando conclusão de {tasks.Count} requisições...");
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        if (tasks.Count > 0)
        {
            Console.WriteLine($"Aguardando conclusão das últimas {tasks.Count} requisições...");
            await Task.WhenAll(tasks);
        }

        Console.WriteLine("Teste concluído. Gerando estatísticas...");
        Console.WriteLine();
        
        var responseStatistics = new ResponseStatistics(options, totalTasks, responses.ToList());
        responseStatistics.PrintStatistics();
    }
}
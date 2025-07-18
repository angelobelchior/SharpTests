namespace SharpTests.Methods;

public static partial class Global
{
    public static void Check(Response response, Dictionary<string, Func<Response, bool>> checks)
    {
        foreach (var check in checks)
        {
            var isOk = check.Value(response);
            var label = isOk ? " OK" : "ERR";
            Console.WriteLine($"[{label}]: {check.Key}");
        }
    }
}
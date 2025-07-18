namespace SharpTests.Methods;

public static partial class Global
{
    public static Result Check(Response response, Dictionary<string, Func<Response, bool>> checks)
    {
        foreach (var check in checks)
        {
            var isOk = check.Value(response);
            var label = isOk ? " OK" : "ERR";
            Console.WriteLine($"[{label}]: {check.Key}");

            if (!isOk)
                return new Result(isOk, response, check.Key);
        }

        return new Result(true, response, string.Empty);
    }
}
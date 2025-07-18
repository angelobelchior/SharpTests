namespace SharpTests.Methods;

public static partial class Global
{
    public static Task Sleep(Time time)
        => Task.Delay(time.Milliseconds);
}
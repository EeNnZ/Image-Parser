namespace ParserCore.Tools;

public static class WrappedCall
{
    private static Action<string>? _handler;
    
    public static void SetHandler(Action<string>? handler)
    {
        _handler = handler;
    }

    public static void Action(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _handler?.Invoke(e.ToString());
            throw;
        }
    }

    public static T Function<T>(Func<T> function)
    {
        try
        {
            return function();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _handler?.Invoke(e.ToString());
            throw;
        }
    }
}
using System;

public class Log
{
    private static void WriteLine (ConsoleColor color, string message, params object [] args)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine (string.Format (message, args));
        Console.ForegroundColor = previous;
    }

    public static void Verbose (string message, params object [] args)
    {
        Console.WriteLine (message, args);
    }

    public static void Info (string message, params object [] args)
    {
        Console.WriteLine (message, args);
    }

    public static void Warning (string message, params object [] args)
    {
        Console.WriteLine (message, args);
    }

    public static void Error (string message, params object [] args)
    {
        Console.WriteLine (message, args);
    }
}

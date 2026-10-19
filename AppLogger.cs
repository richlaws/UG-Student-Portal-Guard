using System;
using System.IO;

namespace s;

public static class AppLogger
{
    private static readonly string logFile =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "UG_Automation_Log.txt");

    public static void Info(string message)
    {
        Write("INFO", message);
    }

    public static void Error(string message)
    {
        Write("ERROR", message);
    }

    public static void Warning(string message)
    {
        Write("WARN", message);
    }

    private static void Write(string level, string message)
    {
        try
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(logFile, line + Environment.NewLine);
        }
        catch
        {
            // fail silently (never crash app because of logging)
        }
    }
}

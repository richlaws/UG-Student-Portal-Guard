using System;
using System.Threading;

namespace s;

public static class RetryEngine
{
    public static T Execute<T>(Func<T> action, int retries = 3, int delayMs = 1500)
    {
        Exception lastEx = null;

        for (int i = 0; i < retries; i++)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                lastEx = ex;
                AppLogger.Warning($"Retry {i + 1} failed: {ex.Message}");
                Thread.Sleep(delayMs);
            }
        }

        AppLogger.Error($"Final failure after retries: {lastEx?.Message}");
        throw lastEx;
    }

    public static void Execute(Action action, int retries = 3, int delayMs = 1500)
    {
        Execute(() =>
        {
            action();
            return true;
        }, retries, delayMs);
    }
}

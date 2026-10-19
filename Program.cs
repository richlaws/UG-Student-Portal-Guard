using Avalonia;
using System;

namespace s;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Start browser BEFORE UI
            BrowserManager.StartBrowser();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Startup Error: " + ex.Message);
        }
        finally
        {
            // Clean shutdown
            BrowserManager.CloseBrowser();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}

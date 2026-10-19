using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;

namespace s;

public static class BrowserManager
{
    public static IWebDriver Driver { get; private set; }

    public static void StartBrowser()
{
    if (Driver != null)
        return;

    var options = new ChromeOptions();
    options.PageLoadStrategy = PageLoadStrategy.Eager;
    
    options.AddArgument("--disable-gpu");
    options.AddArgument("--no-sandbox");
    options.AddArgument("--disable-dev-shm-usage");
    options.AddArgument("--window-size=1920,1080");
    // options.AddArgument("--headless");

    int attempts = 0;

    while (attempts < 5)
    {
        try
        {
            attempts++;

            Console.WriteLine(
                $"Attempt {attempts}: Creating ChromeDriver...");

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            Driver = new ChromeDriver(service, options);

            Console.WriteLine(
                $"Attempt {attempts}: ChromeDriver created successfully.");

            Driver.Manage().Timeouts().PageLoad =
                TimeSpan.FromSeconds(10);

            Console.WriteLine(
                $"Attempt {attempts}: Navigating to the UG portal...");

            Driver.Navigate().GoToUrl(
                "https://ienabler.ug.edu.gh/pls/prodi41/w99pkg.mi_login");

            Console.WriteLine(
                $"Attempt {attempts}: Navigation completed.");

            Thread.Sleep(4000);

            string url = Driver.Url;

            Console.WriteLine(
                $"Attempt {attempts}: Current URL: {url}");

            if (!url.Contains("mi_login",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception(
                    "The UG portal login page was not reached.");
            }

            Console.WriteLine("UG Portal loaded successfully.");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Attempt {attempts} failed during browser startup or navigation.");

            Console.WriteLine(
                $"Exception type: {ex.GetType().FullName}");

            Console.WriteLine(
                $"Error details: {ex.Message}");

            try
            {
                Driver?.Quit();
            }
            catch (Exception cleanupError)
            {
                Console.WriteLine(
                    $"Browser cleanup message: {cleanupError.Message}");
            }

            Driver = null;

            if (attempts < 5)
            {
                Console.WriteLine("Retrying in 2 seconds...");
                Thread.Sleep(2000);
            }
        }
    }

    Console.WriteLine(
        "Failed to load the UG portal after 5 attempts.");
}

    public static void CloseBrowser()
    {
        try
        {
            Driver?.Quit();
        }
        catch { }

        Driver = null;
    }
    
    public static void ResetSession()
    {
        try
        {
            Driver?.Quit();
            Driver = null;
        }
        catch { }

        StartBrowser();
        Driver.Navigate().GoToUrl("https://ienabler.ug.edu.gh/pls/prodi41/w99pkg.mi_login");
    }
}

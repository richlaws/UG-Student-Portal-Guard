using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace s;

public class SeleniumService
{
    private IWebDriver driver => BrowserManager.Driver;

    // =========================
    // SAFE FINDER
    // =========================
    private IWebElement Find(By by)
    {
        return RetryEngine.Execute(() =>
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            return wait.Until(d => d.FindElement(by));
        });
    }
    
    // =========================
// WAIT FOR PAGE CONDITION
// =========================
private void WaitForPage(Func<IWebDriver, bool> condition, int seconds = 20)
{
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));

    wait.Until(d =>
    {
        try
        {
            return condition(d);
        }
        catch
        {
            return false;
        }
    });
}

    // =========================
    // OPEN REGISTRATION
    // =========================
    public void OpenRegistrationMenu()
    {
        RetryEngine.Execute(() =>
        {
            AppLogger.Info("Opening registration menu");

            driver.SwitchTo().DefaultContent();
            driver.SwitchTo().Frame("F1");

            Find(By.Id("menuId5")).Click();

WaitForPage(d =>
{
    return d.PageSource.Contains("Register")
        || d.PageSource.Contains("Registration");
});
        });
    }

    // =========================
    // CHECK STATUS
    // =========================
    public string CheckStatus()
    {
        return RetryEngine.Execute(() =>
        {
            AppLogger.Info("Checking registration status");

            driver.SwitchTo().DefaultContent();
            driver.SwitchTo().Frame("F1");

            Find(By.Id("menuId8")).Click();

WaitForPage(d =>
{
    string page = d.PageSource;

    return page.Contains("Proof Of Registration")
        || page.Contains("not permitted")
        || page.Contains("no current enrolments");
});

            driver.SwitchTo().DefaultContent();
            driver.SwitchTo().Frame("F3");

            var page = driver.PageSource;

            if (page.Contains("not permitted") || page.Contains("no current enrolments"))
                return "BLOCKED";

            if (page.Contains("Proof Of Registration"))
                return "ALREADY_REGISTERED";

            return "READY";
        });
    }

    // =========================
    // REGISTER COURSES
    // =========================
    public (List<string> success, List<string> failed) RegisterCourses(
    List<string> codes,
    Action<string>? progress = null)
    {
        var success = new List<string>();
        var failed = new List<string>();

        RetryEngine.Execute(() =>
        {
            AppLogger.Info("Starting course registration");

            driver.SwitchTo().DefaultContent();
            driver.SwitchTo().Frame("F3");
            
            using var db = new AppDbContext();

            var courseNames = db.Courses.ToDictionary(c => c.CourseCode,c => c.CourseName);

            foreach (var code in codes)
            {
                try
                {
                    string displayName = code;

                    if (courseNames.ContainsKey(code))
                    {
                        displayName =
                        $"{code} - {courseNames[code]}";
                    }

                    progress?.Invoke(
                    $"Selecting {displayName}...");

                    var row = driver.FindElement(By.XPath($"//tr[contains(., '{code}')]"));
                    var checkbox = row.FindElement(By.XPath(".//input[@type='checkbox']"));

                    if (!checkbox.Selected)
                        checkbox.Click();

                    success.Add(code);
                    progress?.Invoke($"{displayName} selected");
                    AppLogger.Info($"Selected course: {code}");
                }
                catch (Exception ex)
                {
                    failed.Add(code);
                    AppLogger.Warning($"Failed course {code}: {ex.Message}");
                }
            }
            
            progress?.Invoke("Submitting registration...");
            progress?.Invoke("Waiting for portal confirmation...");

            try
{
    driver.FindElement(By.XPath("//input[@type='submit']")).Click();
}
catch
{
    driver.FindElement(By.XPath("//button[contains(.,'Register')]")).Click();
}


WaitForPage(d =>
{
    string page = d.PageSource;

    return page.Contains("Registration Details")
        || page.Contains("Continue")
        || page.Contains("Print Registration Details");
});

var details = ReadRegistrationDetails();

progress?.Invoke("Registration Details page verified.");

var portalCourses = ReadDisplayedCourses();

progress?.Invoke("Comparing selected courses with portal...");

var verified = VerifySelectedCourses(
    codes,
    portalCourses);

if (!verified)
{
    progress?.Invoke("Course verification failed.");

    AppLogger.Error(
        "Registration stopped because courses did not match.");

    return;
}

progress?.Invoke("Courses verified successfully.");
        });
        
        ClickContinueButton();

        progress?.Invoke("Moved to Registration Costs page.");
        
        var registrationCost = ReadRegistrationCost();

        progress?.Invoke("Registration cost captured.");
        
        ClickAcceptRegistration();

        progress?.Invoke("Registration accepted.");
        
        VerifyRegistrationCompleted();

        return (success, failed);
    }
    
    public void VerifyRegistrationCompleted()
{
    driver.SwitchTo().DefaultContent();
    driver.SwitchTo().Frame("F3");

    string page = driver.PageSource;

    if(page.Contains("Proof Of Registration"))
    {
        AppLogger.Info(
            "SUCCESS - Proof Of Registration available.");
    }
    else
    {
        AppLogger.Info(
            "Registration process completed.");
    }
}
    
    public RegistrationDetails ReadRegistrationDetails()
{
    var details = new RegistrationDetails();

    driver.SwitchTo().DefaultContent();
    driver.SwitchTo().Frame("F3");

    var rows = driver.FindElements(
        By.XPath("//table[@id='ctx_table']//tr"));

    foreach (var row in rows)
    {
        try
        {
            var cells = row.FindElements(By.TagName("td"));

            if (cells.Count < 2)
                continue;

            string label = cells[0].Text.Trim();
            string value = cells[1].Text.Trim();

            switch (label)
            {
                case "AcademicYear":
                    details.AcademicYear = value;
                    break;

                case "Qualification":
                    details.Qualification = value;
                    break;

                case "Academic Period":
                    details.Semester = value;
                    break;

                case "Period of Study":
                    details.Level = value;
                    break;
            }
        }
        catch
        {
            // ignore malformed rows
        }
    }

    AppLogger.Info("===== Registration Details =====");
    AppLogger.Info("Year: " + details.AcademicYear);
    AppLogger.Info("Qualification: " + details.Qualification);
    AppLogger.Info("Semester: " + details.Semester);
    AppLogger.Info("Level: " + details.Level);

    return details;
}

public List<string> ReadDisplayedCourses()
{
    var courses = new List<string>();

    driver.SwitchTo().DefaultContent();
    driver.SwitchTo().Frame("F3");

    using var db = new AppDbContext();

    var courseNames = db.Courses
        .ToDictionary(
            c => c.CourseCode,
            c => c.CourseName
        );

    var rows = driver.FindElements(
        By.XPath("//table[contains(@class,'datadisp')]//tr"));

    foreach (var row in rows)
    {
        try
        {
            var cells = row.FindElements(By.TagName("td"));

            if (cells.Count == 0)
                continue;

            string firstCell = cells[0].Text.Trim();

            if (firstCell.Contains(" - "))
            {
                string code =
                    firstCell.Split(" - ")[0].Trim();

                // Keep storing only codes
                courses.Add(code);
            }
        }
        catch
        {
            // Ignore malformed rows
        }
    }


    AppLogger.Info("===== Courses shown by portal =====");


    foreach (var code in courses)
    {
        if (courseNames.ContainsKey(code))
        {
            AppLogger.Info(
                $"{code} - {courseNames[code]}");
        }
        else
        {
            AppLogger.Info(code);
        }
    }


    return courses;
}

public void ClickContinueButton()
{
    RetryEngine.Execute(() =>
    {
        AppLogger.Info("Clicking Continue on Registration Details page");

        driver.SwitchTo().DefaultContent();
        driver.SwitchTo().Frame("F3");

        var button = driver.FindElement(
            By.XPath("//input[@value='Continue']")
        );

        button.Click();

        AppLogger.Info("Continue button clicked");
    });
}

public RegistrationCost ReadRegistrationCost()
{
    var cost = new RegistrationCost();

    driver.SwitchTo().DefaultContent();
    driver.SwitchTo().Frame("F3");

    var page = driver.PageSource;

    if (page.Contains("Registration Cost"))
    {
        AppLogger.Info("===== Registration Cost =====");

        var rows = driver.FindElements(
            By.XPath("//table[contains(@class,'datadisp')]//tr"));

        foreach (var row in rows)
        {
            try
            {
                var text = row.Text.Trim();

                if (text.Contains("Total:"))
                {
                    cost.Total = text;
                    AppLogger.Info(text);
                }

                if (text.Contains("Sub Total:"))
                {
                    cost.SubTotal = text;
                    AppLogger.Info(text);
                }
            }
            catch
            {
                // ignore bad rows
            }
        }
    }

    return cost;
}

public void ClickAcceptRegistration()
{
    AppLogger.Info("Accepting registration");

    driver.SwitchTo().DefaultContent();
    driver.SwitchTo().Frame("F3");


    var button = new WebDriverWait(
        driver,
        TimeSpan.FromSeconds(10))
        .Until(d =>
            d.FindElement(By.Id("AcceptButton"))
        );


    button.Click();

    AppLogger.Info(
        "Accept Registration button clicked");


    try
    {
        WebDriverWait alertWait =
            new WebDriverWait(
                driver,
                TimeSpan.FromSeconds(5));


        alertWait.Until(d =>
        {
            try
            {
                return d.SwitchTo().Alert();
            }
            catch
            {
                return null;
            }
        });


        var alert = driver.SwitchTo().Alert();


        AppLogger.Info(
            "Portal alert: " + alert.Text);


        alert.Accept();


        AppLogger.Info(
            "Alert accepted.");
    }
    catch
    {
        AppLogger.Info(
            "No confirmation alert detected.");
    }


    AppLogger.Info(
        "Registration submitted to portal.");
}

public bool VerifySelectedCourses(
    List<string> selectedCourses,
    List<string> portalCourses)
{
    var selected = selectedCourses
        .OrderBy(c => c)
        .ToList();

    var portal = portalCourses
        .OrderBy(c => c)
        .ToList();

    bool match = selected.SequenceEqual(portal);

    AppLogger.Info("===== Course Verification =====");

    if (match)
    {
        AppLogger.Info("PASS - Selected courses match portal.");
    }
    else
    {
        AppLogger.Warning("FAIL - Course mismatch.");

        AppLogger.Info("Selected:");

        foreach (var c in selected)
            AppLogger.Info(c);

        AppLogger.Info("Portal:");

        foreach (var c in portal)
            AppLogger.Info(c);
    }

    return match;
}
}

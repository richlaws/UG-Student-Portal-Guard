using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

using System;
using System.IO;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace s;

public partial class MainWindow : Window
{
    private enum AppState
    {
        Idle,
        Verifying,
        WaitingInternet,
        Done
    }

    private AppState state = AppState.Idle;
    private CancellationTokenSource internetWatcher;

    // =========================
    // COURSES
    // =========================
    private List<string> testCourses = new()
    {
        "DCIT400",
        "DCIT402",
        "DCIT406"
    };
    
    private string SelectedSemester =>
    (SemesterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

    private string SelectedLevel =>
    (LevelComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
    
    // =========================
    // CURRENT REGISTRATION SESSION (ADMIN CONTROLLED)
    // =========================
    private string currentAcademicYear = "2026";
    private string currentSemester = "SECOND SEMESTER";
    private string currentQualification = "PSCIT3";

    public MainWindow()
    {
        InitializeComponent();
    }

    // =========================
    // INTERNET CHECK
    // =========================
    private bool IsInternetAvailable()
    {
        try
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        catch
        {
            return false;
        }
    }

    // =========================
    // ALERT HANDLER (CRITICAL FIX)
    // =========================
    private void HandleAlert(IWebDriver driver)
    {
        try
        {
            var alert = driver.SwitchTo().Alert();
            Console.WriteLine("ALERT: " + alert.Text);
            alert.Accept();
            Thread.Sleep(1000);
            driver.SwitchTo().DefaultContent();
        }
        catch (NoAlertPresentException)
        {
            // safe
        }
    }

    // =========================
    // VERIFY BUTTON
    // =========================
    public async void OnVerifyClick(object sender, RoutedEventArgs e)
    {
        var driver = BrowserManager.Driver;

        if (driver == null)
        {
            BrowserManager.ResetSession();
            driver = BrowserManager.Driver;
        }

        var id = StudentIdInput.Text?.Trim();
        var pass = PasswordInput.Text?.Trim();

        if (!IsInternetAvailable())
        {
            StatusLabel.Text = "No internet connection.";
            StatusLabel.Foreground = Brushes.Red;
            
            VerifyButton.IsEnabled = true;
            VerifyButton.Content = "Verify Student";
            // re-enables credentials input after pressing "verify student" button
            StudentIdInput.IsEnabled = true;
            PasswordInput.IsEnabled = true;
            SemesterComboBox.IsEnabled = true;
            LevelComboBox.IsEnabled = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(id) || id.Length != 8)
        {
            StatusLabel.Text = "Invalid Student ID.";
            StatusLabel.Foreground = Brushes.Red;
            
            VerifyButton.IsEnabled = true;
            VerifyButton.Content = "Verify Student";
            // re-enables credentials input after pressing "verify student" button
            StudentIdInput.IsEnabled = true;
            PasswordInput.IsEnabled = true;
            SemesterComboBox.IsEnabled = true;
            LevelComboBox.IsEnabled = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(pass) || pass.Length != 5)
        {
            StatusLabel.Text = "Invalid PIN.";
            StatusLabel.Foreground = Brushes.Red;
            
            VerifyButton.IsEnabled = true;
            VerifyButton.Content = "Verify Student";
            // re-enables credentials input after pressing "verify student" button
            StudentIdInput.IsEnabled = true;
            PasswordInput.IsEnabled = true;
            SemesterComboBox.IsEnabled = true;
            LevelComboBox.IsEnabled = true;
            return;
        }

        state = AppState.Verifying;
        VerifyButton.IsEnabled = false;
        VerifyButton.Content = "Verifying...";
        // disables credentials input after pressing "verify student" button
        StudentIdInput.IsEnabled = false;
        PasswordInput.IsEnabled = false;
        SemesterComboBox.IsEnabled = false;
        LevelComboBox.IsEnabled = false;

        StatusLabel.Text = "Logging into the UG Student Portal...";
        StatusLabel.Foreground = Brushes.Goldenrod;

        bool ok = await ValidateLogin(id, pass);

        if (!ok)
        {
            StatusLabel.Text = "Login failed.";
            StatusLabel.Foreground = Brushes.Red;
            
            VerifyButton.IsEnabled = true;
            VerifyButton.Content = "Verify Student";
            // re-enables credentials input after pressing "verify student" button
            StudentIdInput.IsEnabled = true;
            PasswordInput.IsEnabled = true;
            SemesterComboBox.IsEnabled = true;
            LevelComboBox.IsEnabled = true;
            return;
        }

        StatusLabel.Text =
    "Login successful. Checking your registration status...";
        StatusLabel.Foreground = Brushes.LimeGreen;
        
        // Store the verified student ID in the current session
        // before checking existing registration information.
        Session.StudentId = id;

        try
{

List<RegistrationInfo> portalInfo = await Task.Run(() =>
{
    return ReadRegistrationInformation();
});

// NEW: decide BEFORE continuing navigation
RegistrationInfo matchedRegistration =
    FindExistingRegistration(portalInfo);


if (matchedRegistration != null)
{
    var popup = new RegistrationInfoWindow();

    popup.SetData(matchedRegistration, id);

    popup.Show();


    StatusLabel.Text =
    "Registration already exists for the selected semester.";

    StatusLabel.Foreground =
        Brushes.LimeGreen;


    Console.WriteLine(
        "STOP: user already registered");


    state = AppState.Done;
    
    VerifyButton.IsEnabled = true;
    VerifyButton.Content = "Verify Student";
    // re-enables credentials input after pressing "verify student" button
    StudentIdInput.IsEnabled = true;
    PasswordInput.IsEnabled = true;
    SemesterComboBox.IsEnabled = true;
    LevelComboBox.IsEnabled = true;
    return;
}

StatusLabel.Text =
    "Opening the course registration page...";

StatusLabel.Foreground =
    Brushes.Goldenrod;

bool reachedCoursePage =
    await Task.Run(() =>
        SafeNavigateToSubjectSelection());

if (!reachedCoursePage)
{
    StatusLabel.Text =
    "Unable to open the course selection page.";

    StatusLabel.Foreground =
        Brushes.Red;

    VerifyButton.IsEnabled = true;
    VerifyButton.Content = "Verify Student";
    // re-enables credentials input after pressing "verify student" button
    StudentIdInput.IsEnabled = true;
    PasswordInput.IsEnabled = true;
    SemesterComboBox.IsEnabled = true;
    LevelComboBox.IsEnabled = true;
    return;
}

// Open the academic setup window.
// The student will choose their department,
// level, programme, and semester there.
var setupWindow =
    new StudentSetupWindow();

setupWindow.Show();

StatusLabel.Text =
    "Please select your academic information.";

StatusLabel.Foreground =
    Brushes.Goldenrod;

this.Close();

}
catch (Exception ex)
{
    StatusLabel.Text = "Error in flow.";
    StatusLabel.Foreground = Brushes.Red;
    VerifyButton.IsEnabled = true;
    VerifyButton.Content = "Verify Student";
    // re-enables credentials input after pressing "verify student" button
    StudentIdInput.IsEnabled = true;
    PasswordInput.IsEnabled = true;
    SemesterComboBox.IsEnabled = true;
    LevelComboBox.IsEnabled = true;
    Console.WriteLine(ex);
}
    }

    // =========================
    // LOGIN
    // =========================
    private Task<bool> ValidateLogin(string id, string pin)
    {
        return Task.Run(() =>
        {
            try
            {
                var driver = BrowserManager.Driver;

                driver.SwitchTo().DefaultContent();
                driver.Navigate().GoToUrl(
                    "https://ienabler.ug.edu.gh/pls/prodi41/w99pkg.mi_login"
                );

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

                var idInput = wait.Until(d => d.FindElement(By.CssSelector("input[type='text']")));
                var pinInput = wait.Until(d => d.FindElement(By.CssSelector("input[type='password']")));

                idInput.SendKeys(id);
                pinInput.SendKeys(pin);

                wait.Until(d => d.FindElement(By.CssSelector("input[value='Login']"))).Click();

                Thread.Sleep(3000);

                return driver.Url.Contains("mi_main_menu");
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOGIN ERROR: " + ex.Message);
                return false;
            }
        });
    }
    
    private void ClickSaveAndContinue()
{
    var driver = BrowserManager.Driver;
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

    driver.SwitchTo().DefaultContent();
    driver.SwitchTo().Frame("F3");

    Console.WriteLine("Waiting for Save and Continue...");

    var saveButton = wait.Until(d =>
        d.FindElement(By.XPath("//input[@value='Save and Continue']")));

    Console.WriteLine("Clicking Save and Continue...");

    saveButton.Click();

    wait.Until(d =>
    {
        try
        {
            d.SwitchTo().DefaultContent();
            d.SwitchTo().Frame("F3");

            string page = d.PageSource;

            return page.Contains("Required Registration Credits") &&
                   page.Contains("Save and Continue");
        }
        catch
        {
            return false;
        }
    });
}

    // =========================
    // SAFE NAVIGATION
    // =========================
    private bool SafeNavigateToSubjectSelection()
{
    var driver = BrowserManager.Driver;
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

    // ---------- REGISTRATION ----------
    driver.SwitchTo().DefaultContent();
    driver.SwitchTo().Frame("F1");

    var menu = wait.Until(d => d.FindElement(By.Id("menuId5")));
    
    menu.Click();

Console.WriteLine(driver.Url);
Console.WriteLine(driver.Title);

// Wait until frame F1 is available again
driver.SwitchTo().DefaultContent();

wait.Until(d =>
{
    d.SwitchTo().DefaultContent();

    try
    {
        d.SwitchTo().Frame("F1");

        return d.FindElements(By.Id("menuId7")).Count > 0;
    }
    catch
    {
        return false;
    }
});
    
    if (IsAlreadyRegistered(driver))
{
    Console.WriteLine("Student already registered. Stopping flow.");

    StatusLabel.Text = "Already registered.";
    StatusLabel.Foreground = Brushes.LimeGreen;

    return false;
}

    // ---------- SUBMIT REGISTRATION ----------
    IWebElement submitBtn = null;

wait.Until(d =>
{
    try
    {
        d.SwitchTo().DefaultContent();
        d.SwitchTo().Frame("F1");

        submitBtn = d.FindElement(By.Id("menuId7"));
        return submitBtn.Displayed && submitBtn.Enabled;
    }
    catch
    {
        return false;
    }
});

submitBtn.Click();

Console.WriteLine(driver.Url);
Console.WriteLine(driver.Title);

// Wait briefly to see if an alert appears
try
{
    wait.Until(d =>
    {
        try
        {
            d.SwitchTo().Alert();
            return true;
        }
        catch (NoAlertPresentException)
        {
            return false;
        }
    });

    var alert = driver.SwitchTo().Alert();

    Console.WriteLine("ALERT FOUND:");
    Console.WriteLine(alert.Text);

    alert.Accept();

    // Wait until the alert disappears
    wait.Until(d =>
    {
        try
        {
            d.SwitchTo().Alert();
            return false;
        }
        catch (NoAlertPresentException)
        {
            return true;
        }
    });
}
catch (WebDriverTimeoutException)
{
    Console.WriteLine("No alert found.");
}

    // ---------- RETURN TO FRAME ----------
    driver.SwitchTo().DefaultContent();
    driver.SwitchTo().Frame("F3");

    // ---------- RESTART PROCESS (IF PRESENT) ----------
    try
    {
        var restart = driver.FindElement(
            By.XPath("//input[@value='Restart Process']")
        );

        Console.WriteLine("Restart Process found.");

        restart.Click();

Console.WriteLine(driver.Url);
Console.WriteLine(driver.Title);

// Wait until frame F3 is available again
driver.SwitchTo().DefaultContent();

wait.Until(d =>
{
    try
    {
        d.SwitchTo().DefaultContent();
        d.SwitchTo().Frame("F3");

        return true;
    }
    catch
    {
        return false;
    }
});
    }
    catch (NoSuchElementException)
    {
        Console.WriteLine("Restart Process button not found.");
    }

    // ---------- QUALIFICATION ----------
    Console.WriteLine("Opening Qualification...");

    var qualification = wait.Until(d =>
        d.FindElement(By.XPath("//a[contains(text(),'PSCIT3')]")));

    qualification.Click();

Console.WriteLine(driver.Url);
Console.WriteLine(driver.Title);

// Wait until the "Register for This Qualification" link appears
wait.Until(d =>
{
    try
    {
        d.SwitchTo().DefaultContent();
        d.SwitchTo().Frame("F3");

        return d.FindElements(
            By.PartialLinkText("Register for This Qualification")
        ).Count > 0;
    }
    catch
    {
        return false;
    }
});

    // ---------- REGISTER ----------
Console.WriteLine("Opening Register page...");

var register = wait.Until(d =>
    d.FindElement(By.PartialLinkText("Register for This Qualification")));

register.Click();

// Wait until the Save and Continue button exists
wait.Until(d =>
{
    try
    {
        d.SwitchTo().DefaultContent();
        d.SwitchTo().Frame("F3");

        return d.FindElements(
            By.XPath("//input[@value='Save and Continue']")
        ).Count > 0;
    }
    catch
    {
        return false;
    }
});

// ---------- STUDY CHOICE ----------
ClickSaveAndContinue();

// verify we actually reached study page
var page = driver.PageSource;

bool onStudyPage =
    page.Contains("Save and Continue") &&
    page.Contains("Required Registration Credits");

return onStudyPage;
}

    private bool IsOnCoursePage(IWebDriver driver)
{
    try
    {
        string page = driver.PageSource;

        return page.Contains("Required Registration Credits") &&
               page.Contains("Save and Continue");
    }
    catch
    {
        return false;
    }
}
    // =========================
    // COURSE SELECTION
    // =========================
    private void SelectCourses(List<string> courses)
{
    var driver = BrowserManager.Driver;
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

    driver.SwitchTo().DefaultContent();
    driver.SwitchTo().Frame("F3");
    Thread.Sleep(2000);

    if (!IsOnCoursePage(driver))
    {
        Console.WriteLine("NOT on course page → stopping selection.");
        StatusLabel.Text = "Not on course page.";
        StatusLabel.Foreground = Brushes.Red;
        return;
    }

    int successCount = 0;

    foreach (var course in courses)
    {
        try
        {
            var checkbox = wait.Until(d =>
                d.FindElement(By.CssSelector($"input[value='{course}']"))
            );

            if (!checkbox.Selected)
            {
                checkbox.Click();
                successCount++;
                Console.WriteLine($"Selected: {course}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAILED: {course} → {ex.Message}");
        }
    }

    if (successCount == courses.Count)
    {
        StatusLabel.Text = "ALL courses selected successfully.";
        StatusLabel.Foreground = Brushes.LimeGreen;
    }
    else if (successCount > 0)
    {
        StatusLabel.Text = "Some courses selected. Check logs.";
        StatusLabel.Foreground = Brushes.Orange;
    }
    else
    {
        StatusLabel.Text = "NO courses were selected.";
        StatusLabel.Foreground = Brushes.Red;
    }

    Console.WriteLine($"DONE: {successCount}/{courses.Count} selected");
}

private bool IsAlreadyRegistered(IWebDriver driver)
{
    try
    {
        driver.SwitchTo().DefaultContent();
        driver.SwitchTo().Frame("F3");

        var page = driver.PageSource;

        // Strong indicators of completed registration
        bool registered =
            page.Contains("Registered Courses") ||
            page.Contains("View Registration") ||
            page.Contains("Print Proof") ||
            page.Contains("Registration Complete");

        Console.WriteLine("REG CHECK: " + registered);

        return registered;
    }
    catch (Exception ex)
    {
        Console.WriteLine("REG CHECK ERROR: " + ex.Message);
        return false;
    }
}

private RegistrationInfo FindExistingRegistration(
    List<RegistrationInfo> registrations)
{
    string userSemester = SelectedSemester;
    string userLevel = SelectedLevel;


    Console.WriteLine("Searching registrations...");
    Console.WriteLine("Wanted Semester: " + userSemester);
    Console.WriteLine("Wanted Level: " + userLevel);


    foreach(var reg in registrations)
    {
        Console.WriteLine("----------------");
        Console.WriteLine("Portal Semester: " + reg.Semester);
        Console.WriteLine("Portal Level: " + reg.Level);


        bool semesterMatch =
            reg.Semester.Equals(
                userSemester,
                StringComparison.OrdinalIgnoreCase);


        bool levelMatch =
            reg.Level.Equals(
                userLevel,
                StringComparison.OrdinalIgnoreCase);



        if(semesterMatch && levelMatch)
        {
            Console.WriteLine("MATCH FOUND!");

            return reg;
        }
    }


    Console.WriteLine("NO MATCH FOUND");

    return null;
}


private List<RegistrationInfo> ReadRegistrationInformation()
{
    var driver = BrowserManager.Driver;

    var wait =
        new WebDriverWait(driver, TimeSpan.FromSeconds(20));


    List<RegistrationInfo> results = new();
    
    RegistrationInfo currentRegistration = null;



    try
    {

        driver.SwitchTo().DefaultContent();
        driver.SwitchTo().Frame("F3");


        // open registration information
        var regToggle =
            wait.Until(d =>
            d.FindElement(
            By.XPath("//h6[contains(.,'Registration Information')]")));


        regToggle.Click();


        Thread.Sleep(2000);



        // first visible table
        wait.Until(d => d.FindElement(By.Id("regDiv")));

Console.WriteLine("========== BEFORE CLICKING MORE ==========");

var tablesBefore =
    driver.FindElements(By.XPath("//div[@id='regDiv']//table"));

Console.WriteLine($"Tables found: {tablesBefore.Count}");

try
{
    var moreButton =
        driver.FindElement(
            By.XPath("//h3[contains(.,'More Registration Information')]"));

    Console.WriteLine("Clicking More Registration Information...");

    moreButton.Click();

    Thread.Sleep(3000);
}
catch (Exception ex)
{
    Console.WriteLine("Could not click More Registration Information.");
    Console.WriteLine(ex.Message);
}

Console.WriteLine("========== AFTER CLICKING MORE ==========");

var tablesAfter =
    driver.FindElements(By.XPath("//div[@id='regDiv']//table"));

Console.WriteLine($"Tables found: {tablesAfter.Count}");

int i = 1;

foreach (var table in tablesAfter)
{
    string text = table.Text;
    
    Console.WriteLine();
Console.WriteLine("===== TABLE TYPE =====");

if (text.Contains("Qualification"))
    Console.WriteLine("Qualification Table");

if (text.Contains("Subject"))
    Console.WriteLine("Course Table");

Console.WriteLine("======================");

    Console.WriteLine("--------------------------");
    Console.WriteLine($"TABLE {i++}");
    Console.WriteLine(text);
    
    // ==============================
// REGISTERED COURSES TABLE CHECK
// ==============================

Console.WriteLine("CHECKING COURSE TABLE...");
Console.WriteLine(text);

if(text.Contains("Subject") &&
   text.Contains("Code") &&
   text.Contains("Description"))
{
    Console.WriteLine("COURSE TABLE FOUND");


    if(currentRegistration == null)
    {
        Console.WriteLine(
            "No registration available for courses.");
        
        continue;
    }


    var courseRows =
        table.FindElements(By.TagName("tr"));


    foreach(var row in courseRows)
    {
        var cells =
            row.FindElements(By.XPath("./th|./td"));


        // skip header row
string firstCell = cells.Count > 0
    ? cells[0].Text.Trim()
    : "";

if(cells.Count == 0 ||
   firstCell.Contains("Subject"))
{
    continue;
}


        string code =
            cells[0].Text.Trim();


        if(string.IsNullOrWhiteSpace(code) ||
           code.Length < 5)
        {
            continue;
        }


        RegisteredCourse course =
            new RegisteredCourse();


        course.CourseCode =
            cells[0].Text.Trim();


        course.CourseDescription =
            cells[1].Text.Trim();


        course.AcademicBlock =
            cells[2].Text.Trim();


        course.OfferingType =
            cells[3].Text.Trim();


        course.ClassGroup =
            cells[4].Text.Trim();


        if(cells.Count > 7)
        {
            course.ExamType =
                cells[7].Text.Trim();
        }


        currentRegistration.RegisteredCourses
            .Add(course);


        Console.WriteLine(
            "COURSE FOUND: "
            + course.CourseCode
            + " - "
            + course.CourseDescription);
            
        Console.WriteLine(
    "Courses in current registration: "
    + currentRegistration.RegisteredCourses.Count);
    }
}
    
    Console.WriteLine("ROWS:");

Console.WriteLine("ROWS:");

var debugRows = table.FindElements(By.TagName("tr"));

foreach (var row in debugRows)
{
    Console.WriteLine("----- ROW -----");

    var cells = row.FindElements(By.XPath("./th|./td"));

    foreach (var cell in cells)
    {
        Console.WriteLine(cell.Text);
    }
}


    if(text.Contains("Qualification") &&
       text.Contains("Registration Year") &&
       text.Contains("LEVEL"))
    {
        RegistrationInfo info = new RegistrationInfo();

var rows = table.FindElements(By.TagName("tr"));

foreach (var row in rows)
{
    var cells = row.FindElements(By.XPath("./th|./td"));

    // Qualification row
    if (cells.Count >= 3 &&
        cells[0].Text.Trim() == "Qualification")
    {
        info.QualificationCode =
            cells[1].Text.Trim();

        info.QualificationName =
            cells[2].Text.Trim();
    }

    // Registration Year row
    else if (cells.Count >= 6 &&
             cells[0].Text.Trim() == "Registration Year")
    {
        info.AcademicYear =
            cells[2].Text.Trim();

        string dates =
            cells[5].Text.Trim();

        var parts =
            dates.Split(" to ");

        if (parts.Length == 2)
        {
            info.StartDate = parts[0];
            info.EndDate = parts[1];
        }
        
        Console.WriteLine("START DATE: " + info.StartDate);
Console.WriteLine("END DATE: " + info.EndDate);
    }

    // Period of Study row
    else if (cells.Count >= 3 &&
             cells[0].Text.Trim() == "Period of Study")
    {
        info.Level =
            cells[2].Text.Trim();
    }

    // Academic Block row
    else if (cells.Count >= 3 &&
             cells[0].Text.Trim() == "Academic Block")
    {
        info.Semester =
            cells[2].Text.Trim();
    }
}


        if(text.Contains("FIRST SEMESTER"))
        {
            info.Semester = "FIRST SEMESTER";
        }
        else if(text.Contains("SECOND SEMESTER"))
        {
            info.Semester = "SECOND SEMESTER";
        }


        info.Level =
            ExtractLevel(text);


        results.Add(info);
        
        currentRegistration = info;


        Console.WriteLine("ADDED REGISTRATION:");
        Console.WriteLine(info.Level);
        Console.WriteLine(info.Semester);
        Console.WriteLine(info.AcademicYear);
    }
}



        Console.WriteLine(
        "TOTAL REGISTRATIONS FOUND: "
        + results.Count);
        
        Console.WriteLine();
Console.WriteLine("===== REGISTRATION SUMMARY =====");

foreach (var reg in results)
{
    Console.WriteLine(
        $"{reg.Level} | {reg.Semester}");

    Console.WriteLine(
        $"Courses: {reg.RegisteredCourses.Count}");

    foreach (var c in reg.RegisteredCourses)
    {
        Console.WriteLine(
            $"   {c.CourseCode} - {c.CourseDescription}");
    }

    Console.WriteLine();
}


        return results;


    }
    catch(Exception ex)
    {
        Console.WriteLine(
        "READ REG ERROR "
        + ex.Message);

        return results;
    }
}

    
    // =========================
    // ADMIN
    // =========================
    void OnAdminClick(object sender, RoutedEventArgs e)
    {
        new AdminWindow().Show();
    }
    
    
    private string ExtractAfter(string text, string key)
{
    int index = text.IndexOf(key);
    if (index == -1) return "";

    string result = text.Substring(index + key.Length).Trim();

    // clean extra spaces
    result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");

    return result;
}

private string ExtractLevel(string text)
{
    if (text.Contains("LEVEL"))
    {
        var parts = text.Split("LEVEL");
        if (parts.Length > 1)
            return "LEVEL " + parts[1].Trim().Split(' ')[0];
    }

    return "";
}
}

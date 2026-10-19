using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using System.Net.NetworkInformation;

namespace s;

public partial class CourseSelectionWindow : Window
{
    List<Course> coreCourses;
    List<Course> electiveCourses;

    RegistrationRule currentRule;

    string studentDepartment;
    int studentLevel;
    int studentSemester;

    public CourseSelectionWindow(
        string dept,
        int level,
        int semester)
    {
        InitializeComponent();

        studentDepartment = dept;
        studentLevel = level;
        studentSemester = semester;

        StudentInfoLabel.Text =
            $"Department: {studentDepartment}\n" +
            $"Level: LEVEL {studentLevel}\n" +
            $"Semester: {(studentSemester == 1 ? "FIRST SEMESTER" : "SECOND SEMESTER")}";

        LoadCourses();
    }

    // =========================================================
    // INTERNET CHECK
    // =========================================================
    bool HasInternet()
    {
        return NetworkInterface.GetIsNetworkAvailable();
    }

    // =========================================================
    // LOAD COURSES
    // =========================================================
    void LoadCourses()
    {
        using var db = new AppDbContext();

        currentRule = db.RegistrationRules.FirstOrDefault(r =>
            r.Department == studentDepartment &&
            r.Level == studentLevel &&
            r.Semester == studentSemester);

        if (currentRule == null)
        {
            StatusLabel.Text =
                "No registration rule found.";

            var registerButton =
                this.FindControl<Button>("RegisterButton");

            registerButton.IsEnabled = false;

            return;
        }

        // CLEAR OLD UI FIRST
        CoreCoursesPanel.Children.Clear();
        ElectiveCoursesPanel.Children.Clear();

        coreCourses = db.Courses
            .Where(c => c.IsCore &&
                        c.Department == studentDepartment &&
                        c.Level == studentLevel &&
                        c.Semester == studentSemester)
            .ToList();

        electiveCourses = db.Courses
            .Where(c => !c.IsCore &&
                        c.Department == studentDepartment &&
                        c.Level == studentLevel &&
                        c.Semester == studentSemester)
            .ToList();

        // =====================================================
        // CORE COURSES UI
        // =====================================================
        foreach (var course in coreCourses)
        {
            var cb = new CheckBox
            {
                Content =
                    $"{course.CourseCode} - {course.CourseName} ({course.Credits})",
                Tag = course
            };

            cb.Checked += UpdateCredits;
            cb.Unchecked += UpdateCredits;

            CoreCoursesPanel.Children.Add(cb);
        }

        // =====================================================
        // ELECTIVE COURSES UI
        // =====================================================
        foreach (var course in electiveCourses)
        {
            var cb = new CheckBox
            {
                Content =
                    $"{course.CourseCode} - {course.CourseName} ({course.Credits})",
                Tag = course
            };

            cb.Checked += UpdateCredits;
            cb.Unchecked += UpdateCredits;

            ElectiveCoursesPanel.Children.Add(cb);
        }

        CreditCounter.Text =
            "Credits: 0";

        StatusLabel.Text =
            "Courses loaded successfully.";
    }

    // =========================================================
    // GET SELECTED COURSES
    // =========================================================
    List<Course> GetSelectedCourses()
    {
        var selected = new List<Course>();

        foreach (var cb in CoreCoursesPanel.Children.OfType<CheckBox>())
        {
            if (cb.IsChecked == true)
                selected.Add((Course)cb.Tag);
        }

        foreach (var cb in ElectiveCoursesPanel.Children.OfType<CheckBox>())
        {
            if (cb.IsChecked == true)
                selected.Add((Course)cb.Tag);
        }

        return selected;
    }

    // =========================================================
    // UPDATE CREDITS
    // =========================================================
    void UpdateCredits(
        object sender,
        RoutedEventArgs e)
    {
        int totalCredits = 0;
        int coreCount = 0;
        int electiveCount = 0;

        foreach (var cb in CoreCoursesPanel.Children.OfType<CheckBox>())
        {
            if (cb.IsChecked == true)
            {
                totalCredits +=
                    ExtractCredits(
                        cb.Content?.ToString() ?? "");

                coreCount++;
            }
        }

        foreach (var cb in ElectiveCoursesPanel.Children.OfType<CheckBox>())
        {
            if (cb.IsChecked == true)
            {
                totalCredits +=
                    ExtractCredits(
                        cb.Content?.ToString() ?? "");

                electiveCount++;
            }
        }

        CreditCounter.Text =
            $"Credits: {totalCredits} | Core: {coreCount} | Electives: {electiveCount}";

        ValidateRules(
            totalCredits,
            coreCount,
            electiveCount);
    }

    // =========================================================
    // SAFE CREDIT EXTRACTION
    // =========================================================
    int ExtractCredits(string text)
    {
        try
        {
            var start =
                text.LastIndexOf("(") + 1;

            var end =
                text.LastIndexOf(")");

            var creditText =
                text.Substring(
                    start,
                    end - start);

            return int.Parse(creditText);
        }
        catch
        {
            return 0;
        }
    }

    // =========================================================
    // RULE VALIDATION
    // =========================================================
    void ValidateRules(
        int credits,
        int core,
        int elective)
    {
        var registerButton =
            this.FindControl<Button>("RegisterButton");

        // =====================================================
        // CORE COURSE RULE
        // =====================================================
        if (currentRule.RequireAllCore)
        {
            if (core != coreCourses.Count)
            {
                StatusLabel.Text =
                    $"Please select all {coreCourses.Count} required core courses.";

                registerButton.IsEnabled = false;

                return;
            }
        }
        else
        {
            if (core != currentRule.RequiredCore)
            {
                StatusLabel.Text =
                    $"Please select exactly {currentRule.RequiredCore} core course(s).";

                registerButton.IsEnabled = false;

                return;
            }
        }

        // =====================================================
        // ELECTIVE COURSE RULE
        // =====================================================
        if (elective != currentRule.RequiredElectives)
        {
            StatusLabel.Text =
                $"Please select exactly {currentRule.RequiredElectives} elective course(s).";

            registerButton.IsEnabled = false;

            return;
        }

        // =====================================================
        // MINIMUM CREDITS
        // =====================================================
        if (credits < currentRule.MinCredits)
        {
            StatusLabel.Text =
                $"A minimum of {currentRule.MinCredits} credits is required.";

            registerButton.IsEnabled = false;

            return;
        }

        // =====================================================
        // MAXIMUM CREDITS
        // =====================================================
        if (credits > currentRule.MaxCredits)
        {
            StatusLabel.Text =
                $"You have exceeded the maximum of {currentRule.MaxCredits} credits.";

            registerButton.IsEnabled = false;

            return;
        }

        // =====================================================
        // ALL RULES SATISFIED
        // =====================================================
        StatusLabel.Text =
            "All registration requirements have been satisfied.";

        registerButton.IsEnabled = true;
    }

    // =========================================================
    // REGISTER BUTTON
    // =========================================================
    // Registration does NOT start immediately.
    //
    // First:
    // Course Selection
    //       ↓
    // Registration Summary
    //       ↓
    // Confirm / Cancel
    //
    // Actual Selenium registration happens only after
    // the student confirms the summary.
    // =========================================================
    private void OnRegisterClick(
        object sender,
        RoutedEventArgs e)
    {
        RegisterButton.IsEnabled = false;
        RegisterButton.Content = "Reviewing...";

        try
        {
            var selectedCourses =
                GetSelectedCourses();

            if (selectedCourses.Count == 0)
            {
                StatusLabel.Text =
                    "Please select your courses first.";

                RegisterButton.Content =
                    "REGISTER COURSES";

                RegisterButton.IsEnabled =
                    true;

                return;
            }

            // =================================================
            // CALCULATE TOTAL CREDITS
            // =================================================
            int totalCredits =
                selectedCourses.Sum(c => c.Credits);

            // =================================================
            // BUILD REGISTRATION SUMMARY
            // =================================================
            var summary =
                new RegistrationSummary
                {
                    StudentId =
                        Session.StudentId,

                    Department =
                        studentDepartment,

                    Level =
                        $"LEVEL {studentLevel}",

                    Semester =
                        studentSemester == 1
                            ? "FIRST SEMESTER"
                            : "SECOND SEMESTER",

                    TotalCredits =
                        totalCredits,

                    SelectedCourses =
                        selectedCourses
                            .Select(c =>
                                $"{c.CourseCode} - {c.CourseName} - {c.Credits} Credits")
                            .ToList()
                };

            // =================================================
            // OPEN SUMMARY WINDOW
            // =================================================
            var summaryWindow =
                new RegistrationSummaryWindow();

            summaryWindow.LoadSummary(summary);

            // =================================================
            // RECEIVE USER DECISION
            // =================================================
            summaryWindow.DecisionMade +=
                confirmed =>
                {
                    if (confirmed)
                    {
                        // User confirmed the summary.
                        // Now start actual registration.
                        var courseCodes =
                            selectedCourses
                                .Select(c => c.CourseCode)
                                .ToList();

                        PerformRegistration(
                            courseCodes);
                    }
                    else
                    {
                        // User cancelled.
                        StatusLabel.Text =
                            "Registration cancelled. You can review your courses again.";

                        RegisterButton.Content =
                            "REGISTER COURSES";

                        RegisterButton.IsEnabled =
                            true;
                    }
                };

            StatusLabel.Text =
                "Please review your registration summary.";

            summaryWindow.Show();
        }
        catch (Exception ex)
        {
            StatusLabel.Text =
                "Unable to open registration summary: " +
                ex.Message;

            RegisterButton.Content =
                "REGISTER COURSES";

            RegisterButton.IsEnabled =
                true;
        }
    }

    // =========================================================
    // ACTUAL REGISTRATION
    // =========================================================
    private void PerformRegistration(
        List<string> selectedCourseCodes)
    {
        RegisterButton.IsEnabled = false;
        RegisterButton.Content = "Registering...";

        try
        {
            var bot =
                new SeleniumService();

            StatusLabel.Text =
                "Registration confirmed. Registering courses...";

            var result =
                bot.RegisterCourses(
                    selectedCourseCodes,
                    message =>
                    {
                        StatusLabel.Text =
                            message;
                    });

            // =================================================
            // SOME COURSES FAILED
            // =================================================
            if (result.failed.Count > 0)
            {
                StatusLabel.Text =
                    $"Some courses could not be registered: " +
                    $"{string.Join(", ", result.failed)}";

                RegisterButton.Content =
                    "REGISTER COURSES";

                RegisterButton.IsEnabled =
                    true;
            }
            else
            {
                // =================================================
                // SAVE SUCCESSFUL REGISTRATIONS TO DATABASE
                // =================================================
                using var db =
                    new AppDbContext();

                foreach (var courseCode in result.success)
                {
                    bool alreadyExists =
                        db.RegisteredCourses.Any(r =>
                            r.StudentId == Session.StudentId &&
                            r.Department == Session.Department &&
                            r.Level == Session.Level &&
                            r.Semester == Session.Semester &&
                            r.CourseCode == courseCode);

                    if (alreadyExists)
                        continue;

                    db.RegisteredCourses.Add(
                        new RegisteredCourse
                        {
                            StudentId =
                                Session.StudentId,

                            Department =
                                Session.Department,

                            Level =
                                Session.Level,

                            Semester =
                                Session.Semester,

                            CourseCode =
                                courseCode,

                            RegisteredOn =
                                DateTime.Now,

                            Status =
                                "Registered"
                        });
                }

                db.SaveChanges();

                StatusLabel.Text =
                    "Course registration completed successfully.";

                RegisterButton.Content =
                    "REGISTER COURSES";

                RegisterButton.IsEnabled =
                    true;
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text =
                "Registration failed: " +
                ex.Message;

            RegisterButton.Content =
                "REGISTER COURSES";

            RegisterButton.IsEnabled =
                true;
        }
    }

    // =========================================================
    // BACK BUTTON
    // =========================================================
    private void OnBackClick(
        object sender,
        RoutedEventArgs e)
    {
        var setup =
            new StudentSetupWindow();

        setup.Show();

        this.Close();
    }

    // =========================================================
    // RETRY BUTTON
    // =========================================================
    private void OnRetryClick(
        object sender,
        RoutedEventArgs e)
    {
        LoadCourses();
    }

    // =========================================================
    // MY REGISTERED COURSES
    // =========================================================
    private void OnMyRegisteredCoursesClick(
        object? sender,
        RoutedEventArgs e)
    {
        var window =
            new RegisteredCoursesWindow();

        window.Show();
    }
}

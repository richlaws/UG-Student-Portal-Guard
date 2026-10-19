using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace s;

public partial class RegistrationSummaryWindow : Window
{
    // Tells the calling window whether the user confirmed or cancelled.
    public event Action<bool>? DecisionMade;

    public RegistrationSummaryWindow()
    {
        InitializeComponent();

        ConfirmButton.Click += OnConfirmClick;
        CancelButton.Click += OnCancelClick;
    }

    public void LoadSummary(RegistrationSummary summary)
    {
        StudentLabel.Text =
            $"Student ID: {summary.StudentId}";

        DepartmentLabel.Text =
            $"Department: {summary.Department}";

        LevelLabel.Text =
            $"Level: {summary.Level}";

        SemesterLabel.Text =
            $"Semester: {summary.Semester}";

        CreditLabel.Text =
            $"Total Credits: {summary.TotalCredits}";

        CourseList.Items.Clear();

        foreach (var course in summary.SelectedCourses)
        {
            CourseList.Items.Add(course);
        }

        StatusLabel.Text =
            "Please review your selected courses before proceeding.";

        ConfirmButton.IsEnabled = true;
    }

    private void OnConfirmClick(
        object? sender,
        RoutedEventArgs e)
    {
        // Tell the Course Selection window that
        // the student confirmed the registration.
        DecisionMade?.Invoke(true);

        Close();
    }

    private void OnCancelClick(
        object? sender,
        RoutedEventArgs e)
    {
        // Tell the Course Selection window that
        // the student cancelled.
        DecisionMade?.Invoke(false);

        Close();
    }
}

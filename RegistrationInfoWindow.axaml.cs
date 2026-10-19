using Avalonia.Controls;
using Avalonia.Interactivity;

namespace s;

public partial class RegistrationInfoWindow : Window
{
    public RegistrationInfoWindow()
    {
        InitializeComponent();
    }

    public void SetData(
        RegistrationInfo info,
        string studentId)
    {
        StudentText.Text =
            "Student ID:\n" +
            studentId;

        QualText.Text =
            "Qualification:\n" +
            info.QualificationCode + " " +
            info.QualificationName;

        YearText.Text =
            "Academic Year:\n" +
            info.AcademicYear;

        SemText.Text =
            "Semester:\n" +
            info.Semester;

        LevelText.Text =
            "Study Level:\n" +
            info.Level;

        StartDateText.Text =
            "Start:\n" +
            info.StartDate;

        EndDateText.Text =
            "End:\n" +
            info.EndDate;

        // =========================
        // REGISTERED COURSES
        // =========================

        if (info.RegisteredCourses.Count > 0)
        {
            string courses = "";

            foreach (var course in info.RegisteredCourses)
            {
                courses +=
                    course.CourseCode
                    + " - "
                    + course.CourseDescription
                    + "\n";
            }

            CoursesText.Text =
                courses;
        }
        else
        {
            CoursesText.Text =
                "No registered courses found.";
        }
    }

    private void ViewRegisteredCourses_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var window =
            new RegisteredCoursesWindow();

        window.Show();
    }

    private void Close_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close();
    }
}

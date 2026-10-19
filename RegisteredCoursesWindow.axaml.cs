using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Linq;

namespace s;

public partial class RegisteredCoursesWindow : Window
{
    public RegisteredCoursesWindow()
    {
        InitializeComponent();

        LoadCourses();
    }

    private void LoadCourses()
    {
        using var db = new AppDbContext();

        var registrations =
        (
            from r in db.RegisteredCourses
            join c in db.Courses
                on new
                {
                    r.Department,
                    r.Level,
                    r.Semester,
                    r.CourseCode
                }
                equals new
                {
                    c.Department,
                    c.Level,
                    c.Semester,
                    c.CourseCode
                }
            where r.StudentId == Session.StudentId
            orderby r.CourseCode
            select new
            {
                c.CourseCode,
                c.CourseName,
                c.Credits,
                c.IsCore,
                r.Status,
                r.RegisteredOn
            }
        ).ToList();

        RegisteredCoursesList.Items.Clear();

        foreach (var reg in registrations)
        {
            string type =
                reg.IsCore
                    ? "CORE"
                    : "ELECTIVE";

            RegisteredCoursesList.Items.Add(
                $"{reg.CourseCode} - {reg.CourseName}\n" +
                $"{type} | {reg.Credits} Credits\n" +
                $"{reg.Status}\n" +
                $"{reg.RegisteredOn:g}");
        }
    }

    private void OnCloseClick(
        object? sender,
        RoutedEventArgs e)
    {
        Close();
    }
}

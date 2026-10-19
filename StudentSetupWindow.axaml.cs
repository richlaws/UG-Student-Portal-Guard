using Avalonia.Controls;
using Avalonia.Interactivity;

namespace s;

public partial class StudentSetupWindow : Window
{
    public StudentSetupWindow()
    {
        InitializeComponent();
    }

    private void OnContinueClick(
        object sender,
        RoutedEventArgs e)
    {
        if (DepartmentBox.SelectedItem == null ||
            LevelBox.SelectedItem == null ||
            ProgrammeBox.SelectedItem == null ||
            SemesterBox.SelectedItem == null)
        {
            StatusLabel.Text =
                "Please select all fields.";

            return;
        }

        // Get values from the academic setup UI.
        var department =
            ((ComboBoxItem)DepartmentBox.SelectedItem)
            .Content?
            .ToString() ?? "";

        var level =
            int.Parse(
                ((ComboBoxItem)LevelBox.SelectedItem)
                .Content?
                .ToString() ?? "0"
            );

        var semesterText =
            ((ComboBoxItem)SemesterBox.SelectedItem)
            .Content?
            .ToString() ?? "";

        int semester =
            semesterText.Contains("First")
                ? 1
                : 2;

        // Store the selected academic information
        // in the current student session.
        Session.Department = department;
        Session.Level = level;
        Session.Semester = semester;

        // Pass the same selected information
        // to the course selection window.
        var courseWindow =
            new CourseSelectionWindow(
                department,
                level,
                semester);

        courseWindow.Show();

        this.Close();
    }
}

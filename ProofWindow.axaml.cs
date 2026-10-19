using Avalonia.Controls;

namespace s;

public partial class ProofWindow : Window
{
    public ProofWindow(string studentInfo, string courses)
    {
        InitializeComponent();

        StudentInfoText.Text = studentInfo;
        CourseText.Text = courses;
    }

    private void OnCloseClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
}

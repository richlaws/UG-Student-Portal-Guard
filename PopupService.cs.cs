using Avalonia.Controls;
using System.Threading.Tasks;

namespace s;

public static class PopupService
{
    public static async Task Show(Window parent, string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 350,
            Height = 180,
            Content = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(20)
            }
        };

        await dialog.ShowDialog(parent);
    }
}

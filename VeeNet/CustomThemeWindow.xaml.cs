using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VeeNet.Services;
using DrawingColor = System.Drawing.Color;
using WpfColor = System.Windows.Media.Color;

namespace VeeNet;

public partial class CustomThemeWindow : Window
{
    public BarTheme? ResultTheme { get; private set; }

    public CustomThemeWindow()
    {
        InitializeComponent();
        UpdatePreviews();
    }

    private void Hex_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdatePreviews();
    }

    private void UpdatePreviews()
    {
        UpdatePreview(Hex1, Preview1);
        UpdatePreview(Hex2, Preview2);
        UpdatePreview(Hex3, Preview3);
    }

    private static void UpdatePreview(System.Windows.Controls.TextBox box, System.Windows.Controls.Border preview)
    {
        if (box == null || preview == null) return;

        try
        {
            var text = box.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                if (!text.StartsWith('#')) text = "#" + text;
                var color = (WpfColor)System.Windows.Media.ColorConverter.ConvertFromString(text);
                preview.Background = new SolidColorBrush(color);
                return;
            }
        }
        catch { }

        preview.Background = new SolidColorBrush(WpfColor.FromRgb(51, 51, 51));
    }

    private static DrawingColor? ParseHex(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        text = text.Trim();
        if (!text.StartsWith('#')) text = "#" + text;

        try
        {
            var c = (WpfColor)System.Windows.Media.ColorConverter.ConvertFromString(text);
            return DrawingColor.FromArgb(255, c.R, c.G, c.B);
        }
        catch
        {
            return null;
        }
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        var c1 = ParseHex(Hex1.Text);
        if (c1 == null)
        {
            System.Windows.MessageBox.Show("Color 1 is required and must be a valid hex code.",
                "Invalid Color", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var colors = new List<DrawingColor> { c1.Value };

        var c2 = ParseHex(Hex2.Text);
        if (c2 != null) colors.Add(c2.Value);

        var c3 = ParseHex(Hex3.Text);
        if (c3 != null) colors.Add(c3.Value);

        ResultTheme = new BarTheme("Custom", colors.ToArray());
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

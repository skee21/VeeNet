using System.Drawing;

namespace VeeNet.Services;

public class BarTheme
{
    public string Name { get; }
    public Color[] Colors { get; }

    public BarTheme(string name, params Color[] colors)
    {
        if (colors.Length < 1 || colors.Length > 3)
            throw new ArgumentException("A theme requires 1 to 3 colors.");
        Name = name;
        Colors = colors;
    }

    public Color GetColor(float normalizedHeight)
    {
        if (Colors.Length == 1)
            return Colors[0];

        if (Colors.Length == 2)
            return normalizedHeight < 0.5f ? Colors[0] : Colors[1];

        if (normalizedHeight < 0.4f) return Colors[0];
        if (normalizedHeight < 0.7f) return Colors[1];
        return Colors[2];
    }

    public static readonly BarTheme[] Presets =
    [
        new("Neon (Default)",
            Color.FromArgb(255, 0, 230, 118),
            Color.FromArgb(255, 255, 214, 10),
            Color.FromArgb(255, 255, 55, 95)),

        new("Ocean",
            Color.FromArgb(255, 0, 188, 212),
            Color.FromArgb(255, 38, 166, 255),
            Color.FromArgb(255, 101, 31, 255)),


        new("Monochrome Green",
            Color.FromArgb(255, 0, 230, 118)),

        new("Cyberpunk",
            Color.FromArgb(255, 0, 255, 255),
            Color.FromArgb(255, 255, 0, 255)),

        new("Sakura",
            Color.FromArgb(255, 255, 183, 197),
            Color.FromArgb(255, 255, 105, 180),
            Color.FromArgb(255, 199, 21, 133)),
    ];
}

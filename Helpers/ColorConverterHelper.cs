using System;
using System.Windows.Media;

public static class ColorConverterHelper
{
    public static System.Drawing.Color ToDrawingColor(SolidColorBrush brush)
    {
        if (brush == null)
            throw new ArgumentNullException(nameof(brush));

        var c = brush.Color;
        return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
    }

    public static SolidColorBrush ToSolidColorBrush(System.Drawing.Color color)
    {
        return new SolidColorBrush(
            System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B)
        );
    }
}

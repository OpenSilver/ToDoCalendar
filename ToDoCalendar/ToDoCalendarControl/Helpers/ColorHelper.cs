using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ToDoCalendarControl.Helpers;

public static class ColorHelper
{
    private static readonly Dictionary<Color, Color> _cache = [];

    public static Color AdjustCalendarColor(Color inputColor)
    {
        if (_cache.TryGetValue(inputColor, out Color cachedColor))
        {
            return cachedColor;
        }

        ColorToHSL(inputColor, out float h, out float s, out float l);

        // Adjust only if the color is too bright
        if (l > 0.75f)
        {
            l = 0.6f; // Reduce brightness
            s = Math.Min(1.0f, s * 1.2f); // Increase saturation for vividness
        }

        Color adjustedColor = HSLToColor(h, s, l);
        _cache[inputColor] = adjustedColor; // Store result in cache

        return adjustedColor;
    }

    private static void ColorToHSL(Color color, out float h, out float s, out float l)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float delta = max - min;

        h = 0f;
        if (delta > 0)
        {
            if (max == r)
                h = (g - b) / delta + (g < b ? 6 : 0);
            else if (max == g)
                h = (b - r) / delta + 2;
            else
                h = (r - g) / delta + 4;
            h /= 6;
        }

        l = (max + min) / 2f;
        s = delta == 0 ? 0 : delta / (1 - Math.Abs(2 * l - 1));
    }

    private static Color HSLToColor(float h, float s, float l)
    {
        float r, g, b;

        if (s == 0)
        {
            r = g = b = l; // Achromatic
        }
        else
        {
            float q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            float p = 2 * l - q;
            r = HueToRGB(p, q, h + 1f / 3f);
            g = HueToRGB(p, q, h);
            b = HueToRGB(p, q, h - 1f / 3f);
        }

        return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private static float HueToRGB(float p, float q, float t)
    {
        if (t < 0)
            t += 1;
        if (t > 1)
            t -= 1;
        if (t < 1f / 6f)
            return p + (q - p) * 6 * t;
        if (t < 1f / 2f)
            return q;
        if (t < 2f / 3f)
            return p + (q - p) * (2f / 3f - t) * 6;
        return p;
    }
}


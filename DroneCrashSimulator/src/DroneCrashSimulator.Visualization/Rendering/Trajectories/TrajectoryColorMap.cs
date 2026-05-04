using Raylib_cs;

namespace DroneCrashSimulator.Visualization.Rendering.Trajectories;

public static class TrajectoryColorMap
{
    public static Color GetColorForProgress(float progress, float alpha)
    {
        var r = (byte)(100 + (int)(155 * progress));
        var g = (byte)(200 - (int)(100 * progress));
        var b = (byte)(255 - (int)(155 * progress));
        var a = (byte)(alpha * 255);
        return new Color(r, g, b, a);
    }

    public static Color GetColorForTrajectoryIndex(int index, int totalCount, float alpha)
    {
        var hue = (float)index / totalCount * 360.0f;
        return ColorFromHsv(hue, 0.7f, 0.9f, alpha);
    }

    private static Color ColorFromHsv(float hueDegrees, float saturation, float value, float alpha)
    {
        var c = value * saturation;
        var x = c * (1.0f - Math.Abs(hueDegrees / 60.0f % 2.0f - 1.0f));
        var m = value - c;

        var (r1, g1, b1) = hueDegrees switch
        {
            < 60 => (c, x, 0f),
            < 120 => (x, c, 0f),
            < 180 => (0f, c, x),
            < 240 => (0f, x, c),
            < 300 => (x, 0f, c),
            _ => (c, 0f, x)
        };

        return new Color(
            (byte)((r1 + m) * 255),
            (byte)((g1 + m) * 255),
            (byte)((b1 + m) * 255),
            (byte)(alpha * 255));
    }
}

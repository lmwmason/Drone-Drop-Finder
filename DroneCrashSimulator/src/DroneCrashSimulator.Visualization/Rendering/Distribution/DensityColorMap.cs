using Raylib_cs;

namespace DroneCrashSimulator.Visualization.Rendering.Distribution;

public static class DensityColorMap
{
    public static Color GetColorForDensity(float normalizedDensity)
    {
        byte r = (byte)(50 + (int)(200 * normalizedDensity));
        byte g = (byte)(50 + (int)(50 * (1.0f - normalizedDensity)));
        byte b = (byte)(200 - (int)(150 * normalizedDensity));
        byte a = 220;
        return new Color(r, g, b, a);
    }
}

using System.Numerics;
using Raylib_cs;
using DroneCrashSimulator.Domain.Statistics;
using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Visualization.Rendering.Distribution;

public sealed class LandingHistogram3DRenderer
{
    private const float CellSizeMeters = 4.0f;
    private const float MaxBarHeightMeters = 18.0f;
    private const float BarFillRatio = 0.88f;

    public void Render(IReadOnlyList<CrashPoint> crashPoints, ConfidenceRegion confidenceRegion)
    {
        if (crashPoints.Count == 0) return;

        var grid = BuildDensityGrid(crashPoints);
        var maxCount = grid.Values.Max();

        foreach (var ((cellX, cellY), count) in grid)
        {
            var normalizedDensity = (float)count / maxCount;
            var barHeight = Math.Max(0.1f, normalizedDensity * MaxBarHeightMeters);
            var centerX = (cellX + 0.5f) * CellSizeMeters;
            var centerZ = (cellY + 0.5f) * CellSizeMeters;
            var center = new Vector3(centerX, barHeight * 0.5f, centerZ);
            var barWidth = CellSizeMeters * BarFillRatio;

            var fillColor = ComputeBarColor(normalizedDensity);
            Raylib_cs.Raylib.DrawCube(center, barWidth, barHeight, barWidth, fillColor);
            Raylib_cs.Raylib.DrawCubeWires(center, barWidth, barHeight, barWidth,
                new Color((byte)0, (byte)0, (byte)0, (byte)80));
        }

        new ConfidenceCircleRenderer().Render(confidenceRegion);
    }

    private static Dictionary<(int, int), int> BuildDensityGrid(IReadOnlyList<CrashPoint> crashPoints)
    {
        var grid = new Dictionary<(int, int), int>();
        foreach (var point in crashPoints)
        {
            var cellX = (int)Math.Floor(point.XMeters / CellSizeMeters);
            var cellY = (int)Math.Floor(point.YMeters / CellSizeMeters);
            grid.TryGetValue((cellX, cellY), out var current);
            grid[(cellX, cellY)] = current + 1;
        }
        return grid;
    }

    private static Color ComputeBarColor(float normalizedDensity)
    {
        byte r, g, b;
        if (normalizedDensity < 0.5f)
        {
            var t = normalizedDensity * 2.0f;
            r = 0;
            g = (byte)(int)(255 * t);
            b = (byte)(int)(255 * (1.0f - t));
        }
        else
        {
            var t = (normalizedDensity - 0.5f) * 2.0f;
            r = (byte)(int)(255 * t);
            g = (byte)(int)(255 * (1.0f - t));
            b = 0;
        }
        byte a = 210;
        return new Color(r, g, b, a);
    }
}

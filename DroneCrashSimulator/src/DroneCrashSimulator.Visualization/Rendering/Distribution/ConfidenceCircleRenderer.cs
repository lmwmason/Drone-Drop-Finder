using System.Numerics;
using Raylib_cs;
using DroneCrashSimulator.Domain.Statistics;

namespace DroneCrashSimulator.Visualization.Rendering.Distribution;

public sealed class ConfidenceCircleRenderer
{
    private const int CircleSegments = 64;
    private const float CircleHeightMeters = 0.2f;
    private static readonly Color CircleColor = new(255, 200, 0, 200);
    private static readonly Color CenterColor = new(255, 100, 100, 220);
    private const float CenterRadius = 0.5f;

    public void Render(ConfidenceRegion region)
    {
        var center = new Vector3(
            (float)region.CenterXMeters,
            CircleHeightMeters,
            (float)region.CenterYMeters);

        Raylib_cs.Raylib.DrawCircle3D(
            center,
            (float)region.RadiusMeters,
            Vector3.UnitX,
            90.0f,
            CircleColor);

        Raylib_cs.Raylib.DrawSphere(center, CenterRadius, CenterColor);
    }
}

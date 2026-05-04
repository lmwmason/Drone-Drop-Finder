using System.Numerics;
using Raylib_cs;
using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Visualization.Rendering.Distribution;

public sealed class ScatterRenderer
{
    private const float PointRadius = 0.3f;
    private const float HeightAboveGroundMeters = 0.15f;

    public void Render(IReadOnlyList<CrashPoint> crashPoints)
    {
        if (crashPoints.Count == 0) return;

        var maxDistance = crashPoints.Max(p => p.DistanceFromOriginMeters);
        if (maxDistance < double.Epsilon) maxDistance = 1.0;

        foreach (var point in crashPoints)
        {
            var normalizedDistance = (float)(point.DistanceFromOriginMeters / maxDistance);
            var color = DensityColorMap.GetColorForDensity(normalizedDistance);
            var worldPosition = new Vector3(
                (float)point.XMeters,
                HeightAboveGroundMeters,
                (float)point.YMeters);
            Raylib_cs.Raylib.DrawSphere(worldPosition, PointRadius, color);
        }
    }
}

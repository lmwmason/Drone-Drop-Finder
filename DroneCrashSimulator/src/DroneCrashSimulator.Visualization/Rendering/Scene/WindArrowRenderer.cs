using System.Numerics;
using Raylib_cs;
using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Visualization.Rendering.Scene;

public sealed class WindArrowRenderer
{
    private const float ArrowBaseHeightMeters = 2.0f;
    private const float ArrowScale = 3.0f;
    private static readonly Color WindArrowColor = new(100, 180, 255, 200);

    public void Render(Velocity windVelocity)
    {
        var windMagnitude = windVelocity.MagnitudeMetersPerSecond;
        if (windMagnitude < 0.1) return;

        var arrowLength = (float)windMagnitude * ArrowScale;
        var directionX = (float)(windVelocity.XMetersPerSecond / windMagnitude);
        var directionY = (float)(windVelocity.YMetersPerSecond / windMagnitude);

        var arrowBase = new Vector3(-directionX * arrowLength * 0.5f, ArrowBaseHeightMeters,
            -directionY * arrowLength * 0.5f);
        var arrowTip = new Vector3(directionX * arrowLength * 0.5f, ArrowBaseHeightMeters,
            directionY * arrowLength * 0.5f);

        Raylib_cs.Raylib.DrawLine3D(arrowBase, arrowTip, WindArrowColor);
        Raylib_cs.Raylib.DrawSphere(arrowTip, 0.5f, WindArrowColor);
    }
}

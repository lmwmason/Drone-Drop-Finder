using System.Numerics;
using Raylib_cs;

namespace DroneCrashSimulator.Visualization.Rendering.Scene;

public sealed class GroundGridRenderer
{
    private const int GridSlices = 40;
    private const float GridSpacingMeters = 5.0f;
    private static readonly Color GridColor = new(50, 50, 50, 255);

    public void Render()
    {
        Raylib_cs.Raylib.DrawGrid(GridSlices, GridSpacingMeters);

        var gridHalfExtent = GridSlices * GridSpacingMeters * 0.5f;
        for (var i = -GridSlices; i <= GridSlices; i += 4)
        {
            var offset = i * GridSpacingMeters * 0.5f;
            Raylib_cs.Raylib.DrawLine3D(
                new Vector3(-gridHalfExtent, 0, offset),
                new Vector3(gridHalfExtent, 0, offset),
                GridColor);
            Raylib_cs.Raylib.DrawLine3D(
                new Vector3(offset, 0, -gridHalfExtent),
                new Vector3(offset, 0, gridHalfExtent),
                GridColor);
        }
    }
}

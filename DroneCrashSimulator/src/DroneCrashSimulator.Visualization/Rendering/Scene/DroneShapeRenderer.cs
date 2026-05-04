using System.Numerics;
using Raylib_cs;
using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Visualization.Rendering.Scene;

public sealed class DroneShapeRenderer
{
    private static readonly Color DroneColor = new(200, 200, 50, 180);

    public void RenderAtPosition(Position position, IDroneModel drone)
    {
        var worldPosition = new Vector3(
            (float)position.XMeters,
            (float)position.ZMeters,
            (float)position.YMeters);

        Raylib_cs.Raylib.DrawCubeWires(
            worldPosition,
            (float)drone.Shape.WidthMeters,
            (float)drone.Shape.HeightMeters,
            (float)drone.Shape.LengthMeters,
            DroneColor);
    }
}

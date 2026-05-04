using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using DroneCrashSimulator.Domain.Drones;
using BepuSim = BepuPhysics.Simulation;

namespace DroneCrashSimulator.Physics.Bodies;

public static class ShapeFactory
{
    public static TypedIndex AddDroneShape(BepuSim simulation, DroneShape shape)
    {
        var box = new Box(
            (float)shape.WidthMeters,
            (float)shape.HeightMeters,
            (float)shape.LengthMeters);
        return simulation.Shapes.Add(box);
    }

    public static BodyInertia ComputeDroneInertia(IDroneModel drone)
    {
        var box = new Box(
            (float)drone.Shape.WidthMeters,
            (float)drone.Shape.HeightMeters,
            (float)drone.Shape.LengthMeters);
        return box.ComputeInertia((float)drone.MassKilograms);
    }
}

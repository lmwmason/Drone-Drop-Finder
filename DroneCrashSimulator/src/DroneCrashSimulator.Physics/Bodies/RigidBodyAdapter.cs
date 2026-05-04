using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Trials;
using BepuSim = BepuPhysics.Simulation;

namespace DroneCrashSimulator.Physics.Bodies;

public sealed class RigidBodyAdapter
{
    private readonly BepuSim _simulation;
    private readonly BodyHandle _bodyHandle;

    public RigidBodyAdapter(BepuSim simulation, BodyHandle bodyHandle)
    {
        _simulation = simulation;
        _bodyHandle = bodyHandle;
    }

    public static RigidBodyAdapter CreateFor(
        BepuSim simulation,
        IDroneModel drone,
        FlightState initialState)
    {
        var shapeIndex = ShapeFactory.AddDroneShape(simulation, drone.Shape);
        var inertia = ShapeFactory.ComputeDroneInertia(drone);

        var pose = new RigidPose(
            ToVector3(initialState.Position),
            ToQuaternion(initialState.Attitude));

        var velocity = new BodyVelocity(
            ToVector3(initialState.Velocity),
            ToVector3(initialState.AngularVelocityRadiansPerSecond));

        var bodyDescription = BodyDescription.CreateDynamic(
            pose,
            velocity,
            inertia,
            shapeIndex,
            0.01f);

        var handle = simulation.Bodies.Add(bodyDescription);
        return new RigidBodyAdapter(simulation, handle);
    }

    public void ApplyForceImpulse(
        double forceXNewtons,
        double forceYNewtons,
        double forceZNewtons,
        double massKilograms,
        double dtSeconds)
    {
        var bodyRef = _simulation.Bodies.GetBodyReference(_bodyHandle);
        bodyRef.Velocity.Linear += new Vector3(
            (float)(forceXNewtons / massKilograms * dtSeconds),
            (float)(forceZNewtons / massKilograms * dtSeconds),
            (float)(forceYNewtons / massKilograms * dtSeconds));
    }

    public void ApplyTorqueImpulse(
        double torqueXNewtonMeters,
        double torqueYNewtonMeters,
        double torqueZNewtonMeters,
        InertiaTensor inertiaTensor,
        double dtSeconds)
    {
        var bodyRef = _simulation.Bodies.GetBodyReference(_bodyHandle);
        bodyRef.Velocity.Angular += new Vector3(
            (float)(torqueXNewtonMeters / inertiaTensor.IxxKilogramSquareMeters * dtSeconds),
            (float)(torqueZNewtonMeters / inertiaTensor.IzzKilogramSquareMeters * dtSeconds),
            (float)(torqueYNewtonMeters / inertiaTensor.IyyKilogramSquareMeters * dtSeconds));
    }

    public TrajectorySample ReadCurrentSample(double timeSeconds)
    {
        var bodyRef = _simulation.Bodies.GetBodyReference(_bodyHandle);
        return new TrajectorySample(
            Position: ToPosition(bodyRef.Pose.Position),
            Velocity: ToVelocity(bodyRef.Velocity.Linear),
            Attitude: ToAttitude(bodyRef.Pose.Orientation),
            TimeSeconds: timeSeconds);
    }

    public FlightState ReadCurrentFlightState(double timeSeconds)
    {
        var bodyRef = _simulation.Bodies.GetBodyReference(_bodyHandle);
        return new FlightState(
            Position: ToPosition(bodyRef.Pose.Position),
            Velocity: ToVelocity(bodyRef.Velocity.Linear),
            Attitude: ToAttitude(bodyRef.Pose.Orientation),
            AngularVelocityRadiansPerSecond: ToVelocity(bodyRef.Velocity.Angular),
            TimeSeconds: timeSeconds);
    }

    private static Vector3 ToVector3(Position p) =>
        new((float)p.XMeters, (float)p.ZMeters, (float)p.YMeters);

    private static Vector3 ToVector3(Velocity v) =>
        new((float)v.XMetersPerSecond, (float)v.ZMetersPerSecond, (float)v.YMetersPerSecond);

    private static Quaternion ToQuaternion(Attitude a) =>
        new((float)a.X, (float)a.Y, (float)a.Z, (float)a.W);

    private static Position ToPosition(Vector3 v) =>
        new(XMeters: v.X, YMeters: v.Z, ZMeters: v.Y);

    private static Velocity ToVelocity(Vector3 v) =>
        new(XMetersPerSecond: v.X, YMetersPerSecond: v.Z, ZMetersPerSecond: v.Y);

    private static Attitude ToAttitude(Quaternion q) =>
        new Attitude(W: q.W, X: q.X, Y: q.Y, Z: q.Z).Normalized();
}

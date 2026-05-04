namespace DroneCrashSimulator.Domain.Drones.Models;

public sealed class QuadcopterModel : IDroneModel
{
    public string Name => "Standard Quadcopter";
    public double MassKilograms => 1.5;
    public InertiaTensor InertiaTensor => new(
        IxxKilogramSquareMeters: 0.0213,
        IyyKilogramSquareMeters: 0.0213,
        IzzKilogramSquareMeters: 0.0400);
    public DroneShape Shape => new(
        LengthMeters: 0.35,
        WidthMeters: 0.35,
        HeightMeters: 0.10);
    public AerodynamicProperties AerodynamicProperties => new(
        DragCoefficientFlatFace: 1.2,
        DragCoefficientSideFace: 0.8,
        ReferenceAreaSquareMeters: 0.035,
        LiftCoefficient: 0.0);
}

namespace DroneCrashSimulator.Domain.Drones;

public sealed record AerodynamicProperties(
    double DragCoefficientFlatFace,
    double DragCoefficientSideFace,
    double ReferenceAreaSquareMeters,
    double LiftCoefficient = 0.0);

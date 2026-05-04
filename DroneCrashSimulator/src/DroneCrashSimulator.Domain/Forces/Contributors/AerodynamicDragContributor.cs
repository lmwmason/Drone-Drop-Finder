using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Forces.Contributors;

public sealed class AerodynamicDragContributor : IForceContributor
{
    public AppliedForce ComputeForce(FlightState state, IDroneModel drone, IEnvironment environment)
    {
        var windVelocity = environment.WindField.GetWindVelocityAt(state.Position);
        var relativeVelocity = state.Velocity - windVelocity;
        var relativeMagnitudeMetersPerSecond = relativeVelocity.MagnitudeMetersPerSecond;

        if (relativeMagnitudeMetersPerSecond < double.Epsilon)
            return AppliedForce.Zero;

        var airDensityKilogramsPerCubicMeter =
            environment.Atmosphere.GetAirDensityKilogramsPerCubicMeterAt(state.Position.ZMeters);

        var dragForceNewtons = ComputeDragMagnitudeNewtons(
            drone.AerodynamicProperties,
            airDensityKilogramsPerCubicMeter,
            relativeMagnitudeMetersPerSecond);

        return new AppliedForce(
            XNewtons: -dragForceNewtons * relativeVelocity.XMetersPerSecond / relativeMagnitudeMetersPerSecond,
            YNewtons: -dragForceNewtons * relativeVelocity.YMetersPerSecond / relativeMagnitudeMetersPerSecond,
            ZNewtons: -dragForceNewtons * relativeVelocity.ZMetersPerSecond / relativeMagnitudeMetersPerSecond,
            TorqueXNewtonMeters: 0.0,
            TorqueYNewtonMeters: 0.0,
            TorqueZNewtonMeters: 0.0);
    }

    private static double ComputeDragMagnitudeNewtons(
        AerodynamicProperties aerodynamics,
        double airDensityKilogramsPerCubicMeter,
        double relativeMagnitudeMetersPerSecond)
    {
        var dynamicPressurePascals = 0.5
            * airDensityKilogramsPerCubicMeter
            * relativeMagnitudeMetersPerSecond
            * relativeMagnitudeMetersPerSecond;

        return aerodynamics.DragCoefficientFlatFace
            * aerodynamics.ReferenceAreaSquareMeters
            * dynamicPressurePascals;
    }
}

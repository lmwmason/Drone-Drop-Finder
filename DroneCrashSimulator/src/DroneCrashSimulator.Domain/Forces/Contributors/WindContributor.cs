using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Flight;

namespace DroneCrashSimulator.Domain.Forces.Contributors;

public sealed class WindContributor : IForceContributor
{
    public AppliedForce ComputeForce(FlightState state, IDroneModel drone, IEnvironment environment)
    {
        var turbulenceVelocity = environment.TurbulenceModel.SampleTurbulenceVelocity(state);
        var turbulenceMagnitudeMetersPerSecond = turbulenceVelocity.MagnitudeMetersPerSecond;

        if (turbulenceMagnitudeMetersPerSecond < double.Epsilon)
            return AppliedForce.Zero;

        var airDensityKilogramsPerCubicMeter =
            environment.Atmosphere.GetAirDensityKilogramsPerCubicMeterAt(state.Position.ZMeters);

        var turbulenceDynamicPressurePascals = 0.5
            * airDensityKilogramsPerCubicMeter
            * turbulenceMagnitudeMetersPerSecond
            * turbulenceMagnitudeMetersPerSecond;

        var turbulenceForceNewtons = drone.AerodynamicProperties.DragCoefficientFlatFace
            * drone.AerodynamicProperties.ReferenceAreaSquareMeters
            * turbulenceDynamicPressurePascals;

        return new AppliedForce(
            XNewtons: turbulenceForceNewtons * turbulenceVelocity.XMetersPerSecond / turbulenceMagnitudeMetersPerSecond,
            YNewtons: turbulenceForceNewtons * turbulenceVelocity.YMetersPerSecond / turbulenceMagnitudeMetersPerSecond,
            ZNewtons: turbulenceForceNewtons * turbulenceVelocity.ZMetersPerSecond / turbulenceMagnitudeMetersPerSecond,
            TorqueXNewtonMeters: 0.0,
            TorqueYNewtonMeters: 0.0,
            TorqueZNewtonMeters: 0.0);
    }
}

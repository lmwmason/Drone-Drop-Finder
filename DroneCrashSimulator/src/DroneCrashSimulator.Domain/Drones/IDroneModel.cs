namespace DroneCrashSimulator.Domain.Drones;

public interface IDroneModel
{
    string Name { get; }
    double MassKilograms { get; }
    InertiaTensor InertiaTensor { get; }
    DroneShape Shape { get; }
    AerodynamicProperties AerodynamicProperties { get; }
}

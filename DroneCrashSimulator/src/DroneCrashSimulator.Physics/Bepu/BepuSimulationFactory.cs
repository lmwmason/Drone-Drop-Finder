using BepuPhysics;
using BepuUtilities.Memory;
using BepuSim = BepuPhysics.Simulation;

namespace DroneCrashSimulator.Physics.Bepu;

public static class BepuSimulationFactory
{
    public static BepuSim Create(BufferPool bufferPool)
    {
        return BepuSim.Create(
            bufferPool,
            new NoCollisionCallbacks(),
            new DroneForceCallbacks(),
            new SolveDescription(1, 1));
    }
}

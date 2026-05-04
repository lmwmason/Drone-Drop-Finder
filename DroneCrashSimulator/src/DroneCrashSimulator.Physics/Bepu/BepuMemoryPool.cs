using BepuUtilities.Memory;

namespace DroneCrashSimulator.Physics.Bepu;

public static class BepuMemoryPool
{
    public static BufferPool Create() => new BufferPool();
}

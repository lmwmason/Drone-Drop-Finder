namespace DroneCrashSimulator.Application.Trials;

public sealed class TrialSeedGenerator
{
    private readonly Random _masterRandom;

    public TrialSeedGenerator(int masterSeed)
    {
        _masterRandom = new Random(masterSeed);
    }

    public static TrialSeedGenerator FromSystemTime() =>
        new(Environment.TickCount);

    public int NextSeed() => _masterRandom.Next();
}

using System.Numerics;
using Raylib_cs;
using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Visualization.Rendering.Trajectories;

public sealed class TrajectoryRenderer
{
    private const float MaxVisibleTrajectories = 50;
    private const float BaseAlpha = 0.4f;

    public void RenderAll(IReadOnlyList<TrialResult> results)
    {
        var countToRender = Math.Min(results.Count, (int)MaxVisibleTrajectories);
        var step = results.Count <= countToRender ? 1 : results.Count / countToRender;

        for (var i = 0; i < results.Count; i += step)
            RenderSingleTrajectory(results[i], i, results.Count);
    }

    private static void RenderSingleTrajectory(TrialResult result, int index, int total)
    {
        var samples = result.Trajectory.Samples;
        if (samples.Count < 2) return;

        var color = TrajectoryColorMap.GetColorForTrajectoryIndex(index, total, BaseAlpha);

        for (var i = 0; i < samples.Count - 1; i++)
        {
            var from = ToWorld(samples[i]);
            var to = ToWorld(samples[i + 1]);
            Raylib_cs.Raylib.DrawLine3D(from, to, color);
        }
    }

    private static Vector3 ToWorld(TrajectorySample sample) =>
        new((float)sample.Position.XMeters,
            (float)sample.Position.ZMeters,
            (float)sample.Position.YMeters);
}

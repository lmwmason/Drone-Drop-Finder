using System.Numerics;
using Raylib_cs;
using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Visualization.Animation;

public sealed class AnimationRenderer
{
    private const float DronePointRadius = 0.4f;
    private const int MaxDronesWithTrail = 80;
    private static readonly Color DroneColor = new(220, 220, 60, 230);
    private static readonly Color TrailColor = new(100, 200, 255, 120);

    public void RenderAnimationFrame(TrajectoryAnimator animator, int totalTrialCount)
    {
        var showTrails = totalTrialCount <= MaxDronesWithTrail;

        if (showTrails)
            RenderTrails(animator.GetTrails());

        RenderDrones(animator.GetCurrentPositions());
        RenderProgressBar(animator.Progress);
    }

    private static void RenderDrones(IReadOnlyList<TrajectorySample> positions)
    {
        foreach (var sample in positions)
        {
            var worldPos = ToWorld(sample);
            Raylib_cs.Raylib.DrawSphere(worldPos, DronePointRadius, DroneColor);
        }
    }

    private static void RenderTrails(IReadOnlyList<IReadOnlyList<TrajectorySample>> trails)
    {
        foreach (var trail in trails)
        {
            for (var i = 0; i < trail.Count - 1; i++)
            {
                var alphaFraction = (float)(i + 1) / trail.Count;
                var trailColor = new Color(
                    TrailColor.R, TrailColor.G, TrailColor.B,
                    (byte)(TrailColor.A * alphaFraction));
                Raylib_cs.Raylib.DrawLine3D(ToWorld(trail[i]), ToWorld(trail[i + 1]), trailColor);
            }
        }
    }

    private static void RenderProgressBar(float progress)
    {
        const int barHeight = 6;
        const int barY = 30;
        const int barMargin = 150;
        var screenWidth = Raylib_cs.Raylib.GetScreenWidth();
        var barWidth = screenWidth - barMargin * 2;
        var filledWidth = (int)(barWidth * progress);

        Raylib_cs.Raylib.DrawRectangle(barMargin, barY, barWidth, barHeight,
            new Color((byte)50, (byte)50, (byte)50, (byte)180));
        Raylib_cs.Raylib.DrawRectangle(barMargin, barY, filledWidth, barHeight,
            new Color((byte)80, (byte)200, (byte)120, (byte)220));
        Raylib_cs.Raylib.DrawText(
            $"Simulating... {(int)(progress * 100)}%",
            barMargin, barY + 12, 14, Color.White);
    }

    private static Vector3 ToWorld(TrajectorySample sample) =>
        new((float)sample.Position.XMeters,
            (float)sample.Position.ZMeters,
            (float)sample.Position.YMeters);
}

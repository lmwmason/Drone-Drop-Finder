using System.Numerics;
using Raylib_cs;
using DroneCrashSimulator.Domain.Trials;

namespace DroneCrashSimulator.Visualization.Animation;

public sealed class SequentialAnimationRenderer
{
    private static readonly Color ActiveDroneColor = new((byte)255, (byte)220, (byte)50, (byte)255);
    private static readonly Color TrailColor = new((byte)120, (byte)220, (byte)255, (byte)200);
    private static readonly Color LandedMarkerColor = new((byte)255, (byte)100, (byte)60, (byte)200);
    private const float ActiveDroneRadius = 0.6f;
    private const float LandedMarkerRadius = 0.25f;
    private const float MarkerHeightMeters = 0.12f;

    public void Render(SequentialAnimator animator, int totalTrials)
    {
        RenderLandedMarkers(animator.LandedPoints);

        var position = animator.GetActiveDronePosition();
        if (position is not null)
        {
            RenderTrail(animator.GetActiveDroneTrail());
            RenderActiveDrone(position);
        }

        RenderHud(animator, totalTrials);
    }

    private static void RenderLandedMarkers(IReadOnlyList<CrashPoint> landed)
    {
        foreach (var point in landed)
        {
            var worldPos = new Vector3((float)point.XMeters, MarkerHeightMeters, (float)point.YMeters);
            Raylib_cs.Raylib.DrawSphere(worldPos, LandedMarkerRadius, LandedMarkerColor);
        }
    }

    private static void RenderTrail(IReadOnlyList<TrajectorySample> trail)
    {
        for (var i = 0; i < trail.Count - 1; i++)
        {
            var alpha = (float)(i + 1) / trail.Count;
            var color = new Color(
                TrailColor.R, TrailColor.G, TrailColor.B,
                (byte)(TrailColor.A * alpha));
            Raylib_cs.Raylib.DrawLine3D(ToWorld(trail[i]), ToWorld(trail[i + 1]), color);
        }
    }

    private static void RenderActiveDrone(TrajectorySample position)
    {
        Raylib_cs.Raylib.DrawSphere(ToWorld(position), ActiveDroneRadius, ActiveDroneColor);
    }

    private static void RenderHud(SequentialAnimator animator, int totalTrials)
    {
        const int barX = 140;
        const int barY = 32;
        const int barHeight = 8;
        var screenWidth = Raylib_cs.Raylib.GetScreenWidth();
        var barWidth = screenWidth - barX * 2;
        var filled = (int)(barWidth * animator.Progress);

        Raylib_cs.Raylib.DrawRectangle(barX, barY, barWidth, barHeight,
            new Color((byte)40, (byte)40, (byte)40, (byte)180));
        Raylib_cs.Raylib.DrawRectangle(barX, barY, filled, barHeight,
            new Color((byte)80, (byte)200, (byte)120, (byte)220));

        var label = $"Trial {Math.Min(animator.CurrentTrialIndex + 1, totalTrials)} / {totalTrials}";
        Raylib_cs.Raylib.DrawText(label, barX, barY + 14, 15, Color.White);
    }

    private static Vector3 ToWorld(TrajectorySample s) =>
        new((float)s.Position.XMeters, (float)s.Position.ZMeters, (float)s.Position.YMeters);
}

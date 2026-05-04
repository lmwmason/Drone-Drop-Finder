using System.Numerics;
using DroneCrashSimulator.Application.Batch;
using DroneCrashSimulator.Domain.Drones.Models;
using DroneCrashSimulator.Domain.Trials;
using DroneCrashSimulator.Io.Csv;
using DroneCrashSimulator.Visualization.Camera;
using DroneCrashSimulator.Visualization.Raylib;
using DroneCrashSimulator.Visualization.Rendering.Scene;
using DroneCrashSimulator.Visualization.UI;
using DroneCrashSimulator.Visualization.UI.Layout;
using Raylib_cs;

namespace DroneCrashSimulator.App.Modes;

public sealed class LibraryGeneratorMode
{
    private const int TrialsPerFrame = 12;

    public void Run()
    {
        using var window = new RaylibWindow("Drone Drop Finder — Library Generator");
        var input = new RaylibInputAdapter();
        var orbitCamera = new OrbitCamera();
        var cameraController = new CameraController(orbitCamera, input);
        var panel = new LibraryControlPanel();
        var groundGrid = new GroundGridRenderer();

        SweepExecutor? executor = null;
        SweepCsvWriter? csvWriter = null;
        var isRunning = false;
        var isComplete = false;
        var allCrashPoints = new List<CrashPoint>();
        var completedTrials = 0;
        var totalTrials = 0;
        string? outputMessage = null;

        while (!window.ShouldClose)
        {
            if (panel.RunRequested && !isRunning)
            {
                var config = BuildConfig(panel);
                var drone = BuildDrone(panel);
                executor = new SweepExecutor(config, drone);
                totalTrials = config.TotalTrials;
                allCrashPoints.Clear();
                completedTrials = 0;
                isRunning = true;
                isComplete = false;
                outputMessage = null;

                var csvPath = Path.Combine(
                    panel.SaveDirectoryPath,
                    $"sweep_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                Directory.CreateDirectory(panel.SaveDirectoryPath);
                csvWriter?.Dispose();
                csvWriter = new SweepCsvWriter(csvPath);
                outputMessage = $"Saving to: {csvPath}";
            }

            if (isRunning && executor is not null && csvWriter is not null)
            {
                for (var i = 0; i < TrialsPerFrame && !executor.IsComplete; i++)
                {
                    var output = executor.AdvanceOneTrial();
                    if (output is null) break;

                    csvWriter.WriteTrialResult(output);

                    allCrashPoints.Add(output.Result.CrashPoint);
                    completedTrials++;
                }

                if (executor.IsComplete)
                {
                    csvWriter.Flush();
                    csvWriter.Dispose();
                    csvWriter = null;
                    isRunning = false;
                    isComplete = true;
                }
            }

            window.BeginFrame();
            cameraController.ProcessInput();
            Raylib_cs.Raylib.ClearBackground(Color.Black);

            var camera = cameraController.GetCamera();
            Raylib_cs.Raylib.BeginMode3D(camera);
            groundGrid.Render();
            RenderCrashPoints(allCrashPoints);
            Raylib_cs.Raylib.EndMode3D();

            panel.Render(window.Width, window.Height, isRunning);
            RenderOverlay(
                isRunning, isComplete,
                executor, completedTrials, totalTrials,
                allCrashPoints.Count, outputMessage,
                window.Width, window.Height);

            Raylib_cs.Raylib.DrawFPS(8, 8);
            window.EndFrame();
        }

        csvWriter?.Dispose();
    }

    private static void RenderCrashPoints(List<CrashPoint> points)
    {
        if (points.Count == 0) return;

        var maxDist = points.Max(p => p.DistanceFromOriginMeters);
        if (maxDist < 0.1) maxDist = 1.0;

        foreach (var point in points)
        {
            var density = (float)(point.DistanceFromOriginMeters / maxDist);
            byte r = (byte)(50 + (int)(200 * density));
            byte g = (byte)(220 - (int)(170 * density));
            byte b = (byte)(255 - (int)(200 * density));
            var color = new Color(r, g, b, (byte)140);
            var worldPos = new Vector3((float)point.XMeters, 0.1f, (float)point.YMeters);
            Raylib_cs.Raylib.DrawSphere(worldPos, 0.25f, color);
        }
    }

    private static void RenderOverlay(
        bool isRunning,
        bool isComplete,
        SweepExecutor? executor,
        int completed,
        int total,
        int totalPoints,
        string? outputMessage,
        int screenWidth,
        int screenHeight)
    {
        var panelTop = screenHeight - PanelLayout.PanelHeight;
        var msgY = panelTop - 24;

        if (!isRunning && !isComplete)
        {
            Raylib_cs.Raylib.DrawText(
                "Set drone specs and press  Run Sweep",
                12, 32, 18, Color.Gray);
            return;
        }

        if (isRunning && executor is not null)
        {
            var combo    = executor.CurrentCombination;
            var progress = total > 0 ? (float)completed / total : 0f;
            var barWidth = Math.Min(screenWidth - 24, 600);

            Raylib_cs.Raylib.DrawText("SWEEP RUNNING", 12, 32, 16, Color.Green);
            if (combo is not null)
            {
                Raylib_cs.Raylib.DrawText(
                    $"Alt {combo.AltitudeMeters:F0} m  |  " +
                    $"H {combo.HorizontalSpeedMetersPerSecond:F1} m/s  |  " +
                    $"V {combo.VerticalSpeedMetersPerSecond:+0.#;-0.#;0} m/s",
                    12, 52, 14, Color.White);
            }
            Raylib_cs.Raylib.DrawText(
                $"{completed:N0} / {total:N0}  ({progress * 100:F1} %)",
                12, 70, 14, Color.Yellow);

            Raylib_cs.Raylib.DrawRectangle(12, 90, barWidth, 8,
                new Color((byte)40, (byte)40, (byte)40, (byte)200));
            Raylib_cs.Raylib.DrawRectangle(12, 90, (int)(barWidth * progress), 8,
                new Color((byte)60, (byte)200, (byte)100, (byte)230));
        }

        if (isComplete)
        {
            Raylib_cs.Raylib.DrawText("SWEEP COMPLETE", 12, 32, 18, Color.Green);
            Raylib_cs.Raylib.DrawText(
                $"Crash points recorded: {totalPoints:N0}",
                12, 56, 14, Color.White);
        }

        if (outputMessage is not null)
            Raylib_cs.Raylib.DrawText(
                outputMessage, 12, msgY, 13,
                isComplete ? Color.Green : Color.Yellow);
    }

    private static SweepConfiguration BuildConfig(LibraryControlPanel panel) =>
        new(
            AltitudeMinMeters: panel.AltitudeMinMeters,
            AltitudeMaxMeters: panel.AltitudeMaxMeters,
            AltitudeLevelCount: panel.AltitudeLevelCount,
            HorizontalSpeedMaxMetersPerSecond: panel.HorizontalSpeedMaxMetersPerSecond,
            HorizontalSpeedLevelCount: panel.HorizontalSpeedLevelCount,
            VerticalSpeedMinMetersPerSecond: panel.VerticalSpeedMinMetersPerSecond,
            VerticalSpeedMaxMetersPerSecond: panel.VerticalSpeedMaxMetersPerSecond,
            VerticalSpeedLevelCount: panel.VerticalSpeedLevelCount,
            TrialsPerCombination: panel.TrialsPerCombination,
            MaxWindSpeedMetersPerSecond: panel.MaxWindSpeedMetersPerSecond);

    private static SpecifiedDroneModel BuildDrone(LibraryControlPanel panel) =>
        new(
            massKilograms: panel.MassKilograms,
            dragCoefficient: panel.DragCoefficient,
            referenceAreaSquareMeters: panel.ReferenceAreaSquareMeters,
            cruiseSpeedMetersPerSecond: panel.HorizontalSpeedMaxMetersPerSecond);
}

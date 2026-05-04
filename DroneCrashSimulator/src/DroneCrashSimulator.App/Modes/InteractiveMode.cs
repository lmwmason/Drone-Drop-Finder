using DroneCrashSimulator.Application.Aggregation;
using DroneCrashSimulator.Application.Trials;
using DroneCrashSimulator.Domain.Drones.Models;
using DroneCrashSimulator.Domain.Environment.Atmosphere;
using DroneCrashSimulator.Domain.Environment.Terrains;
using DroneCrashSimulator.Domain.Environment.Turbulence;
using DroneCrashSimulator.Domain.Environment.WindFields;
using DroneCrashSimulator.Domain.Environment;
using DroneCrashSimulator.Domain.Failure.Modes;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Statistics;
using DroneCrashSimulator.Domain.Trials;
using DroneCrashSimulator.Io.Csv;
using DroneCrashSimulator.Io.Json;
using DroneCrashSimulator.Physics.Simulation;
using DroneCrashSimulator.Visualization.Animation;
using DroneCrashSimulator.Visualization.Camera;
using DroneCrashSimulator.Visualization.Raylib;
using DroneCrashSimulator.Visualization.Rendering.Distribution;
using DroneCrashSimulator.Visualization.Rendering.Scene;
using DroneCrashSimulator.Visualization.UI;
using DroneCrashSimulator.Visualization.UI.Layout;
using Raylib_cs;

namespace DroneCrashSimulator.App.Modes;

public sealed class InteractiveMode
{
    public void Run()
    {
        using var window = new RaylibWindow("Drone Crash Simulator");
        var input = new RaylibInputAdapter();
        var orbitCamera = new OrbitCamera();
        var cameraController = new CameraController(orbitCamera, input);
        var controlPanel = new ControlPanel();
        var aggregator = new StatisticsAggregator();
        var trialRunner = new TrialRunner(TimestepConfiguration.Default);

        var groundGrid = new GroundGridRenderer();
        var histogramRenderer = new LandingHistogram3DRenderer();
        var seqAnimRenderer = new SequentialAnimationRenderer();

        var phase = SimulationPhase.Idle;
        List<TrialResult> results = new();
        SequentialAnimator? animator = null;
        ConfidenceRegion? confidenceRegion = null;
        GpsPosition? originGps = null;
        string? saveNotification = null;
        int notificationFrames = 0;

        while (!window.ShouldClose)
        {
            if (controlPanel.SimulateRequested)
            {
                originGps = new GpsPosition(
                    controlPanel.LatitudeDegrees,
                    controlPanel.LongitudeDegrees,
                    controlPanel.AltitudeMeters);

                results = ComputeAllTrials(trialRunner, controlPanel);
                animator = new SequentialAnimator(results);
                confidenceRegion = null;
                phase = SimulationPhase.Animating;
            }

            if (phase == SimulationPhase.Animating && animator is not null)
            {
                animator.Advance();
                if (animator.IsComplete)
                {
                    confidenceRegion = aggregator.ComputeConfidenceRegion95(results);
                    phase = SimulationPhase.ShowingDistribution;
                }
            }

            window.BeginFrame();
            cameraController.ProcessInput();
            Raylib_cs.Raylib.ClearBackground(Color.Black);

            var camera = cameraController.GetCamera();
            Raylib_cs.Raylib.BeginMode3D(camera);
            groundGrid.Render();

            switch (phase)
            {
                case SimulationPhase.Animating when animator is not null:
                    seqAnimRenderer.Render(animator, results.Count);
                    break;

                case SimulationPhase.ShowingDistribution when confidenceRegion is not null:
                    histogramRenderer.Render(
                        results.Select(r => r.CrashPoint).ToList(),
                        confidenceRegion);
                    break;
            }

            Raylib_cs.Raylib.EndMode3D();
            controlPanel.Render(
                window.Width, window.Height,
                saveEnabled: phase == SimulationPhase.ShowingDistribution);
            RenderOverlay(phase, results, confidenceRegion, controlPanel, originGps);

            if (notificationFrames > 0)
            {
                Raylib_cs.Raylib.DrawText(
                    saveNotification,
                    12, window.Height - PanelLayout.PanelHeight - 26,
                    15, Color.Green);
                notificationFrames--;
            }

            Raylib_cs.Raylib.DrawFPS(8, 8);
            window.EndFrame();

            if (phase == SimulationPhase.ShowingDistribution
                && confidenceRegion is not null
                && controlPanel.SaveRequested)
            {
                SaveResults(results, confidenceRegion, controlPanel.SaveDirectoryPath, originGps);
                saveNotification = $"Saved to: {controlPanel.SaveDirectoryPath}";
                notificationFrames = 180;
            }
        }
    }

    private static List<TrialResult> ComputeAllTrials(
        TrialRunner trialRunner,
        ControlPanel panel)
    {
        var drone = new SpecifiedDroneModel(
            massKilograms: panel.MassKilograms,
            dragCoefficient: panel.DragCoefficient,
            referenceAreaSquareMeters: panel.ReferenceAreaSquareMeters,
            cruiseSpeedMetersPerSecond: panel.CruiseSpeedMetersPerSecond);

        var failureMode = new TotalThrustLossMode();
        var seedGenerator = TrialSeedGenerator.FromSystemTime();
        var computed = new List<TrialResult>(panel.TrialCount);

        for (var i = 0; i < panel.TrialCount; i++)
        {
            var seed = seedGenerator.NextSeed();
            var random = new Random(seed);
            var environment = SampleEnvironment(panel.MaxWindSpeedMetersPerSecond, random);
            var configuration = new TrialConfiguration(
                FailurePosition: new Position(0.0, 0.0, panel.AltitudeMeters),
                FailureVelocity: Velocity.Zero,
                TrialCount: panel.TrialCount,
                CruiseSpeedMetersPerSecond: panel.CruiseSpeedMetersPerSecond);

            computed.Add(trialRunner.RunTrial(
                i, seed, drone, environment, failureMode, configuration));
        }

        return computed;
    }

    private static IEnvironment SampleEnvironment(
        float maxWindSpeedMetersPerSecond,
        Random random)
    {
        var windSpeed = random.NextDouble() * maxWindSpeedMetersPerSecond;
        var windAngle = random.NextDouble() * 2.0 * Math.PI;
        var windVelocity = new Velocity(
            XMetersPerSecond: windSpeed * Math.Cos(windAngle),
            YMetersPerSecond: windSpeed * Math.Sin(windAngle),
            ZMetersPerSecond: 0.0);

        return new StandardEnvironment(
            new StandardAtmosphere(),
            new FlatTerrain(),
            new UniformWindField(windVelocity),
            new DrydenTurbulenceModel());
    }

    private static void SaveResults(
        IReadOnlyList<TrialResult> results,
        ConfidenceRegion confidenceRegion,
        string saveDirectory,
        GpsPosition? originGps)
    {
        Directory.CreateDirectory(saveDirectory);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var screenshotPath = Path.Combine(saveDirectory, $"distribution_{timestamp}.png");
        var tempPath = Path.Combine(
            Path.GetTempPath(), $"drone_dist_{Environment.TickCount}.png");
        var screenImage = Raylib_cs.Raylib.LoadImageFromScreen();
        Raylib_cs.Raylib.ExportImage(screenImage, tempPath);
        Raylib_cs.Raylib.UnloadImage(screenImage);
        File.Copy(tempPath, screenshotPath, overwrite: true);
        File.Delete(tempPath);

        var csvPath = Path.Combine(saveDirectory, $"crash_points_{timestamp}.csv");
        using var csvWriter = new CrashPointCsvWriter(csvPath, originGps);
        foreach (var result in results)
            csvWriter.WriteResult(result);
        csvWriter.Flush();

        var aggregator = new StatisticsAggregator();
        var summary = aggregator.ComputeSummary(results);
        var jsonPath = Path.Combine(saveDirectory, $"summary_{timestamp}.json");
        new SummaryJsonWriter().Write(jsonPath, summary, confidenceRegion);
    }

    private static void RenderOverlay(
        SimulationPhase phase,
        IReadOnlyList<TrialResult> results,
        ConfidenceRegion? region,
        ControlPanel panel,
        GpsPosition? gps)
    {
        switch (phase)
        {
            case SimulationPhase.Idle:
                Raylib_cs.Raylib.DrawText(
                    "Set GPS + Drone specs, then press Simulate",
                    12, 32, 17, Color.Gray);
                break;

            case SimulationPhase.ShowingDistribution when region is not null && gps is not null:
                Raylib_cs.Raylib.DrawText(
                    $"GPS: {gps.LatitudeDegrees:F5}, {gps.LongitudeDegrees:F5}  " +
                    $"Alt: {gps.AltitudeMeters:F0} m",
                    12, 32, 15, Color.White);
                Raylib_cs.Raylib.DrawText(
                    $"Trials: {results.Count}  |  " +
                    $"95% radius: {region.RadiusMeters:F1} m  |  " +
                    $"Mean offset: ({region.CenterXMeters:F1}, {region.CenterYMeters:F1}) m",
                    12, 54, 15, Color.Yellow);
                Raylib_cs.Raylib.DrawText(
                    "Blue = rare  |  Red = frequent", 12, 76, 14, Color.LightGray);
                break;
        }
    }
}

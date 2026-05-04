using Raylib_cs;
using DroneCrashSimulator.Application.Aggregation;
using DroneCrashSimulator.Domain.Drones;
using DroneCrashSimulator.Domain.Flight;
using DroneCrashSimulator.Domain.Statistics;
using DroneCrashSimulator.Domain.Trials;
using DroneCrashSimulator.Visualization.Camera;
using DroneCrashSimulator.Visualization.Rendering.Distribution;
using DroneCrashSimulator.Visualization.Rendering.Scene;
using DroneCrashSimulator.Visualization.Rendering.Trajectories;
using DroneCrashSimulator.Visualization.UI;

namespace DroneCrashSimulator.Visualization.Rendering;

public sealed class Renderer
{
    private readonly CameraController _cameraController;
    private readonly GroundGridRenderer _groundGrid;
    private readonly WindArrowRenderer _windArrow;
    private readonly TrajectoryRenderer _trajectoryRenderer;
    private readonly ScatterRenderer _scatterRenderer;
    private readonly ConfidenceCircleRenderer _confidenceCircleRenderer;
    private readonly ControlPanel _controlPanel;

    public Renderer(
        CameraController cameraController,
        GroundGridRenderer groundGrid,
        WindArrowRenderer windArrow,
        TrajectoryRenderer trajectoryRenderer,
        ScatterRenderer scatterRenderer,
        ConfidenceCircleRenderer confidenceCircleRenderer,
        ControlPanel controlPanel)
    {
        _cameraController = cameraController;
        _groundGrid = groundGrid;
        _windArrow = windArrow;
        _trajectoryRenderer = trajectoryRenderer;
        _scatterRenderer = scatterRenderer;
        _confidenceCircleRenderer = confidenceCircleRenderer;
        _controlPanel = controlPanel;
    }

    public void RenderFrame(
        int screenWidth,
        int screenHeight,
        IReadOnlyList<TrialResult> results,
        ConfidenceRegion? confidenceRegion,
        Velocity windVelocity)
    {
        _cameraController.ProcessInput();

        Raylib_cs.Raylib.ClearBackground(Color.Black);

        var camera = _cameraController.GetCamera();
        Raylib_cs.Raylib.BeginMode3D(camera);

        _groundGrid.Render();
        _windArrow.Render(windVelocity);

        if (results.Count > 0)
        {
            _trajectoryRenderer.RenderAll(results);
            _scatterRenderer.Render(results.Select(r => r.CrashPoint).ToList());
        }

        if (confidenceRegion is not null)
            _confidenceCircleRenderer.Render(confidenceRegion);

        Raylib_cs.Raylib.EndMode3D();

        _controlPanel.Render(screenWidth, screenHeight, saveEnabled: false);

        RenderResultStats(results, confidenceRegion);
        Raylib_cs.Raylib.DrawFPS(8, 8);
    }

    private static void RenderResultStats(
        IReadOnlyList<TrialResult> results,
        ConfidenceRegion? region)
    {
        if (results.Count == 0) return;

        Raylib_cs.Raylib.DrawText($"Trials: {results.Count}", 8, 30, 16, Color.White);

        if (region is null) return;
        Raylib_cs.Raylib.DrawText(
            $"95% radius: {region.RadiusMeters:F1} m", 8, 52, 16, Color.Yellow);
        Raylib_cs.Raylib.DrawText(
            $"Center: ({region.CenterXMeters:F1}, {region.CenterYMeters:F1}) m", 8, 74, 16, Color.Yellow);
    }
}

using Raylib_cs;
using DroneCrashSimulator.Visualization.Raylib;

namespace DroneCrashSimulator.Visualization.Camera;

public sealed class CameraController
{
    private readonly OrbitCamera _camera;
    private readonly RaylibInputAdapter _input;

    public CameraController(OrbitCamera camera, RaylibInputAdapter input)
    {
        _camera = camera;
        _input = input;
    }

    public Camera3D GetCamera() => _camera.BuildRaylibCamera();

    public void ProcessInput()
    {
        HandleMouseOrbit();
        HandleScrollZoom();
    }

    private void HandleMouseOrbit()
    {
        if (!_input.IsMouseButtonDown(MouseButton.Right))
            return;

        var delta = _input.GetMouseDelta();
        _camera.Orbit(delta.X, -delta.Y);
    }

    private void HandleScrollZoom()
    {
        var wheelMove = _input.GetMouseWheelMove();
        if (Math.Abs(wheelMove) > float.Epsilon)
            _camera.Zoom(wheelMove);
    }
}

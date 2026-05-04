using System.Numerics;
using Raylib_cs;

namespace DroneCrashSimulator.Visualization.Camera;

public sealed class OrbitCamera
{
    private const float DefaultRadiusMeters = 80.0f;
    private const float DefaultAzimuthDegrees = 45.0f;
    private const float DefaultElevationDegrees = 30.0f;
    private const float MinElevationDegrees = 5.0f;
    private const float MaxElevationDegrees = 89.0f;
    private const float ZoomSensitivity = 3.0f;
    private const float OrbitSensitivity = 0.3f;

    private float _radiusMeters = DefaultRadiusMeters;
    private float _azimuthDegrees = DefaultAzimuthDegrees;
    private float _elevationDegrees = DefaultElevationDegrees;
    private Vector3 _target = Vector3.Zero;

    public Camera3D BuildRaylibCamera()
    {
        var azimuthRadians = _azimuthDegrees * MathF.PI / 180.0f;
        var elevationRadians = _elevationDegrees * MathF.PI / 180.0f;

        var cameraPosition = new Vector3(
            _target.X + _radiusMeters * MathF.Cos(elevationRadians) * MathF.Cos(azimuthRadians),
            _target.Y + _radiusMeters * MathF.Sin(elevationRadians),
            _target.Z + _radiusMeters * MathF.Cos(elevationRadians) * MathF.Sin(azimuthRadians));

        return new Camera3D(cameraPosition, _target, Vector3.UnitY, 45.0f, CameraProjection.Perspective);
    }

    public void Orbit(float deltaAzimuthDegrees, float deltaElevationDegrees)
    {
        _azimuthDegrees += deltaAzimuthDegrees * OrbitSensitivity;
        _elevationDegrees = Math.Clamp(
            _elevationDegrees + deltaElevationDegrees * OrbitSensitivity,
            MinElevationDegrees,
            MaxElevationDegrees);
    }

    public void Zoom(float deltaRadius)
    {
        _radiusMeters = Math.Max(5.0f, _radiusMeters - deltaRadius * ZoomSensitivity);
    }

    public void SetTarget(Vector3 target)
    {
        _target = target;
    }
}

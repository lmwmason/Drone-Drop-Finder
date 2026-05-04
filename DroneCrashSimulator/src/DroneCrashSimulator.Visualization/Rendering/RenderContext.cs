using Raylib_cs;

namespace DroneCrashSimulator.Visualization.Rendering;

public sealed record RenderContext(
    Camera3D Camera,
    int ScreenWidth,
    int ScreenHeight);

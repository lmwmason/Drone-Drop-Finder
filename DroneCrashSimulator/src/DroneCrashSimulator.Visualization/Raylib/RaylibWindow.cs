using Raylib_cs;

namespace DroneCrashSimulator.Visualization.Raylib;

public sealed class RaylibWindow : IDisposable
{
    private const int DefaultWidth = 1280;
    private const int DefaultHeight = 720;
    private const int MinWidth = 960;
    private const int MinHeight = 540;
    private const int TargetFps = 60;

    public RaylibWindow(string title)
    {
        Raylib_cs.Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib_cs.Raylib.InitWindow(DefaultWidth, DefaultHeight, title);
        Raylib_cs.Raylib.SetWindowMinSize(MinWidth, MinHeight);
        Raylib_cs.Raylib.SetTargetFPS(TargetFps);
    }

    public bool ShouldClose => Raylib_cs.Raylib.WindowShouldClose();

    public int Width => Raylib_cs.Raylib.GetScreenWidth();
    public int Height => Raylib_cs.Raylib.GetScreenHeight();

    public float DeltaTimeSeconds => Raylib_cs.Raylib.GetFrameTime();

    public void BeginFrame() => Raylib_cs.Raylib.BeginDrawing();
    public void EndFrame() => Raylib_cs.Raylib.EndDrawing();

    public void ClearBackground(Color color) => Raylib_cs.Raylib.ClearBackground(color);

    public void Dispose() => Raylib_cs.Raylib.CloseWindow();
}

using System.Numerics;
using Raylib_cs;

namespace DroneCrashSimulator.Visualization.Raylib;

public sealed class RaylibInputAdapter
{
    public bool IsKeyPressed(KeyboardKey key) =>
        Raylib_cs.Raylib.IsKeyPressed(key);

    public bool IsKeyDown(KeyboardKey key) =>
        Raylib_cs.Raylib.IsKeyDown(key);

    public bool IsMouseButtonDown(MouseButton button) =>
        Raylib_cs.Raylib.IsMouseButtonDown(button);

    public Vector2 GetMouseDelta() =>
        Raylib_cs.Raylib.GetMouseDelta();

    public float GetMouseWheelMove() =>
        Raylib_cs.Raylib.GetMouseWheelMove();

    public Vector2 GetMousePosition() =>
        Raylib_cs.Raylib.GetMousePosition();
}

using Raylib_cs;

namespace DroneCrashSimulator.Visualization.UI.Controls;

public sealed class ButtonControl
{
    private readonly string _label;
    private static readonly Color NormalColor = new(60, 120, 200, 255);
    private static readonly Color HoverColor = new(80, 150, 230, 255);
    private static readonly Color PressedColor = new(40, 90, 160, 255);

    public ButtonControl(string label)
    {
        _label = label;
    }

    public bool WasClicked { get; private set; }

    public void Render(int x, int y, int width, int height)
    {
        var mouse = Raylib_cs.Raylib.GetMousePosition();
        var isHovered = mouse.X >= x && mouse.X <= x + width
            && mouse.Y >= y && mouse.Y <= y + height;

        var isPressed = isHovered && Raylib_cs.Raylib.IsMouseButtonDown(MouseButton.Left);
        WasClicked = isHovered && Raylib_cs.Raylib.IsMouseButtonPressed(MouseButton.Left);

        var buttonColor = isPressed ? PressedColor : isHovered ? HoverColor : NormalColor;
        Raylib_cs.Raylib.DrawRectangle(x, y, width, height, buttonColor);
        Raylib_cs.Raylib.DrawRectangleLines(x, y, width, height, Color.White);

        var textWidth = Raylib_cs.Raylib.MeasureText(_label, 16);
        Raylib_cs.Raylib.DrawText(_label, x + (width - textWidth) / 2, y + (height - 16) / 2, 16, Color.White);
    }
}

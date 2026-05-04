using Raylib_cs;

namespace DroneCrashSimulator.Visualization.UI.Controls;

public sealed class TextInputControl
{
    private const int MaxLength = 120;
    private static readonly Color ActiveBorder = new((byte)100, (byte)160, (byte)255, (byte)255);
    private static readonly Color InactiveBorder = new((byte)80, (byte)80, (byte)80, (byte)255);
    private static readonly Color Background = new((byte)25, (byte)25, (byte)35, (byte)255);

    private readonly string _label;
    private bool _focused;
    private int _cursorBlinkCounter;

    public TextInputControl(string label, string initialValue)
    {
        _label = label;
        Text = initialValue;
    }

    public string Text { get; private set; }

    public void Update(int x, int y, int width)
    {
        const int height = 24;
        var mouse = Raylib_cs.Raylib.GetMousePosition();
        var isHovered = mouse.X >= x && mouse.X <= x + width
            && mouse.Y >= y && mouse.Y <= y + height;

        if (Raylib_cs.Raylib.IsMouseButtonPressed(MouseButton.Left))
            _focused = isHovered;

        if (_focused)
            ProcessKeyboard();

        _cursorBlinkCounter = (_cursorBlinkCounter + 1) % 60;

        Raylib_cs.Raylib.DrawText(_label, x, y - 16, 13, Color.White);
        var border = _focused ? ActiveBorder : InactiveBorder;
        Raylib_cs.Raylib.DrawRectangle(x, y, width, height, Background);
        Raylib_cs.Raylib.DrawRectangleLines(x, y, width, height, border);

        var displayText = Text.Length > 36 ? "..." + Text[^36..] : Text;
        Raylib_cs.Raylib.DrawText(displayText, x + 5, y + 5, 13, Color.White);

        if (_focused && _cursorBlinkCounter < 30)
        {
            var cursorX = x + 5 + Raylib_cs.Raylib.MeasureText(displayText, 13);
            Raylib_cs.Raylib.DrawRectangle(cursorX, y + 4, 2, 16, Color.White);
        }
    }

    private void ProcessKeyboard()
    {
        if (Raylib_cs.Raylib.IsKeyPressed(KeyboardKey.Backspace) && Text.Length > 0)
            Text = Text[..^1];

        if (Raylib_cs.Raylib.IsKeyPressed(KeyboardKey.Escape))
            _focused = false;

        int key;
        while ((key = Raylib_cs.Raylib.GetCharPressed()) > 0)
        {
            if (Text.Length < MaxLength)
                Text += (char)key;
        }
    }
}

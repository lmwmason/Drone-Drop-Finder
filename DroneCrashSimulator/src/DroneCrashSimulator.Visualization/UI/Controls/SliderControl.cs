using Raylib_cs;

namespace DroneCrashSimulator.Visualization.UI.Controls;

public sealed class SliderControl
{
    private const int FontSize = 13;
    private const int TrackOffsetY = 18;
    private const int TrackHeight = 4;
    private const int HandleHalfWidth = 5;
    private const int HandleHeight = 12;

    public const int TotalHeight = TrackOffsetY + TrackHeight + 6;

    private readonly string _label;
    private readonly float _minValue;
    private readonly float _maxValue;
    private float _currentValue;

    public SliderControl(string label, float minValue, float maxValue, float initialValue)
    {
        _label = label;
        _minValue = minValue;
        _maxValue = maxValue;
        _currentValue = Math.Clamp(initialValue, minValue, maxValue);
    }

    public float Value => _currentValue;

    public void Render(int x, int y, int width)
    {
        var valueText = FormatValue();
        var valueWidth = Raylib_cs.Raylib.MeasureText(valueText, FontSize);

        Raylib_cs.Raylib.DrawText(_label, x, y, FontSize, Color.LightGray);
        Raylib_cs.Raylib.DrawText(valueText,
            x + width - valueWidth, y, FontSize, Color.Yellow);

        var trackY = y + TrackOffsetY;
        Raylib_cs.Raylib.DrawRectangle(x, trackY, width, TrackHeight,
            new Color((byte)70, (byte)70, (byte)70, (byte)255));

        var fraction = (_currentValue - _minValue) / (_maxValue - _minValue);
        var filledWidth = (int)(fraction * width);
        Raylib_cs.Raylib.DrawRectangle(x, trackY, filledWidth, TrackHeight,
            new Color((byte)80, (byte)150, (byte)255, (byte)255));

        var handleX = x + filledWidth;
        Raylib_cs.Raylib.DrawRectangle(
            handleX - HandleHalfWidth, trackY - 4,
            HandleHalfWidth * 2, HandleHeight, Color.White);

        HandleDrag(x, trackY, width);
    }

    private string FormatValue()
    {
        var range = _maxValue - _minValue;
        if (range >= 10.0f) return _currentValue.ToString("F0");
        if (range >= 1.0f) return _currentValue.ToString("F1");
        return _currentValue.ToString("F3");
    }

    private void HandleDrag(int trackX, int trackY, int width)
    {
        if (!Raylib_cs.Raylib.IsMouseButtonDown(MouseButton.Left))
            return;

        var mouse = Raylib_cs.Raylib.GetMousePosition();
        if (mouse.Y < trackY - 6 || mouse.Y > trackY + HandleHeight) return;
        if (mouse.X < trackX - HandleHalfWidth || mouse.X > trackX + width + HandleHalfWidth)
            return;

        var fraction = (mouse.X - trackX) / width;
        _currentValue = _minValue + fraction * (_maxValue - _minValue);
        _currentValue = Math.Clamp(_currentValue, _minValue, _maxValue);
    }
}

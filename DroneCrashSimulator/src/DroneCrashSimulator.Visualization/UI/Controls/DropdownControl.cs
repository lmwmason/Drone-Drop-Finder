using Raylib_cs;

namespace DroneCrashSimulator.Visualization.UI.Controls;

public sealed class DropdownControl
{
    private readonly string _label;
    private readonly IReadOnlyList<string> _options;
    private int _selectedIndex;
    private bool _isOpen;

    public DropdownControl(string label, IReadOnlyList<string> options, int initialIndex = 0)
    {
        _label = label;
        _options = options;
        _selectedIndex = Math.Clamp(initialIndex, 0, Math.Max(0, options.Count - 1));
    }

    public string SelectedOption => _options.Count > 0 ? _options[_selectedIndex] : string.Empty;
    public int SelectedIndex => _selectedIndex;

    public void Render(int x, int y, int width)
    {
        const int ItemHeight = 24;

        Raylib_cs.Raylib.DrawText(_label, x, y, 14, Color.White);

        var buttonY = y + 18;
        var buttonColor = _isOpen ? Color.DarkGray : new Color(60, 60, 60, 255);
        Raylib_cs.Raylib.DrawRectangle(x, buttonY, width, ItemHeight, buttonColor);
        Raylib_cs.Raylib.DrawRectangleLines(x, buttonY, width, ItemHeight, Color.Gray);
        Raylib_cs.Raylib.DrawText(SelectedOption, x + 6, buttonY + 5, 13, Color.White);

        if (CheckButtonClick(x, buttonY, width, ItemHeight))
            _isOpen = !_isOpen;

        if (_isOpen)
            RenderDropdownItems(x, buttonY + ItemHeight, width, ItemHeight);
    }

    private void RenderDropdownItems(int x, int startY, int width, int itemHeight)
    {
        for (var i = 0; i < _options.Count; i++)
        {
            var itemY = startY + i * itemHeight;
            var itemColor = i == _selectedIndex
                ? new Color(80, 80, 120, 255)
                : new Color(45, 45, 45, 255);

            Raylib_cs.Raylib.DrawRectangle(x, itemY, width, itemHeight, itemColor);
            Raylib_cs.Raylib.DrawRectangleLines(x, itemY, width, itemHeight, Color.DarkGray);
            Raylib_cs.Raylib.DrawText(_options[i], x + 6, itemY + 5, 13, Color.White);

            if (!CheckButtonClick(x, itemY, width, itemHeight)) continue;
            _selectedIndex = i;
            _isOpen = false;
        }
    }

    private static bool CheckButtonClick(int x, int y, int width, int height)
    {
        if (!Raylib_cs.Raylib.IsMouseButtonPressed(MouseButton.Left))
            return false;

        var mouse = Raylib_cs.Raylib.GetMousePosition();
        return mouse.X >= x && mouse.X <= x + width
            && mouse.Y >= y && mouse.Y <= y + height;
    }
}

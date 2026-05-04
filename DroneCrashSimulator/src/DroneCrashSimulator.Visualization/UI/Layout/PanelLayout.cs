using Raylib_cs;

namespace DroneCrashSimulator.Visualization.UI.Layout;

public sealed class PanelLayout
{
    public const int PanelHeight = 250;
    private const int TopPadding = 16;
    private const int ColumnGap = 12;
    private static readonly Color PanelBackground =
        new((byte)12, (byte)12, (byte)22, (byte)240);

    public int PanelY(int screenHeight) => screenHeight - PanelHeight;
    public int ContentStartY(int screenHeight) => PanelY(screenHeight) + TopPadding;

    public int ColumnCount => 6;
    public int ColumnWidth(int screenWidth) =>
        (screenWidth - ColumnGap * (ColumnCount + 1)) / ColumnCount;

    public int ColumnX(int screenWidth, int colIndex) =>
        ColumnGap + colIndex * (ColumnWidth(screenWidth) + ColumnGap);

    public void RenderBackground(int screenWidth, int screenHeight)
    {
        Raylib_cs.Raylib.DrawRectangle(
            0, PanelY(screenHeight), screenWidth, PanelHeight, PanelBackground);
        Raylib_cs.Raylib.DrawLine(
            0, PanelY(screenHeight),
            screenWidth, PanelY(screenHeight),
            new Color((byte)60, (byte)60, (byte)80, (byte)255));
    }
}

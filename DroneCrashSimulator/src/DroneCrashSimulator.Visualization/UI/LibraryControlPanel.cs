using Raylib_cs;
using DroneCrashSimulator.Visualization.UI.Controls;
using DroneCrashSimulator.Visualization.UI.Layout;

namespace DroneCrashSimulator.Visualization.UI;

public sealed class LibraryControlPanel
{
    private const int HeaderFontSize = 12;
    private const int SliderRowGap = SliderControl.TotalHeight + 10;

    private readonly PanelLayout _layout;

    private readonly SliderControl _massSlider;
    private readonly SliderControl _dragCoefficientSlider;
    private readonly SliderControl _referenceAreaSlider;

    private readonly SliderControl _altitudeMinSlider;
    private readonly SliderControl _altitudeMaxSlider;
    private readonly SliderControl _altitudeLevelsSlider;

    private readonly SliderControl _hSpeedMaxSlider;
    private readonly SliderControl _hSpeedLevelsSlider;

    private readonly SliderControl _vSpeedMinSlider;
    private readonly SliderControl _vSpeedMaxSlider;
    private readonly SliderControl _vSpeedLevelsSlider;

    private readonly SliderControl _maxWindSpeedSlider;
    private readonly SliderControl _trialsPerComboSlider;

    private readonly ButtonControl _runButton;
    private readonly TextInputControl _savePathInput;

    public LibraryControlPanel()
    {
        _layout = new PanelLayout();

        _massSlider            = new SliderControl("Mass (kg)",      0.1f,   50.0f,  1.5f);
        _dragCoefficientSlider = new SliderControl("Drag Coeff Cd",  0.1f,    2.5f,  1.2f);
        _referenceAreaSlider   = new SliderControl("Ref Area (m²)",  0.005f,  1.0f,  0.035f);

        _altitudeMinSlider     = new SliderControl("Min (m)",        10.0f,  200.0f, 20.0f);
        _altitudeMaxSlider     = new SliderControl("Max (m)",        50.0f,  500.0f, 300.0f);
        _altitudeLevelsSlider  = new SliderControl("Levels",          2.0f,   20.0f, 10.0f);

        _hSpeedMaxSlider       = new SliderControl("Max (m/s)",       0.0f,   40.0f, 20.0f);
        _hSpeedLevelsSlider    = new SliderControl("Levels",          2.0f,   15.0f,  8.0f);

        _vSpeedMinSlider       = new SliderControl("Min (m/s)",     -25.0f,    0.0f, -10.0f);
        _vSpeedMaxSlider       = new SliderControl("Max (m/s)",       0.0f,   25.0f, 10.0f);
        _vSpeedLevelsSlider    = new SliderControl("Levels",          2.0f,   10.0f,  5.0f);

        _maxWindSpeedSlider    = new SliderControl("Max Wind (m/s)",  0.0f,   25.0f, 12.0f);
        _trialsPerComboSlider  = new SliderControl("Trials / combo", 50.0f, 1000.0f, 400.0f);

        _runButton    = new ButtonControl("Run Sweep");
        _savePathInput = new TextInputControl(
            "Save Directory",
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    }

    public float MassKilograms             => _massSlider.Value;
    public float DragCoefficient           => _dragCoefficientSlider.Value;
    public float ReferenceAreaSquareMeters => _referenceAreaSlider.Value;

    public float AltitudeMinMeters =>
        Math.Min(_altitudeMinSlider.Value, _altitudeMaxSlider.Value - 10.0f);
    public float AltitudeMaxMeters  => _altitudeMaxSlider.Value;
    public int   AltitudeLevelCount => (int)_altitudeLevelsSlider.Value;

    public float HorizontalSpeedMaxMetersPerSecond => _hSpeedMaxSlider.Value;
    public int   HorizontalSpeedLevelCount         => (int)_hSpeedLevelsSlider.Value;

    public float VerticalSpeedMinMetersPerSecond => _vSpeedMinSlider.Value;
    public float VerticalSpeedMaxMetersPerSecond => _vSpeedMaxSlider.Value;
    public int   VerticalSpeedLevelCount         => (int)_vSpeedLevelsSlider.Value;

    public float MaxWindSpeedMetersPerSecond => _maxWindSpeedSlider.Value;
    public int   TrialsPerCombination        => (int)_trialsPerComboSlider.Value;

    public int TotalCombinations =>
        AltitudeLevelCount * HorizontalSpeedLevelCount * VerticalSpeedLevelCount;
    public int TotalTrials => TotalCombinations * TrialsPerCombination;

    public bool   RunRequested      => _runButton.WasClicked;
    public string SaveDirectoryPath => _savePathInput.Text;

    public void Render(int screenWidth, int screenHeight, bool isRunning)
    {
        _layout.RenderBackground(screenWidth, screenHeight);

        var y0  = _layout.ContentStartY(screenHeight);
        var col = _layout.ColumnWidth(screenWidth);

        var x0 = _layout.ColumnX(screenWidth, 0);
        var x1 = _layout.ColumnX(screenWidth, 1);
        var x2 = _layout.ColumnX(screenWidth, 2);
        var x3 = _layout.ColumnX(screenWidth, 3);
        var x4 = _layout.ColumnX(screenWidth, 4);
        var x5 = _layout.ColumnX(screenWidth, 5);

        RenderHeader("Drone Specs  [FIXED]", x0, y0);
        _massSlider.Render(x0, y0 + 18, col);
        _dragCoefficientSlider.Render(x0, y0 + 18 + SliderRowGap, col);
        _referenceAreaSlider.Render(x0, y0 + 18 + SliderRowGap * 2, col);

        RenderHeader("Altitude  [GRID]", x1, y0);
        _altitudeMinSlider.Render(x1, y0 + 18, col);
        _altitudeMaxSlider.Render(x1, y0 + 18 + SliderRowGap, col);
        _altitudeLevelsSlider.Render(x1, y0 + 18 + SliderRowGap * 2, col);

        RenderHeader("H-Speed  [GRID]", x2, y0);
        _hSpeedMaxSlider.Render(x2, y0 + 18, col);
        _hSpeedLevelsSlider.Render(x2, y0 + 18 + SliderRowGap, col);

        RenderHeader("V-Speed  [GRID]", x3, y0);
        _vSpeedMinSlider.Render(x3, y0 + 18, col);
        _vSpeedMaxSlider.Render(x3, y0 + 18 + SliderRowGap, col);
        _vSpeedLevelsSlider.Render(x3, y0 + 18 + SliderRowGap * 2, col);

        RenderHeader("Monte Carlo  [RANDOM]", x4, y0);
        _maxWindSpeedSlider.Render(x4, y0 + 18, col);
        _trialsPerComboSlider.Render(x4, y0 + 18 + SliderRowGap, col);
        RenderSweepStats(x4, y0 + 18 + SliderRowGap * 2 + 6);

        RenderHeader("Output", x5, y0);
        _savePathInput.Update(x5, y0 + 18, col);
        if (!isRunning)
            _runButton.Render(x5, y0 + 18 + SliderRowGap + 20, col, 36);
        else
            RenderRunningIndicator(x5, y0 + 18 + SliderRowGap + 20, col);

        RenderHelpText(screenWidth, screenHeight);
    }

    private static void RenderHeader(string text, int x, int y)
    {
        Raylib_cs.Raylib.DrawText(text, x, y, HeaderFontSize,
            new Color((byte)100, (byte)165, (byte)255, (byte)220));
    }

    private void RenderSweepStats(int x, int y)
    {
        Raylib_cs.Raylib.DrawText(
            $"Combos : {TotalCombinations,5:N0}", x, y,      13, Color.White);
        Raylib_cs.Raylib.DrawText(
            $"Trials : {TotalTrials,5:N0}",  x, y + 19, 13, Color.Yellow);
        Raylib_cs.Raylib.DrawText(
            $"Est    : ~{EstimatedMinutes():F0} min", x, y + 38, 13, Color.DarkGray);
    }

    private static void RenderRunningIndicator(int x, int y, int width)
    {
        Raylib_cs.Raylib.DrawRectangle(x, y, width, 36,
            new Color((byte)30, (byte)30, (byte)30, (byte)180));
        Raylib_cs.Raylib.DrawText("Running...", x + 8, y + 10, 14, Color.Green);
    }

    private float EstimatedMinutes() => TotalTrials * 0.08f / 60.0f;

    private static void RenderHelpText(int screenWidth, int screenHeight)
    {
        const string hint = "RMB: orbit  |  Scroll: zoom  |  Click Save Directory to type";
        var w = Raylib_cs.Raylib.MeasureText(hint, 11);
        Raylib_cs.Raylib.DrawText(
            hint, screenWidth - w - 8, screenHeight - 18, 11, Color.DarkGray);
    }
}

using Raylib_cs;
using DroneCrashSimulator.Visualization.UI.Controls;
using DroneCrashSimulator.Visualization.UI.Layout;

namespace DroneCrashSimulator.Visualization.UI;

public sealed class ControlPanel
{
    private readonly PanelLayout _layout;

    private readonly TextInputControl _latInput;
    private readonly TextInputControl _lonInput;
    private readonly SliderControl _altitudeSlider;

    private readonly SliderControl _massSlider;
    private readonly SliderControl _dragCoefficientSlider;
    private readonly SliderControl _referenceAreaSlider;
    private readonly SliderControl _cruiseSpeedSlider;

    private readonly SliderControl _maxWindSpeedSlider;
    private readonly SliderControl _trialCountSlider;

    private readonly ButtonControl _simulateButton;
    private readonly TextInputControl _savePathInput;
    private readonly ButtonControl _saveButton;

    public ControlPanel()
    {
        _layout = new PanelLayout();

        _latInput = new TextInputControl("GPS Latitude", "37.5665");
        _lonInput = new TextInputControl("GPS Longitude", "126.9780");
        _altitudeSlider = new SliderControl("Failure Altitude (m)", 20.0f, 500.0f, 100.0f);

        _massSlider = new SliderControl("Mass (kg)", 0.1f, 50.0f, 1.5f);
        _dragCoefficientSlider = new SliderControl("Drag Coeff (Cd)", 0.1f, 2.5f, 1.2f);
        _referenceAreaSlider = new SliderControl("Ref. Area (m^2)", 0.005f, 1.0f, 0.035f);
        _cruiseSpeedSlider = new SliderControl("Cruise Speed (m/s)", 1.0f, 30.0f, 8.0f);

        _maxWindSpeedSlider = new SliderControl("Max Wind (m/s)", 0.0f, 25.0f, 10.0f);
        _trialCountSlider = new SliderControl("Trials", 10.0f, 500.0f, 100.0f);

        _simulateButton = new ButtonControl("Simulate");
        _savePathInput = new TextInputControl(
            "Save Directory",
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        _saveButton = new ButtonControl("Save PNG + CSV");
    }

    public double LatitudeDegrees => TryParseDouble(_latInput.Text, 37.5665);
    public double LongitudeDegrees => TryParseDouble(_lonInput.Text, 126.9780);
    public float AltitudeMeters => _altitudeSlider.Value;
    public float MassKilograms => _massSlider.Value;
    public float DragCoefficient => _dragCoefficientSlider.Value;
    public float ReferenceAreaSquareMeters => _referenceAreaSlider.Value;
    public float CruiseSpeedMetersPerSecond => _cruiseSpeedSlider.Value;
    public float MaxWindSpeedMetersPerSecond => _maxWindSpeedSlider.Value;
    public int TrialCount => (int)_trialCountSlider.Value;
    public bool SimulateRequested => _simulateButton.WasClicked;
    public bool SaveRequested => _saveButton.WasClicked;
    public string SaveDirectoryPath => _savePathInput.Text;

    public void Render(int screenWidth, int screenHeight, bool saveEnabled)
    {
        _layout.RenderBackground(screenWidth, screenHeight);

        var y0 = _layout.ContentStartY(screenHeight);
        var col = _layout.ColumnWidth(screenWidth);
        const int pad = 14;

        RenderSectionHeader("GPS & Altitude [FIXED]", pad, y0 - 2);
        _latInput.Update(pad, y0 + 16, col - 22);
        _lonInput.Update(pad, y0 + 62, col - 22);
        _altitudeSlider.Render(pad, y0 + 112, col - 22);

        RenderSectionHeader("Drone Physics [FIXED]", col + pad, y0 - 2);
        _massSlider.Render(col + pad, y0 + 16, col - 22);
        _dragCoefficientSlider.Render(col + pad, y0 + 74, col - 22);
        _referenceAreaSlider.Render(col + pad, y0 + 132, col - 22);

        RenderSectionHeader("Aerodynamics [FIXED]", col * 2 + pad, y0 - 2);
        _cruiseSpeedSlider.Render(col * 2 + pad, y0 + 16, col - 22);
        RenderRandomisedLabel(col * 2 + pad, y0 + 78);

        RenderSectionHeader("Monte Carlo [RANDOM]", col * 3 + pad, y0 - 2);
        _maxWindSpeedSlider.Render(col * 3 + pad, y0 + 16, col - 22);
        _trialCountSlider.Render(col * 3 + pad, y0 + 74, col - 22);
        _simulateButton.Render(col * 3 + pad, y0 + 148, col - 22, 38);

        if (saveEnabled)
        {
            _savePathInput.Update(col * 4 + pad, y0 + 16, col - 22);
            _saveButton.Render(col * 4 + pad, y0 + 74, col - 22, 38);
        }
        else
        {
            Raylib_cs.Raylib.DrawText(
                "Run simulation first", col * 4 + pad, y0 + 36, 13, Color.DarkGray);
        }

        RenderHelpText(screenWidth, screenHeight);
    }

    private static void RenderSectionHeader(string text, int x, int y)
    {
        Raylib_cs.Raylib.DrawText(text, x, y, 12,
            new Color((byte)110, (byte)170, (byte)255, (byte)210));
    }

    private static void RenderRandomisedLabel(int x, int y)
    {
        var lines = new[]
        {
            "Randomised per trial:",
            "  Wind speed  [0, Max]",
            "  Wind direction 0-360",
            "  Init velocity dir",
            "  Init velocity speed",
            "  Attitude at failure"
        };
        for (var i = 0; i < lines.Length; i++)
            Raylib_cs.Raylib.DrawText(lines[i], x, y + i * 17, 12, Color.Gray);
    }

    private static void RenderHelpText(int screenWidth, int screenHeight)
    {
        Raylib_cs.Raylib.DrawText(
            "RMB: orbit  |  Scroll: zoom  |  Click GPS fields to type",
            screenWidth - 370, screenHeight - 20, 12, Color.DarkGray);
    }

    private static double TryParseDouble(string text, double fallback) =>
        double.TryParse(
            text,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v)
            ? v : fallback;
}

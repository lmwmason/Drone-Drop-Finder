namespace DroneCrashSimulator.Domain.Drones.Models;

public sealed class SpecifiedDroneModel : IDroneModel
{
    private readonly double _massKilograms;
    private readonly double _dragCoefficient;
    private readonly double _referenceAreaSquareMeters;
    private readonly DroneShape _shape;

    public SpecifiedDroneModel(
        double massKilograms,
        double dragCoefficient,
        double referenceAreaSquareMeters,
        double cruiseSpeedMetersPerSecond)
    {
        _massKilograms = massKilograms;
        _dragCoefficient = dragCoefficient;
        _referenceAreaSquareMeters = referenceAreaSquareMeters;
        CruiseSpeedMetersPerSecond = cruiseSpeedMetersPerSecond;

        var bodyRadius = Math.Sqrt(referenceAreaSquareMeters / Math.PI);
        _shape = new DroneShape(
            LengthMeters: bodyRadius * 2.0,
            WidthMeters: bodyRadius * 2.0,
            HeightMeters: bodyRadius * 0.4);
    }

    public string Name => "Custom Drone";
    public double MassKilograms => _massKilograms;
    public double CruiseSpeedMetersPerSecond { get; }

    public InertiaTensor InertiaTensor => ComputeBoxInertiaTensor();

    public DroneShape Shape => _shape;

    public AerodynamicProperties AerodynamicProperties => new(
        DragCoefficientFlatFace: _dragCoefficient,
        DragCoefficientSideFace: _dragCoefficient * 0.65,
        ReferenceAreaSquareMeters: _referenceAreaSquareMeters);

    private InertiaTensor ComputeBoxInertiaTensor()
    {
        var m = _massKilograms;
        var w = _shape.WidthMeters;
        var h = _shape.HeightMeters;
        var l = _shape.LengthMeters;
        return new InertiaTensor(
            IxxKilogramSquareMeters: m * (h * h + l * l) / 12.0,
            IyyKilogramSquareMeters: m * (w * w + l * l) / 12.0,
            IzzKilogramSquareMeters: m * (w * w + h * h) / 12.0);
    }
}

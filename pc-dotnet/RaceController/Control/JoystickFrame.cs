namespace RaceController;

internal sealed record JoystickFrame(
    int X,
    int Y,
    int Z,
    int Rz,
    double Ch1Normalized,
    double Ch2Normalized,
    double Throttle,
    double Brake)
{
    public JoystickFrame WithoutBrake()
    {
        return this with { Rz = RcMath.VJoyMin, Brake = 0.0 };
    }
}

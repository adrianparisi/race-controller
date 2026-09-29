namespace RaceController;

internal static class RcMath
{
    public const int VJoyMin = 1;
    public const int VJoyMax = 32768;

    public static double Clamp(double value, double minimum, double maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, value));
    }

    public static int SignedToVJoy(double value)
    {
        var scaled = (Clamp(value, -1.0, 1.0) + 1.0) / 2.0;
        return (int)Math.Round(VJoyMin + scaled * (VJoyMax - VJoyMin));
    }

    public static int PositiveToVJoy(double value)
    {
        return (int)Math.Round(VJoyMin + Clamp(value, 0.0, 1.0) * (VJoyMax - VJoyMin));
    }

    public static (double throttle, double brake) SplitCenteredAxis(double value, double deadzone)
    {
        if (Math.Abs(value) <= deadzone)
        {
            return (0.0, 0.0);
        }

        var usable = Math.Max(0.001, 1.0 - deadzone);
        if (value > 0)
        {
            return (Clamp((value - deadzone) / usable, 0.0, 1.0), 0.0);
        }

        return (0.0, Clamp((-value - deadzone) / usable, 0.0, 1.0));
    }
}

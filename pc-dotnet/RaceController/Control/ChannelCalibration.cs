namespace RaceController;

internal sealed record ChannelCalibration(int Minimum = 1000, int Center = 1500, int Maximum = 2000)
{
    public double NormalizedSigned(int pulseUs)
    {
        double value;
        if (pulseUs >= Center)
        {
            var span = Math.Max(1, Maximum - Center);
            value = (double)(pulseUs - Center) / span;
        }
        else
        {
            var span = Math.Max(1, Center - Minimum);
            value = -((double)(Center - pulseUs) / span);
        }

        return RcMath.Clamp(value, -1.0, 1.0);
    }
}

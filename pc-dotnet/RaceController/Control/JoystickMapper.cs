namespace RaceController;

internal sealed class JoystickMapper
{
    private readonly ChannelCalibration _ch1;
    private readonly ChannelCalibration _ch2;
    private readonly bool _invertCh1;
    private readonly bool _invertCh2;
    private readonly double _throttleDeadzone;

    public JoystickMapper(
        ChannelCalibration ch1,
        ChannelCalibration ch2,
        bool invertCh1,
        bool invertCh2,
        double throttleDeadzone)
    {
        _ch1 = ch1;
        _ch2 = ch2;
        _invertCh1 = invertCh1;
        _invertCh2 = invertCh2;
        _throttleDeadzone = throttleDeadzone;
    }

    public JoystickFrame Map(RcFrame frame)
    {
        var ch1Signed = _ch1.NormalizedSigned(frame.Ch1Us);
        var ch2Signed = _ch2.NormalizedSigned(frame.Ch2Us);

        if (_invertCh1)
        {
            ch1Signed = -ch1Signed;
        }

        if (_invertCh2)
        {
            ch2Signed = -ch2Signed;
        }

        var (throttle, brake) = RcMath.SplitCenteredAxis(ch2Signed, _throttleDeadzone);

        return new JoystickFrame(
            RcMath.SignedToVJoy(ch1Signed),
            RcMath.SignedToVJoy(ch2Signed),
            RcMath.PositiveToVJoy(throttle),
            RcMath.PositiveToVJoy(brake),
            ch1Signed,
            ch2Signed,
            throttle,
            brake);
    }
}

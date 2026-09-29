using RaceController;

namespace RaceController.Tests;

[TestClass]
public sealed class JoystickMapperTests
{
    [TestMethod]
    public void CenterMapsToCenteredSteeringAndNoPedals()
    {
        var mapper = CreateMapper();
        var joystick = mapper.Map(new RcFrame(1, 1500, 1500, "OK"));

        Assert.AreEqual(16384, joystick.X);
        Assert.AreEqual(16384, joystick.Y);
        Assert.AreEqual(RcMath.VJoyMin, joystick.Z);
        Assert.AreEqual(RcMath.VJoyMin, joystick.Rz);
        Assert.AreEqual(0.0, joystick.Throttle);
        Assert.AreEqual(0.0, joystick.Brake);
    }

    [TestMethod]
    public void ValuesClampAtEndpoints()
    {
        var mapper = CreateMapper();
        var joystick = mapper.Map(new RcFrame(1, 900, 2100, "OK"));

        Assert.AreEqual(RcMath.VJoyMin, joystick.X);
        Assert.AreEqual(RcMath.VJoyMax, joystick.Y);
        Assert.AreEqual(RcMath.VJoyMax, joystick.Z);
        Assert.AreEqual(RcMath.VJoyMin, joystick.Rz);
    }

    [TestMethod]
    public void InvertedCh2SplitsThrottleAndBrake()
    {
        var mapper = CreateMapper(invertCh2: true);

        var throttle = mapper.Map(new RcFrame(1, 1500, 1000, "OK"));
        var brake = mapper.Map(new RcFrame(2, 1500, 2000, "OK"));

        Assert.AreEqual(RcMath.VJoyMax, throttle.Z);
        Assert.AreEqual(RcMath.VJoyMin, throttle.Rz);
        Assert.AreEqual(RcMath.VJoyMin, brake.Z);
        Assert.AreEqual(RcMath.VJoyMax, brake.Rz);
    }

    [TestMethod]
    public void DeadzoneKeepsSmallNoiseOffPedals()
    {
        var mapper = CreateMapper(invertCh2: true, throttleDeadzone: 0.03);
        var joystick = mapper.Map(new RcFrame(1, 1500, 1490, "OK"));

        Assert.AreEqual(0.0, joystick.Throttle);
        Assert.AreEqual(0.0, joystick.Brake);
        Assert.AreEqual(RcMath.VJoyMin, joystick.Z);
        Assert.AreEqual(RcMath.VJoyMin, joystick.Rz);
    }

    private static JoystickMapper CreateMapper(
        bool invertCh1 = false,
        bool invertCh2 = false,
        double throttleDeadzone = 0.0)
    {
        return new JoystickMapper(
            new ChannelCalibration(),
            new ChannelCalibration(),
            invertCh1,
            invertCh2,
            throttleDeadzone);
    }
}

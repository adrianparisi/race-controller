using System.Globalization;
using System.IO.Ports;
using RaceController;

var options = Options.Parse(args);
if (options.ListPorts)
{
    foreach (var port in SerialPort.GetPortNames().OrderBy(x => x))
    {
        Console.WriteLine(port);
    }

    return 0;
}

if (string.IsNullOrWhiteSpace(options.Port))
{
    Console.Error.WriteLine("Error: --port es requerido salvo que uses --list-ports");
    return 1;
}

VJoyDevice? vjoy = null;
KeyboardThrottle? keyboardThrottle = null;

try
{
    vjoy = options.UseVJoy ? new VJoyDevice(options.VJoyDeviceId) : null;
    keyboardThrottle = options.KeyboardThrottle
        ? new KeyboardThrottle(options.KeyboardThrottleKey)
        : null;

    using var serial = new SerialPort(options.Port, options.Baud)
    {
        ReadTimeout = 1000,
        NewLine = "\n",
    };

    Console.WriteLine($"Abriendo {options.Port} a {options.Baud} baudios...");
    serial.Open();
    Console.WriteLine("Leyendo frames RC. Presiona Ctrl+C para salir.");

    var stopRequested = false;
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        stopRequested = true;
    };

    var mapper = new JoystickMapper(
        new ChannelCalibration(options.Ch1Min, options.Ch1Center, options.Ch1Max),
        new ChannelCalibration(options.Ch2Min, options.Ch2Center, options.Ch2Max),
        options.InvertCh1,
        options.InvertCh2,
        options.ThrottleDeadzone);

    while (!stopRequested)
    {
        var rcFrame = ReadFrame(serial);
        if (rcFrame is null)
        {
            continue;
        }

        var joystick = mapper.Map(rcFrame);
        var keyboardPressed = UpdateKeyboardThrottle(options, keyboardThrottle, rcFrame, joystick);
        var vjoyFrame = options.SuppressBrakeWhileKeyboardThrottle && keyboardPressed
            ? joystick.WithoutBrake()
            : joystick;

        if (vjoy is not null && rcFrame.Status == "OK")
        {
            vjoy.Update(vjoyFrame);
        }

        if (options.Print)
        {
            Console.WriteLine(FormatStatus(rcFrame, joystick, vjoyFrame, keyboardPressed));
        }

        if (options.RateLimitMs > 0)
        {
            Thread.Sleep(options.RateLimitMs);
        }
    }

    Console.WriteLine();
    Console.WriteLine("Listo.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
finally
{
    keyboardThrottle?.Release();
    vjoy?.Dispose();
}

static RcFrame? ReadFrame(SerialPort serial)
{
    try
    {
        return RcFrame.Parse(serial.ReadLine());
    }
    catch (TimeoutException)
    {
        return null;
    }
}

static bool UpdateKeyboardThrottle(
    Options options,
    KeyboardThrottle? keyboardThrottle,
    RcFrame rcFrame,
    JoystickFrame joystick)
{
    if (keyboardThrottle is null)
    {
        return false;
    }

    var keyboardValue = options.KeyboardThrottleSource switch
    {
        ThrottleSource.Throttle => joystick.Throttle,
        ThrottleSource.Brake => joystick.Brake,
        ThrottleSource.Either => Math.Max(joystick.Throttle, joystick.Brake),
        _ => joystick.Throttle,
    };

    var pressed = rcFrame.Status == "OK" && keyboardValue >= options.KeyboardThrottleThreshold;
    keyboardThrottle.Update(pressed);
    return pressed;
}

static string FormatStatus(
    RcFrame rcFrame,
    JoystickFrame joystick,
    JoystickFrame vjoyFrame,
    bool keyboardPressed)
{
    return string.Create(
        CultureInfo.InvariantCulture,
        $"seq={rcFrame.Sequence:000000} " +
        $"ch1={rcFrame.Ch1Us,4}us ({joystick.Ch1Normalized:+0.000;-0.000;+0.000}) " +
        $"ch2={rcFrame.Ch2Us,4}us ({joystick.Ch2Normalized:+0.000;-0.000;+0.000}) " +
        $"thr={joystick.Throttle:0.000} brk={joystick.Brake:0.000} key={(keyboardPressed ? 1 : 0)} " +
        $"vjoy=(x={vjoyFrame.X,5},y={vjoyFrame.Y,5},z={vjoyFrame.Z,5},rz={vjoyFrame.Rz,5}) " +
        $"status={rcFrame.Status}");
}

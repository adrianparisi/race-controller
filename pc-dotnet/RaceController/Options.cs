using System.Globalization;

namespace RaceController;

internal enum ThrottleSource
{
    Throttle,
    Brake,
    Either,
}

internal sealed class Options
{
    public string? Port { get; private init; }
    public int Baud { get; private init; } = 115200;
    public bool Print { get; private init; }
    public bool ListPorts { get; private init; }
    public bool UseVJoy { get; private init; }
    public uint VJoyDeviceId { get; private init; } = 1;
    public bool InvertCh1 { get; private init; }
    public bool InvertCh2 { get; private init; }
    public double ThrottleDeadzone { get; private init; } = 0.03;
    public bool KeyboardThrottle { get; private init; }
    public string KeyboardThrottleKey { get; private init; } = "up";
    public double KeyboardThrottleThreshold { get; private init; } = 0.03;
    public ThrottleSource KeyboardThrottleSource { get; private init; } = ThrottleSource.Throttle;
    public bool SuppressBrakeWhileKeyboardThrottle { get; private init; }
    public int RateLimitMs { get; private init; }
    public int Ch1Min { get; private init; } = 1000;
    public int Ch1Center { get; private init; } = 1500;
    public int Ch1Max { get; private init; } = 2000;
    public int Ch2Min { get; private init; } = 1000;
    public int Ch2Center { get; private init; } = 1500;
    public int Ch2Max { get; private init; } = 2000;

    public static Options Parse(string[] args)
    {
        var options = new MutableOptions();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            string Next()
            {
                if (++i >= args.Length)
                {
                    throw new ArgumentException($"Falta valor para {arg}");
                }

                return args[i];
            }

            switch (arg)
            {
                case "--help":
                case "-h":
                    PrintHelp();
                    Environment.Exit(0);
                    break;
                case "--port":
                    options.Port = Next();
                    break;
                case "--baud":
                    options.Baud = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--print":
                    options.Print = true;
                    break;
                case "--list-ports":
                    options.ListPorts = true;
                    break;
                case "--vjoy":
                    options.UseVJoy = true;
                    break;
                case "--vjoy-device":
                    options.VJoyDeviceId = uint.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--invert-ch1":
                    options.InvertCh1 = true;
                    break;
                case "--invert-ch2":
                    options.InvertCh2 = true;
                    break;
                case "--throttle-deadzone":
                    options.ThrottleDeadzone = double.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--keyboard-throttle":
                    options.KeyboardThrottle = true;
                    break;
                case "--keyboard-throttle-key":
                    options.KeyboardThrottleKey = Next();
                    break;
                case "--keyboard-throttle-threshold":
                    options.KeyboardThrottleThreshold = double.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--keyboard-throttle-source":
                    options.KeyboardThrottleSource = Next().ToLowerInvariant() switch
                    {
                        "throttle" => ThrottleSource.Throttle,
                        "brake" => ThrottleSource.Brake,
                        "either" => ThrottleSource.Either,
                        var value => throw new ArgumentException($"Fuente invalida: {value}"),
                    };
                    break;
                case "--suppress-brake-while-keyboard-throttle":
                    options.SuppressBrakeWhileKeyboardThrottle = true;
                    break;
                case "--rate-limit-ms":
                    options.RateLimitMs = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--ch1-min":
                    options.Ch1Min = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--ch1-center":
                    options.Ch1Center = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--ch1-max":
                    options.Ch1Max = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--ch2-min":
                    options.Ch2Min = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--ch2-center":
                    options.Ch2Center = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                case "--ch2-max":
                    options.Ch2Max = int.Parse(Next(), CultureInfo.InvariantCulture);
                    break;
                default:
                    throw new ArgumentException($"Argumento desconocido: {arg}");
            }
        }

        if (!options.Print && !options.UseVJoy)
        {
            options.Print = true;
        }

        return options.ToImmutable();
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
        RaceController .NET

        Uso:
          RaceController --port COM3 --vjoy --print --invert-ch2 --keyboard-throttle --keyboard-throttle-key up --keyboard-throttle-source brake --suppress-brake-while-keyboard-throttle

        Opciones principales:
          --list-ports
          --port COM3
          --vjoy
          --print
          --invert-ch1
          --invert-ch2
          --keyboard-throttle
          --keyboard-throttle-key up|w|space
          --keyboard-throttle-source throttle|brake|either
          --suppress-brake-while-keyboard-throttle
        """);
    }

    private sealed class MutableOptions
    {
        public string? Port { get; set; }
        public int Baud { get; set; } = 115200;
        public bool Print { get; set; }
        public bool ListPorts { get; set; }
        public bool UseVJoy { get; set; }
        public uint VJoyDeviceId { get; set; } = 1;
        public bool InvertCh1 { get; set; }
        public bool InvertCh2 { get; set; }
        public double ThrottleDeadzone { get; set; } = 0.03;
        public bool KeyboardThrottle { get; set; }
        public string KeyboardThrottleKey { get; set; } = "up";
        public double KeyboardThrottleThreshold { get; set; } = 0.03;
        public ThrottleSource KeyboardThrottleSource { get; set; } = ThrottleSource.Throttle;
        public bool SuppressBrakeWhileKeyboardThrottle { get; set; }
        public int RateLimitMs { get; set; }
        public int Ch1Min { get; set; } = 1000;
        public int Ch1Center { get; set; } = 1500;
        public int Ch1Max { get; set; } = 2000;
        public int Ch2Min { get; set; } = 1000;
        public int Ch2Center { get; set; } = 1500;
        public int Ch2Max { get; set; } = 2000;

        public Options ToImmutable()
        {
            return new Options
            {
                Port = Port,
                Baud = Baud,
                Print = Print,
                ListPorts = ListPorts,
                UseVJoy = UseVJoy,
                VJoyDeviceId = VJoyDeviceId,
                InvertCh1 = InvertCh1,
                InvertCh2 = InvertCh2,
                ThrottleDeadzone = ThrottleDeadzone,
                KeyboardThrottle = KeyboardThrottle,
                KeyboardThrottleKey = KeyboardThrottleKey,
                KeyboardThrottleThreshold = KeyboardThrottleThreshold,
                KeyboardThrottleSource = KeyboardThrottleSource,
                SuppressBrakeWhileKeyboardThrottle = SuppressBrakeWhileKeyboardThrottle,
                RateLimitMs = RateLimitMs,
                Ch1Min = Ch1Min,
                Ch1Center = Ch1Center,
                Ch1Max = Ch1Max,
                Ch2Min = Ch2Min,
                Ch2Center = Ch2Center,
                Ch2Max = Ch2Max,
            };
        }
    }
}

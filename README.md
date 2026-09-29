# RaceController
> Note: this project was created with help from ChatGPT.

<img width="1672" height="941" alt="diagram" src="https://github.com/user-attachments/assets/c9d539ca-d993-485b-b84b-3c4117beab69" />


RC receiver to PC controller adapter for a DumboRC P6DC(G) receiver and an Arduino Uno.

The current setup is:

1. The Arduino Uno reads PWM pulses from the RC receiver.
2. The Arduino sends channel values over USB serial.
3. The .NET PC app normalizes those values and feeds vJoy. It can also hold a keyboard key for throttle when a game does not handle the throttle axis well.

## Project Status

- Arduino firmware: `firmware/uno_pwm_serial/uno_pwm_serial.ino`
- .NET PC app: `pc-dotnet/RaceController`
- MSTest tests: `pc-dotnet/RaceController.Tests`
- Visual Studio solution: `RaceController.slnx`

## Hardware Wiring

Connect the DumboRC P6DC(G) receiver to the Arduino Uno like this:

| Receiver | Arduino Uno | Purpose |
|---|---:|---|
| CH1 signal / S | D2 | Steering |
| CH2 signal / S | D3 | Throttle / brake |
| GND / - | GND | Common ground |
| VCC / + | 5V or 5V BEC | Receiver power |

Minimal wiring diagram:

```text
P6DC(G) CH1 S  ---------------->  Arduino D2
P6DC(G) CH2 S  ---------------->  Arduino D3
P6DC(G) GND/-  ---------------->  Arduino GND
P6DC(G) VCC/+  ---- 5V/BEC ---->  Arduino 5V only if powering from Arduino
```

On typical servo-style connectors:

```text
S / signal  = PWM channel signal
+ / VCC     = positive power
- / GND     = ground
```

If using female-to-female servo leads:

- CH1: connect `S` to Arduino `D2`; you may also carry `+` and `-` if this same cable powers the receiver.
- CH2: connect `S` to Arduino `D3`.
- Ground: receiver `GND/-` must be connected to Arduino `GND`, even if the receiver uses an external BEC or battery.

Important notes:

- Power the receiver with a suitable 5 V source. The Arduino `5V` pin is fine for simple receiver-only tests, but use a separate BEC for servos or external loads.
- If the receiver uses an external power source, always connect receiver `GND` to Arduino `GND`.
- Do not connect receiver signal wires to analog pins for this firmware; use `D2` and `D3`.
- Do not power servos from the Arduino USB 5 V rail during initial tests.
- Do not connect an external 5 V source to the Arduino `5V` pin while also powering the Arduino through USB unless you know how that supply is isolated.

## Serial Protocol

The Arduino opens serial at `115200` baud and sends one CSV line roughly every 20 ms:

```text
RC,seq,ch1_us,ch2_us,status
```

Example:

```text
RC,1234,1502,1018,OK
```

Fields:

- `seq`: incremental sample counter.
- `ch1_us`: CH1 pulse width in microseconds.
- `ch2_us`: CH2 pulse width in microseconds.
- `status`: `OK`, `CH1_TIMEOUT`, `CH2_TIMEOUT`, or `CH1_CH2_TIMEOUT`.

Expected RC pulse range:

- minimum: `1000 us`
- center: `1500 us`
- maximum: `2000 us`

The firmware accepts valid pulses from `800` to `2200 us` to tolerate shifted endpoints.

## Arduino Firmware

Open `firmware/uno_pwm_serial/uno_pwm_serial.ino` in Arduino IDE.

Use:

- Board: `Arduino Uno`
- Port: the Arduino COM port

Upload the sketch.

For a first test, open Serial Monitor at `115200` baud and move steering/throttle. You should see something like:

```text
RC,15,1498,1503,OK
RC,16,1602,1504,OK
RC,17,1710,1505,OK
```

If a channel times out:

- check that the signal wire is on the correct Arduino pin;
- check common ground;
- check that the receiver is powered, bound, and receiving transmitter input.

## .NET PC App

Open in Visual Studio:

```text
J:\src\RaceController\RaceController.slnx
```

Build:

```powershell
cd J:\src\RaceController
dotnet build RaceController.slnx
```

Run tests:

```powershell
cd J:\src\RaceController
dotnet test pc-dotnet\RaceController.Tests\RaceController.Tests.csproj
```

List serial ports:

```powershell
pc-dotnet\RaceController\bin\Debug\net10.0-windows\win-x64\RaceController.exe --list-ports
```

Run with the default game-controller configuration:

```powershell
cd J:\src\RaceController
.\run-dotnet.ps1
```

Equivalent manual command:

```powershell
pc-dotnet\RaceController\bin\Debug\net10.0-windows\win-x64\RaceController.exe --port COM3 --vjoy --print --invert-ch2 --keyboard-throttle --keyboard-throttle-key up --keyboard-throttle-source brake --suppress-brake-while-keyboard-throttle
```

The build copies `vJoyInterface.dll` from `C:\Program Files\vJoy\x64` to the output directory.

## vJoy Setup

For PC games that can read a joystick/gamepad:

1. Install vJoy.
2. Open `Configure vJoy`.
3. Enable device 1.
4. Enable at least axes `X`, `Y`, `Z`, and `Rz`.
5. Run the app with `--vjoy`.
6. Check `joy.cpl` and confirm the virtual joystick moves.
7. Assign controls in the game if needed.

Initial mapping:

| RC input | Virtual joystick output |
|---|---|
| CH1 steering | X |
| CH2 combined throttle/brake | Y |
| CH2 separate throttle | Z |
| CH2 separate brake/reverse | Rz |

The `Y` axis is centered at rest. `Z` and `Rz` are zero at rest: throttle raises `Z`, brake/reverse raises `Rz`.

For the tested DumboRC setup, CH2 needs inversion:

```powershell
pc-dotnet\RaceController\bin\Debug\net10.0-windows\win-x64\RaceController.exe --port COM3 --vjoy --print --invert-ch2
```

If steering and brake/reverse work but throttle does not, and you do not want to change the game configuration, use keyboard throttle:

```powershell
pc-dotnet\RaceController\bin\Debug\net10.0-windows\win-x64\RaceController.exe --port COM3 --vjoy --print --invert-ch2 --keyboard-throttle
```

That mode keeps vJoy for steering/brake and holds the up-arrow key when CH2 detects throttle. If the game uses `W` for throttle:

```powershell
pc-dotnet\RaceController\bin\Debug\net10.0-windows\win-x64\RaceController.exe --port COM3 --vjoy --print --invert-ch2 --keyboard-throttle --keyboard-throttle-key w
```

Suggested in-game mapping:

| Action | Suggested input |
|---|---|
| Steering | vJoy X |
| Throttle | vJoy Z, or keyboard throttle mode |
| Brake / reverse | vJoy Rz |

Example: in Trackmania, this project was tested successfully with vJoy for steering/brake-reverse and keyboard throttle using the up-arrow key.

## Calibration

Default calibration assumes:

```text
min = 1000 us
center = 1500 us
max = 2000 us
```

You can adjust endpoints with arguments:

```powershell
pc-dotnet\RaceController\bin\Debug\net10.0-windows\win-x64\RaceController.exe --port COM3 --print --ch1-min 980 --ch1-center 1502 --ch1-max 2015 --ch2-min 1005 --ch2-center 1490 --ch2-max 1988
```

Practical calibration flow:

1. Run with `--print`.
2. Leave the controller at rest and note CH1/CH2 center values.
3. Move steering to both extremes and note CH1 min/max.
4. Move trigger to both extremes and note CH2 min/max.
5. Run again with those values.

## Next Step

Once CH1 and CH2 are solid, more P6DC(G) channels can be added using pin-change interrupts or a library such as PinChangeInterrupt. This first version uses `D2` and `D3` because they are the Uno external interrupt pins and are the most reliable starting point.

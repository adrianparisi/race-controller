using System.Runtime.InteropServices;

namespace RaceController.Native;

[Flags]
internal enum KeyEventFlags : uint
{
    None = 0,
    ExtendedKey = 0x0001,
    KeyUp = 0x0002,
    ScanCode = 0x0008,
}

internal enum InputType : uint
{
    Keyboard = 1,
}

[StructLayout(LayoutKind.Sequential)]
internal struct KeyboardInput
{
    public ushort VirtualKey;
    public ushort ScanCode;
    public KeyEventFlags Flags;
    public uint Time;
    public UIntPtr ExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MouseInput
{
    public int X;
    public int Y;
    public uint MouseData;
    public uint Flags;
    public uint Time;
    public UIntPtr ExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct HardwareInput
{
    public uint Message;
    public ushort ParamL;
    public ushort ParamH;
}

[StructLayout(LayoutKind.Explicit)]
internal struct InputUnion
{
    [FieldOffset(0)]
    public MouseInput Mouse;

    [FieldOffset(0)]
    public KeyboardInput Keyboard;

    [FieldOffset(0)]
    public HardwareInput Hardware;
}

[StructLayout(LayoutKind.Sequential)]
internal struct Input
{
    public InputType Type;
    public InputUnion Union;
}

internal static partial class User32
{
    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial uint SendInput(uint inputCount, [In] Input[] inputs, int size);

    [LibraryImport("user32.dll")]
    public static partial void keybd_event(byte virtualKey, ushort scanCode, KeyEventFlags flags, UIntPtr extraInfo);
}

using System.Runtime.InteropServices;

namespace RaceController.Native;

internal enum HidUsage : uint
{
    X = 0x30,
    Y = 0x31,
    Z = 0x32,
    Rz = 0x35,
}

internal enum VJoyStatus : uint
{
    Own = 0,
    Free = 1,
    Busy = 2,
    Missing = 3,
    Unknown = 4,
}

internal static partial class VJoyNative
{
    [LibraryImport("vJoyInterface.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool vJoyEnabled();

    [LibraryImport("vJoyInterface.dll")]
    public static partial VJoyStatus GetVJDStatus(uint rID);

    [LibraryImport("vJoyInterface.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AcquireVJD(uint rID);

    [LibraryImport("vJoyInterface.dll")]
    public static partial void RelinquishVJD(uint rID);

    [LibraryImport("vJoyInterface.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetAxis(int value, uint rID, HidUsage axis);
}

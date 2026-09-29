using RaceController.Native;

namespace RaceController;

internal sealed class VJoyDevice : IDisposable
{
    private readonly uint _deviceId;
    private bool _acquired;

    public VJoyDevice(uint deviceId)
    {
        _deviceId = deviceId;

        if (!VJoyNative.vJoyEnabled())
        {
            throw new InvalidOperationException("vJoy no esta habilitado.");
        }

        var status = VJoyNative.GetVJDStatus(_deviceId);
        if (status != VJoyStatus.Free && status != VJoyStatus.Own)
        {
            throw new InvalidOperationException($"vJoy device {_deviceId} no esta libre. Estado: {status}.");
        }

        if (!VJoyNative.AcquireVJD(_deviceId))
        {
            throw new InvalidOperationException($"No pude adquirir vJoy device {_deviceId}.");
        }

        _acquired = true;
    }

    public void Update(JoystickFrame frame)
    {
        SetAxis(frame.X, HidUsage.X);
        SetAxis(frame.Y, HidUsage.Y);
        SetAxis(frame.Z, HidUsage.Z);
        SetAxis(frame.Rz, HidUsage.Rz);
    }

    public void Dispose()
    {
        if (_acquired)
        {
            VJoyNative.RelinquishVJD(_deviceId);
            _acquired = false;
        }
    }

    private void SetAxis(int value, HidUsage axis)
    {
        if (!VJoyNative.SetAxis(value, _deviceId, axis))
        {
            throw new InvalidOperationException($"No pude escribir eje {axis} en vJoy.");
        }
    }
}

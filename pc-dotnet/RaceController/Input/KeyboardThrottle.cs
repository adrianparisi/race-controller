using System.Runtime.InteropServices;
using RaceController.Native;

namespace RaceController;

internal sealed class KeyboardThrottle
{
    private readonly KeyDefinition _key;
    private bool _isDown;

    public KeyboardThrottle(string keyName)
    {
        _key = KeyDefinition.FromName(keyName);
    }

    public void Update(bool pressed)
    {
        if (pressed == _isDown)
        {
            return;
        }

        Send(pressed);
        _isDown = pressed;
    }

    public void Release()
    {
        Update(false);
    }

    private void Send(bool pressed)
    {
        var flags = KeyEventFlags.ScanCode;
        if (_key.IsExtended)
        {
            flags |= KeyEventFlags.ExtendedKey;
        }

        if (!pressed)
        {
            flags |= KeyEventFlags.KeyUp;
        }

        var input = new Input
        {
            Type = InputType.Keyboard,
            Union = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = 0,
                    ScanCode = _key.ScanCode,
                    Flags = flags,
                    Time = 0,
                    ExtraInfo = UIntPtr.Zero,
                },
            },
        };

        var sent = User32.SendInput(1, [input], Marshal.SizeOf<Input>());
        if (sent == 1)
        {
            return;
        }

        var fallbackFlags = KeyEventFlags.None;
        if (_key.IsExtended)
        {
            fallbackFlags |= KeyEventFlags.ExtendedKey;
        }

        if (!pressed)
        {
            fallbackFlags |= KeyEventFlags.KeyUp;
        }

        User32.keybd_event(_key.VirtualKey, _key.ScanCode, fallbackFlags, UIntPtr.Zero);
    }
}

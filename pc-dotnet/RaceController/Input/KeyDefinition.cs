namespace RaceController;

internal readonly record struct KeyDefinition(byte VirtualKey, ushort ScanCode, bool IsExtended)
{
    public static KeyDefinition FromName(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "up" => new KeyDefinition(0x26, 0x48, true),
            "w" => new KeyDefinition(0x57, 0x11, false),
            "space" => new KeyDefinition(0x20, 0x39, false),
            _ => throw new ArgumentException("Tecla no soportada. Usa up, w o space."),
        };
    }
}

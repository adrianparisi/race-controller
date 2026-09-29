using System.Globalization;

namespace RaceController;

internal sealed record RcFrame(uint Sequence, int Ch1Us, int Ch2Us, string Status)
{
    public static RcFrame? Parse(string line)
    {
        var parts = line.Trim().Split(',');
        if (parts.Length != 5 || parts[0] != "RC")
        {
            return null;
        }

        if (!uint.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var sequence) ||
            !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ch1) ||
            !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ch2))
        {
            return null;
        }

        return new RcFrame(sequence, ch1, ch2, parts[4]);
    }
}

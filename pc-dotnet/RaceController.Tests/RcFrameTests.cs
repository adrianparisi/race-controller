using RaceController;

namespace RaceController.Tests;

[TestClass]
public sealed class RcFrameTests
{
    [TestMethod]
    public void ParseValidFrameReturnsValues()
    {
        var frame = RcFrame.Parse("RC,42,1501,1002,OK");

        Assert.IsNotNull(frame);
        Assert.AreEqual(42u, frame.Sequence);
        Assert.AreEqual(1501, frame.Ch1Us);
        Assert.AreEqual(1002, frame.Ch2Us);
        Assert.AreEqual("OK", frame.Status);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("hello")]
    [DataRow("RC,broken,1500,1500,OK")]
    [DataRow("RC,1,1500,OK")]
    public void ParseRejectsNoise(string line)
    {
        Assert.IsNull(RcFrame.Parse(line));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YourGameServer.UnitTests;

[TestClass]
public class PlayerCode_IsSane
{
    [TestMethod]
    public void CanDecodeFromStringCorrectly()
    {
        var pc = new PlayerCode(111);
        var pc2 = PlayerCode.FromString(pc.ToString());
        Assert.IsTrue(pc == pc2, $"decoded from string should be same with origin. {pc} : {pc2}");
    }

    // [TestMethod]
    // public void CanDecodeIDCorrectly()
    // {
    //     var pc = new PlayerCode(123456);
    //     Assert.IsTrue(pc.ToID() == 123456, $"decoded id should be same with origin. 123456 : {pc.ToID()}, {pc.ID64}");
    // }
}

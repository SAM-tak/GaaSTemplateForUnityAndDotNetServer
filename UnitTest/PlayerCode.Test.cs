using System.Diagnostics;
using System.Numerics;
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

    [TestMethod]
    public void Exam()
    {
        ulong oid = 123456;
        ulong id1 = oid * 0x1FFFFFFFFFFFFFFF;
        ulong id2 = (ulong)(new BigInteger(oid) * new BigInteger(0x1FFFFFFFFFFFFFFF) % (new BigInteger(ulong.MaxValue) + 1));
        Assert.IsTrue(id1 == id2, $"overflowed value should be same as mod value. {id1} : {id2}");
    }

    [TestMethod]
    public void CanDecodeIDCorrectly()
    {
        ulong oid = 123456;
        var pc = new PlayerCode(oid);
        var id = pc.ToID();
        Assert.IsTrue(id == oid, $"decoded id should be same with origin. {oid} : {id}, {pc.ID64}");
    }

    [TestMethod]
    public void CanDecodeIDCorrectly_LargeID()
    {
        ulong oid = 1234567890;
        var pc = new PlayerCode(oid);
        var id = pc.ToID();
        Assert.IsTrue(id == oid, $"decoded id should be same with origin. {oid} : {id}, {pc.ID64}");
    }

    [TestMethod]
    public void CanDecodeIDCorrectly_VeryLargeID()
    {
        ulong oid = 1234567890123456;
        var pc = new PlayerCode(oid);
        var id = pc.ToID();
        Assert.IsTrue(id == oid, $"decoded id should be same with origin. {oid} : {id}, {pc.ID64}");
    }
}

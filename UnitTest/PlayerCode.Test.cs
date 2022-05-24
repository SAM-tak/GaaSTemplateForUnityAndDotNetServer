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
        var pc1 = PlayerCode.FromID(123456);
        var pc2 = PlayerCode.FromString(pc1.ToString());
        Assert.IsTrue(pc1 == pc2, $"decoded from string should be same with origin. {pc1} : {pc2}");
    }

    [TestMethod]
    public void CanDecodeFromStringCorrectly_LargeID()
    {
        var pc1 = PlayerCode.FromID(1234567890);
        var pc2 = PlayerCode.FromString(pc1.ToString());
        Assert.IsTrue(pc1 == pc2, $"decoded from string should be same with origin. {pc1} : {pc2}");
    }

    [TestMethod]
    public void CanDecodeFromStringCorrectly_VeryLargeID()
    {
        var pc1 = PlayerCode.FromID(1234567890123456);
        var pc2 = PlayerCode.FromString(pc1.ToString());
        Assert.IsTrue(pc1 == pc2, $"decoded from string should be same with origin. {pc1} : {pc2}");
    }

    [TestMethod]
    public void ExamEquality()
    {
        ulong oid = 123456;
        ulong id1 = oid * 0x1FFFFFFFFFFFFFFF;
        ulong id2 = (ulong)(new BigInteger(oid) * new BigInteger(0x1FFFFFFFFFFFFFFF) % (new BigInteger(ulong.MaxValue) + 1));
        Assert.IsTrue(id1 == id2, $"overflowed value should be same as mod value. {id1} : {id2}");
    }

    [TestMethod]
    public void IsPrimePrimeToModulo()
    {
        var p = PlayerCode.Prime;
        var m = BigInteger.Pow(2, 64);
        Assert.IsTrue(p >= 3, $"not {p} >= 3");
        Assert.IsTrue(m > p, $"not {m} > {p}");
        Assert.IsTrue(m % p > 0, $"not {m} % {p} > 0");
    }

    [TestMethod]
    public void CanDecodeIDCorrectly()
    {
        ulong oid = 123456;
        var pc = PlayerCode.FromID(oid);
        var id = pc.ToID();
        Assert.IsTrue(id == oid, $"decoded id should be same with origin. {oid} : {id}, {pc.ID64}");
    }

    [TestMethod]
    public void CanDecodeIDCorrectly_LargeID()
    {
        ulong oid = 1234567890;
        var pc = PlayerCode.FromID(oid);
        var id = pc.ToID();
        Assert.IsTrue(id == oid, $"decoded id should be same with origin. {oid} : {id}, {pc.ID64}");
    }

    [TestMethod]
    public void CanDecodeIDCorrectly_VeryLargeID()
    {
        ulong oid = 1234567890123456;
        var pc = PlayerCode.FromID(oid);
        var id = pc.ToID();
        Assert.IsTrue(id == oid, $"decoded id should be same with origin. {oid} : {id}, {pc.ID64}");
    }
}

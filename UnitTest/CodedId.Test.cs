using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YourGameServer.UnitTests;

[TestClass]
public class CodedId_IsSane
{
    [TestMethod]
    public void CanDecodeHashidsCorrectly()
    {
        ulong oid = 123456;
        ushort osecret = 123;
        Assert.IsTrue((osecret | (int)((oid & 0xFF) << 16)) >= 0);
        Assert.IsTrue((int)((oid >> 8) & 0xFFFFFFF) >= 0);
        Assert.IsTrue((int)((oid >> 40) & 0xFFFFFFF) >= 0);
        var hashids = IDCoder.Encode(oid, 123);
        var (id, secret) = IDCoder.Decode(hashids);
        Assert.IsTrue(id == oid && secret == osecret, $"decoded id should be same with origin. {oid} : {id}, {hashids}");
    }

    [TestMethod]
    public void CanDecodeHashidsCorrectly_LargeID()
    {
        ulong oid = 1234567890123456;
        ushort osecret = 123;
        Assert.IsTrue((osecret | (int)((oid & 0xFF) << 16)) >= 0);
        Assert.IsTrue((int)((oid >> 8) & 0xFFFFFFF) >= 0);
        Assert.IsTrue((int)((oid >> 40) & 0xFFFFFFF) >= 0);
        var hashids = IDCoder.Encode(oid, osecret);
        var (id, secret) = IDCoder.Decode(hashids);
        Assert.IsTrue(id == oid && secret == osecret, $"decoded id should be same with origin. {oid} : {id}, {hashids}");
    }

    [TestMethod]
    public void CanDecodeHashidsCorrectly_VeryLargeID()
    {
        ulong oid = ulong.MaxValue - 100;
        ushort osecret = 123;
        Assert.IsTrue((osecret | (int)((oid & 0xFF) << 16)) >= 0);
        Assert.IsTrue((int)((oid >> 8) & 0xFFFFFFF) >= 0);
        Assert.IsTrue((int)((oid >> 40) & 0xFFFFFFF) >= 0);
        var hashids = IDCoder.Encode(oid, osecret);
        var (id, secret) = IDCoder.Decode(hashids);
        Assert.IsTrue(id == oid && secret == osecret, $"decoded id should be same with origin. {oid} : {id}, {hashids}");
    }
}

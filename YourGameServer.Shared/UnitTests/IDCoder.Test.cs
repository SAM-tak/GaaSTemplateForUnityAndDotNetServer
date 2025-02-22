using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YourGameServer.Shared.UnitTests;

[TestClass]
public class IDCoder_IsSane
{
    [TestInitialize]
    public void TestInitialize()
    {
        IDCoder.Initialize(234456);
    }

    [TestMethod]
    public void CanDecodeHashidsCorrectly()
    {
        ulong oid = 123456;
        Assert.IsTrue((int)((oid >> 8) & 0xFFFFFFF) >= 0);
        Assert.IsTrue((int)((oid >> 40) & 0xFFFFFFF) >= 0);
        var hashids = IDCoder.Encode(oid);
        var id = IDCoder.Decode(hashids);
        Assert.AreEqual(oid, id, $"decoded id should be same with origin. {oid} : {id}, {hashids}");
    }

    [TestMethod]
    public void CanDecodeLoginKeyCorrectly()
    {
        ulong oid = 301;
        Assert.IsTrue((int)((oid >> 8) & 0xFFFFFFF) >= 0);
        Assert.IsTrue((int)((oid >> 40) & 0xFFFFFFF) >= 0);
        var hashids = IDCoder.EncodeForLoginKey(oid);
        var id = IDCoder.DecodeFromLoginKey(hashids);
        Assert.AreEqual(oid, id, $"decoded id should be same with origin. {oid} : {id}, {hashids}");
    }

    [TestMethod]
    public void CanDecodeHashidsCorrectly_LargeID()
    {
        ulong oid = 1234567890123456;
        Assert.IsTrue((int)((oid >> 8) & 0xFFFFFFF) >= 0);
        Assert.IsTrue((int)((oid >> 40) & 0xFFFFFFF) >= 0);
        var hashids = IDCoder.Encode(oid);
        var id = IDCoder.Decode(hashids);
        Assert.AreEqual(oid, id, $"decoded id should be same with origin. {oid} : {id}, {hashids}");
    }

    [TestMethod]
    public void CanDecodeHashidsCorrectly_VeryLargeID()
    {
        ulong oid = ulong.MaxValue - 100;
        Assert.IsTrue((int)((oid >> 8) & 0xFFFFFFF) >= 0);
        Assert.IsTrue((int)((oid >> 40) & 0xFFFFFFF) >= 0);
        var hashids = IDCoder.Encode(oid);
        var id = IDCoder.Decode(hashids);
        Assert.AreEqual(oid, id, $"decoded id should be same with origin. {oid} : {id}, {hashids}");
    }
}

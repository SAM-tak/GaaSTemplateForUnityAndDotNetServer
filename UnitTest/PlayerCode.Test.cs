using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.ContentModel;

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
        var pc1 = PlayerCode.FromID(1234567890123456);
        var pc2 = PlayerCode.FromString(pc1.ToString());
        Assert.IsTrue(pc1 == pc2, $"decoded from string should be same with origin. {pc1} : {pc2}");
    }

    [TestMethod]
    public void CanDecodeFromStringCorrectly_VeryLargeID()
    {
        var pc1 = PlayerCode.FromID(ulong.MaxValue - 100);
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

    static bool IsPrime(ulong number)
    {
        if(number <= 1) return false;
        if(number == 2) return true;
        if(number % 2 == 0) return false;

        var boundary = (ulong)Math.Floor(Math.Sqrt(number));

        for(ulong i = 3; i <= boundary; i += 2)
            if(number % i == 0)
                return false;

        return true;
    }

    [TestMethod]
    public void HasPrimeInvMod()
    {
        var mod = new BigInteger(ulong.MaxValue) + 1;
        for(ulong i = 0x123456789ABCDEF; i < 0x123456789ABCDEF + 1000; ++i) {
            if(IsPrime(i)) {
                if(new BigInteger(i).TryModInverse(mod, out var modinv)) {
                    //Assert.Fail($"modinv found : 0x{i:X} -> 0x{modinv:X}");
                    return;
                }
                else Console.WriteLine($"modinv not found to 0x{i:X}");
            }
        }
        Assert.Fail("modinv not found in range");
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
    public void CanDecodeIDCorrectly_Zero()
    {
        ulong oid = 0;
        var pc = PlayerCode.FromID(oid);
        var id = pc.ToID();
        Assert.IsTrue(id == oid, $"decoded id should be same with origin. {oid} : {id}, {pc.ID64}");
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
        ulong oid = 1234567890123456;
        var pc = PlayerCode.FromID(oid);
        var id = pc.ToID();
        Assert.IsTrue(id == oid, $"decoded id should be same with origin. {oid} : {id}, {pc.ID64}");
    }

    [TestMethod]
    public void CanDecodeIDCorrectly_VeryLargeID()
    {
        ulong oid = ulong.MaxValue - 100;
        var pc = PlayerCode.FromID(oid);
        var id = pc.ToID();
        Assert.IsTrue(id == oid, $"decoded id should be same with origin. {oid} : {id}, {pc.ID64}");
    }
}

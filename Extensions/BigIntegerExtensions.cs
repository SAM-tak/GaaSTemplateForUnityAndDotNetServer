namespace YourGameServer.Extensions;

using System.Numerics;

public static class BigIntegerExtensions
{
    // source : https://stackoverflow.com/a/64125658/2330420
    static bool _TryModInverse(BigInteger number, BigInteger modulo, out BigInteger result)
    {
        if(number < 1) throw new ArgumentOutOfRangeException(nameof(number));
        if(modulo < 2) throw new ArgumentOutOfRangeException(nameof(modulo));
        // Extended Euclidean Algorithm
        BigInteger n = number, m = modulo, v = 0, d = 1;
        while(n > 0) {
            BigInteger t = m / n, x = n;
            n = m % x;
            m = x;
            x = d;
            d = checked(v - t * x); // Just in case
            v = x;
        }
        result = v % modulo;
        if(result < 0) result += modulo;
        return number * result % modulo == 1;
    }

    public static bool TryModInverse(this BigInteger number, BigInteger modulo, out BigInteger result)
    {
        if(_TryModInverse(number, modulo, out result)) return true;
        result = default;
        return false;
    }

    public static BigInteger ModInverse(this BigInteger number, BigInteger modulo)
    {
        if(_TryModInverse(number, modulo, out var result)) return result;
        throw new ArithmeticException($"Cannot solve : number * result % modulo ({number * result % modulo}) != 1");
    }
}

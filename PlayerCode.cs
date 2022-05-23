//#define USE_RANDOMGEN
#if !USE_RANDOMGEN
#define USE_SUFFLEDCHARLIST
#define USE_SHIFTCIPER
#define USE_MHASH
//#define COMPUTE_MODINV
#if COMPUTE_MODINV
using System.Numerics;
#endif
#endif
using System.Security.Cryptography;
namespace YourGameServer;

[Serializable]
public struct PlayerCode : IEquatable<PlayerCode>
{
    public ulong id0;
    public ushort id1;

    // ABCDEFGHJKLMNPQRSTUVWXYZ23456789 upper case only, expect 1 & I, 0 & O
#if USE_SUFFLEDCHARLIST
    // must be randomize per project
    private static readonly char[] _validCharacters = "YXEJLVCS465ZKBM8G9TRAPD7NF2HW3UQ".ToCharArray(); // length = 32
#else
    private static readonly char[] _validCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray(); // length = 32
#endif
    public const int StringPresentationLength = 16;
#if USE_SHIFTCIPER
    // must be randomize per project
    private static readonly int[] _digitToShift = { 3, 9, 4, 12, 7, 2, 14, 5, 1, 11, 13, 6, 10, 15, 0, 8 };
#endif
#if USE_MHASH
    private const ulong _prime = 0x1FFFFFFFFFFFFFFF; // Mersenne prime number #9
    private const ulong _modinv = 0xDFFFFFFFFFFFFFFF; // Precomputed modular multiplicative inverse for 0x1FFFFFFFFFFFFFFF mod 0x10000000000000000
#endif

    /// <summary>
    /// create from player id
    /// </summary>
    /// <remarks>
    /// This is guaranteed colision-free when id is unique.
    /// </remarks>
    public PlayerCode(ulong id)
    {
#if USE_MHASH
        id = id * _prime + 1; // this is minimum perfect hash function.
#endif
        var rnd = (ulong)RandomNumberGenerator.GetInt32(0x10000); // 16bit width random number
        id0 = ((rnd & 0b1) << 4)               |  (id & 0xF)                       // 5bit
            | ((rnd & 0b10) << 8)              | ((id & 0xF0) << 1)                // 10bit
            | ((rnd & 0b100) << 12)            | ((id & 0xF00) << 2)               // 15bit
            | ((rnd & 0b1000) << 16)           | ((id & 0xF000) << 3)              // 20bit
            | ((rnd & 0b10000) << 20)          | ((id & 0xF0000) << 4)             // 25bit
            | ((rnd & 0b100000) << 24)         | ((id & 0xF00000) << 5)            // 30bit
            | ((rnd & 0b1000000) << 28)        | ((id & 0xF000000) << 6)           // 35bit
            | ((rnd & 0b10000000) << 32)       | ((id & 0xF0000000) << 7)          // 40bit
            | ((rnd & 0b100000000) << 36)      | ((id & 0xF00000000) << 8)         // 45bit
            | ((rnd & 0b1000000000) << 40)     | ((id & 0xF000000000) << 9)        // 50bit
            | ((rnd & 0b10000000000) << 44)    | ((id & 0xF0000000000) << 10)      // 55bit
            | ((rnd & 0b100000000000) << 48)   | ((id & 0xF00000000000) << 11)     // 60bit
            |                                    ((id & 0xF000000000000) << 12)    // 64bit
        ;
        id1 = (ushort)(
              ((rnd & 0b1000000000000) >> 12)  | ((id & 0xF0000000000000) >> 51)   // 69bit
            | ((rnd & 0b10000000000000) >> 8)  | ((id & 0xF00000000000000) >> 50)  // 74bit
            | ((rnd & 0b100000000000000) >> 4) | ((id & 0xF000000000000000) >> 49) // 79bit
            |  (rnd & 0b1000000000000000)                                          // 80bit
        );
    }

    public static string NewString(ulong id) => new PlayerCode(id).ToString();

#if USE_RANDOMGEN
    static PlayerCode New()
    {
        // RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue) returns weird value
        var tmp1 = (ulong)RandomNumberGenerator.GetInt32(0x20000);
        var tmp2 = (ulong)RandomNumberGenerator.GetInt32(0x40000000);
        var tmp3 = (ulong)RandomNumberGenerator.GetInt32(0x40000000);
        return new PlayerCode {
            id0 = ((tmp1 & 0xF) << 60) | (tmp2 << 30) | tmp3,
            id1 = (ushort)(tmp1 >> 4),
        };
    }

    public static async Task<PlayerCode> NewAsync(Func<PlayerCode, Task<bool>> isUnique)
    {
        var result = New();
        while(isUnique != null && !await isUnique(result)) result = New();
        return result;
    }

    public static async Task<string> NewStringAsync(Func<string, Task<bool>> isUnique)
    {
        var result = New().ToString();
        while(isUnique != null && !await isUnique(result)) result = New().ToString();
        return result;
    }
#else
    public ulong ID64 =>
               (id0 & 0xF)                                   // 4bit
            | ((id0 & ((ulong)0xF0 << 1)) >> 1)              // 8bit
            | ((id0 & ((ulong)0xF00 << 2)) >> 2)             // 12bit
            | ((id0 & ((ulong)0xF000 << 3)) >> 3)            // 16bit
            | ((id0 & ((ulong)0xF0000 << 4)) >> 4)           // 20bit
            | ((id0 & ((ulong)0xF00000 << 5)) >> 5)          // 24bit
            | ((id0 & ((ulong)0xF000000 << 6)) >> 6)         // 28bit
            | ((id0 & ((ulong)0xF0000000 << 7)) >> 7)        // 32bit
            | ((id0 & ((ulong)0xF00000000 << 8)) >> 8)       // 36bit
            | ((id0 & ((ulong)0xF000000000 << 9)) >> 9)      // 40bit
            | ((id0 & ((ulong)0xF0000000000 << 10)) >> 10)   // 44bit
            | ((id0 & ((ulong)0xF00000000000 << 11)) >> 11)  // 48bit
            | ((id0 & ((ulong)0xF000000000000 << 12)) >> 12) // 52bit
            | (((ulong)id1 & (0xF << 1)) << (52 - 1))        // 56bit
            | (((ulong)id1 & (0xF0 << 2)) << (52 - 2))       // 60bit
            | (((ulong)id1 & (0xF00 << 3)) << (52 - 3))      // 64bit
            ;

#if COMPUTE_MODINV
    public static bool TryModInverse(BigInteger number, BigInteger modulo, out BigInteger result)
    {
        if(number < 1) throw new ArgumentOutOfRangeException(nameof(number));
        if(modulo < 2) throw new ArgumentOutOfRangeException(nameof(modulo));
        var n = number;
        var m = modulo;
        // Extended Euclidean Algorithm
        BigInteger v = 0, d = 1;
        while(n > 0) {
            var t = m / n;
            var x = n;
            n = m % x;
            m = x;
            x = d;
            d = checked(v - t * x); // Just in case
            v = x;
        }
        result = v % modulo;
        if(result < 0) result += modulo;
        if(number * result % modulo == 1) return true;
        result = default;
        return false;
    }
#endif

    public ulong ToID()
    {
#if USE_MHASH
        // a = x*p + 1 (mod m)
        // x = (a - 1)*p^-1 (mod m)
#if COMPUTE_MODINV
        if(TryModInverse(_prime, new BigInteger(ulong.MaxValue) + 1, out var modinv)) {
            Console.WriteLine($"modinv 0x{modinv:X}");
            return (ID64 - 1) * (ulong)modinv;
        }
        return 0;
#else
        return (ID64 - 1) * _modinv;
#endif
#else
        return ID64;
#endif
    }
#endif

    public static PlayerCode FromString(string source)
    {
        var ret = new PlayerCode();
        var digit = 0;
#if USE_SHIFTCIPER
        static int FromCharacter(char c, int digit) => (Array.IndexOf(_validCharacters, c) + 32 - _digitToShift[digit]) % 32;
#else
        static int FromCharacter(char c, int _) => Array.IndexOf(_validCharacters, c);
#endif
        foreach(var c in source) {
            if(_validCharacters.Contains(c)) {
                var i = FromCharacter(c, digit);
                if(digit < 3) {
                    ret.id1 |= (ushort)(i << 5 * (2 - digit) + 1);
                }
                else if(digit == 3) {
                    ret.id1 |= (ushort)(i >> 4);
                    ret.id0 |= (ulong)i << 60;
                }
                else if(digit < StringPresentationLength) {
                    ret.id0 |= (ulong)i << 5 * (15 - digit);
                }
                ++digit;
                if(digit == StringPresentationLength) break;
            }
            else if(!char.IsWhiteSpace(c) && c != '-') {
                throw new FormatException($"Illegal character found. {c}");
            }
        }
        if(digit < StringPresentationLength) {
            throw new ArgumentException($"source string is too short. {source}");
        }
        return ret;
    }

    public bool Equals(PlayerCode other) => id0 == other.id0 && id1 == other.id1;

    public override bool Equals(object? obj) => obj is PlayerCode code && Equals(code);

    public override int GetHashCode() => HashCode.Combine(id0, id1);

    public override string ToString()
    {
        var mid = (id1 & 0x1) << 4 | (int)(id0 >> 5 * 12) & 0x1F;
#if USE_SHIFTCIPER
        static char ToCharacter(int i, int digit) => _validCharacters[(i + _digitToShift[digit]) % 32];
#else
        static char ToCharacter(int i, int _) => _validCharacters[i];
#endif
        return new string(new char[] {
            ToCharacter(id1 >> 5 * 2 + 1   , 0),
            ToCharacter(id1 >> 5 + 1 & 0x1F, 1),
            ToCharacter(id1 >> 1     & 0x1F, 2),
            ToCharacter(mid, 3),

            ToCharacter((int)(id0 >> 5 * 11 & 0x1F), 4),
            ToCharacter((int)(id0 >> 5 * 10 & 0x1F), 5),
            ToCharacter((int)(id0 >> 5 * 9  & 0x1F), 6),
            ToCharacter((int)(id0 >> 5 * 8  & 0x1F), 7),

            ToCharacter((int)(id0 >> 5 * 7  & 0x1F), 8),
            ToCharacter((int)(id0 >> 5 * 6  & 0x1F), 9),
            ToCharacter((int)(id0 >> 5 * 5  & 0x1F), 10),
            ToCharacter((int)(id0 >> 5 * 4  & 0x1F), 11),

            ToCharacter((int)(id0 >> 5 * 3  & 0x1F), 12),
            ToCharacter((int)(id0 >> 5 * 2  & 0x1F), 13),
            ToCharacter((int)(id0 >> 5      & 0x1F), 14),
            ToCharacter((int)(id0           & 0x1F), 15)
        });
    }

    public static bool operator ==(PlayerCode left, PlayerCode right) => left.Equals(right);

    public static bool operator !=(PlayerCode left, PlayerCode right) => !(left == right);
}

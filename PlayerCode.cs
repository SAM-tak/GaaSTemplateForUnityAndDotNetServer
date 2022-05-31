#define USE_SHIFTCIPER
#define USE_MMHASH
#if USE_MMHASH
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
    private static readonly char[] _validCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray(); // length = 32
    public const int StringPresentationLength = 16;
#if USE_SHIFTCIPER
    // must be randomize per project
    private static readonly int[] _digitToShift = { 3, 9, 4, 12, 7, 2, 14, 5, 1, 11, 13, 6, 10, 15, 0, 8 };
#endif
#if USE_MMHASH
    public const ulong Prime = 0x123456789ABCE1B;
#if !COMPUTE_MODINV
    private const ulong _modinv = 0x5BBAC5AA496A5C13; // Precomputed modular multiplicative inverse for 0x123456789ABCE18 mod 0x10000000000000000
#endif
#else
    public const ulong Prime = 10007;
#endif

    /// <summary>
    /// create from player id
    /// </summary>
    /// <remarks>
    /// This is guaranteed colision-free when id is unique.
    /// </remarks>
    public static PlayerCode FromIDAndSecret(ulong id, ushort secret, bool hash)
    {
        if(hash) {
            // Mercari(metal_unk)'s multiplicative hash
            // source : https://engineering.mercari.com/blog/entry/2017-08-29-115047/
            // source : https://qiita.com/epsilon/items/aeefa085417b7134f793
            // a * p + 1 (mod m); p is prime, p ⊥ m, 3 <= p < m
            id = id * Prime + 1; // this is minimum perfect hash function.
        }

        var rnd = (ulong)secret; // 16bit width random number
        return new PlayerCode {
            id0 = ((rnd & 0b0000000000000001) << 4)  |  (id & 0x000000000000000F)         // 5bit
                | ((rnd & 0b0000000000000010) << 8)  | ((id & 0x00000000000000F0) << 1)   // 10bit
                | ((rnd & 0b0000000000000100) << 12) | ((id & 0x0000000000000F00) << 2)   // 15bit
                | ((rnd & 0b0000000000001000) << 16) | ((id & 0x000000000000F000) << 3)   // 20bit
                | ((rnd & 0b0000000000010000) << 20) | ((id & 0x00000000000F0000) << 4)   // 25bit
                | ((rnd & 0b0000000000100000) << 24) | ((id & 0x0000000000F00000) << 5)   // 30bit
                | ((rnd & 0b0000000001000000) << 28) | ((id & 0x000000000F000000) << 6)   // 35bit
                | ((rnd & 0b0000000010000000) << 32) | ((id & 0x00000000F0000000) << 7)   // 40bit
                | ((rnd & 0b0000000100000000) << 36) | ((id & 0x0000000F00000000) << 8)   // 45bit
                | ((rnd & 0b0000001000000000) << 40) | ((id & 0x000000F000000000) << 9)   // 50bit
                | ((rnd & 0b0000010000000000) << 44) | ((id & 0x00000F0000000000) << 10)  // 55bit
                | ((rnd & 0b0000100000000000) << 48) | ((id & 0x0000F00000000000) << 11)  // 60bit
                                                     | ((id & 0x000F000000000000) << 12), // 64bit
            id1 = (ushort)(
                  ((rnd & 0b0001000000000000) >> 12) | ((id & 0x00F0000000000000) >> 51)  // 69bit
                | ((rnd & 0b0010000000000000) >> 8)  | ((id & 0x0F00000000000000) >> 50)  // 74bit
                | ((rnd & 0b0100000000000000) >> 4)  | ((id & 0xF000000000000000) >> 49)  // 79bit
                |  (rnd & 0b1000000000000000)                                             // 80bit
            )
        };
    }

    public static PlayerCode FromID(ulong id, bool hash = true) => FromIDAndSecret(id, (ushort)RandomNumberGenerator.GetInt32(0x10000), hash);

    public static string NewStringFromIDAndSecret(ulong id, ushort secret, bool hash) => FromIDAndSecret(id, secret, hash).ToString("-");

    public static string NewStringFromID(ulong id) => FromID(id, true).ToString();

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

    public ulong ID64 =>
          ((id0 & ((ulong)0x000000000000F << 0)) >> 0)   // 4bit
        | ((id0 & ((ulong)0x00000000000F0 << 1)) >> 1)   // 8bit
        | ((id0 & ((ulong)0x0000000000F00 << 2)) >> 2)   // 12bit
        | ((id0 & ((ulong)0x000000000F000 << 3)) >> 3)   // 16bit
        | ((id0 & ((ulong)0x00000000F0000 << 4)) >> 4)   // 20bit
        | ((id0 & ((ulong)0x0000000F00000 << 5)) >> 5)   // 24bit
        | ((id0 & ((ulong)0x000000F000000 << 6)) >> 6)   // 28bit
        | ((id0 & ((ulong)0x00000F0000000 << 7)) >> 7)   // 32bit
        | ((id0 & ((ulong)0x0000F00000000 << 8)) >> 8)   // 36bit
        | ((id0 & ((ulong)0x000F000000000 << 9)) >> 9)   // 40bit
        | ((id0 & ((ulong)0x00F0000000000 << 10)) >> 10) // 44bit
        | ((id0 & ((ulong)0x0F00000000000 << 11)) >> 11) // 48bit
        | ((id0 & ((ulong)0xF000000000000 << 12)) >> 12) // 52bit
        | (((ulong)id1 & (0x000F << 1)) << (52 - 1))     // 56bit
        | (((ulong)id1 & (0x00F0 << 2)) << (52 - 2))     // 60bit
        | (((ulong)id1 & (0x0F00 << 3)) << (52 - 3))     // 64bit
        ;

    public ulong ToID(bool hash = true)
    {
        if(hash) {
#if COMPUTE_MODINV
            var modinv = new BigInteger(Prime).ModInverse(new BigInteger(ulong.MaxValue) + 1);
            Console.WriteLine($"modinv 0x{modinv:X}");
            return (ID64 - 1) * (ulong)modinv;
#else
            return (ID64 - 1) * _modinv;
#endif
        }
        return ID64;
    }

    public static PlayerCode FromString(string source, bool shift = true)
    {
        var ret = new PlayerCode();
        var digit = 0;
        int FromCharacter(char c, int digit) => shift ? (Array.IndexOf(_validCharacters, c) + 32 - _digitToShift[digit]) % 32 : Array.IndexOf(_validCharacters, c);
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

    public override string ToString() => ToString(true);

    public string ToString(bool shift)
    {
        var mid = (id1 & 0x1) << 4 | (int)(id0 >> 5 * 12) & 0x1F;
        char ToCharacter(int i, int digit) => shift ? _validCharacters[(i + _digitToShift[digit]) % 32] : _validCharacters[i];
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

    public string ToString(string fmt)
    {
        var ret = ToString(!fmt.Contains("noshift"));
        if(fmt.Contains('-')) {
            return $"{ret.AsSpan()[..4]}-{ret.AsSpan()[4..8]}-{ret.AsSpan()[8..12]}-{ret.AsSpan()[12..16]}";
        }
        return ret;
    }

    static readonly string[] _ngwords = { "FUCK", "PUSSY", "CUNT", "ANUS", "ASS" };

    public bool ContainsSensitiveWord {
        get {
            var code = ToString();
            return _ngwords.Any(i => code.Contains(i));
        }
    }

    public static bool operator ==(PlayerCode left, PlayerCode right) => left.Equals(right);

    public static bool operator !=(PlayerCode left, PlayerCode right) => !(left == right);
}

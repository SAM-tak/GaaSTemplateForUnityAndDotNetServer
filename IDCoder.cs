using System.Globalization;
using HashidsNet;

namespace YourGameServer;

public static class IDCoder
{
    static Hashids? _hashids = null;

    public static void Initialize(string salt)
    {
        _hashids = new(salt, 9, "abcdefghijknpqrstuvxyz23456789", "cfhistu");
    }

    public static string Encode(ulong id, ushort secret) => _hashids?.EncodeHex($"{id:X}{secret:X4}") ?? string.Empty;

    public static (ulong, ushort) Decode(string source)
    {
        var values = _hashids?.DecodeHex(source);
        return (ulong.Parse(values.AsSpan()[..^4], NumberStyles.HexNumber), ushort.Parse(values.AsSpan()[^4..], NumberStyles.HexNumber));
    }
}

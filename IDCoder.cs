using System.Globalization;
using HashidsNet;

namespace YourGameServer;

public static class IDCoder
{
    static readonly Hashids _hashids = new("jmEjWULJPpIwRveMMdxQEcHWRgKjJgPs", 10, "abcdefghijknpqrstuvxyz23456789", "cfhistu");

    public static string Encode(ulong id, ushort secret) => _hashids.EncodeHex($"{id:X}{secret:X4}");

    public static (ulong, ushort) Decode(string source)
    {
        var values = _hashids.DecodeHex(source);
        return (ulong.Parse(values.AsSpan()[..^4], NumberStyles.HexNumber), ushort.Parse(values.AsSpan()[^4..], NumberStyles.HexNumber));
    }
}

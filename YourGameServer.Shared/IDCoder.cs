using Sqids;

namespace YourGameServer.Shared;

public static class IDCoder
{
    static SqidsEncoder<ulong>? _sqids = null;
    static SqidsEncoder<ulong>? _sqidsForPlayerCode = null;

    public static void Initialize(int seed)
    {
        var alphabetsForPlayerCode = "abcdefghijknpqrstuvxyz23456789".ToCharArray();
        new Random(seed).Shuffle(alphabetsForPlayerCode.AsSpan());
        _sqidsForPlayerCode = new(new() { MinLength = 8, Alphabet = new string(alphabetsForPlayerCode) });
        var alphabetsForLoginKey = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        new Random(seed).Shuffle(alphabetsForLoginKey.AsSpan());
        _sqids = new(new() { MinLength = 4, Alphabet = new string(alphabetsForLoginKey) });
    }

    public static string Encode(ulong id) => _sqids?.Encode(id) ?? string.Empty;

    public static ulong Decode(string source) => _sqids?.Decode(source)?[0] ?? 0;

    public static string EncodeForPlayerCode(ulong id, ushort secret) => _sqidsForPlayerCode?.Encode([id, secret]) ?? string.Empty;

    public static (ulong, ushort) DecodeFromPlayerCode(string source)
    {
        var ids = _sqidsForPlayerCode?.Decode(source) ?? [0, 0];
        return (ids[0], (ushort)ids[1]);
    }
}

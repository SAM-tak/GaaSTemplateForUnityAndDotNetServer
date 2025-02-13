using Sqids;

namespace YourGameServer.Shared;

public static class IDCoder
{
    static SqidsEncoder<ulong>? _sqids = null;
    static SqidsEncoder<ulong>? _sqidsForLoginKey = null;

    public static void Initialize(int seed = 234456, int seedForLoginKey = 592183)
    {
        _sqids = CreateEncoder(seed);
        _sqidsForLoginKey = CreateEncoder(seedForLoginKey);
    }

    static SqidsEncoder<ulong>? CreateEncoder(int seed)
    {
        var alphabets = "abcdefghijknpqrstuvxyz23456789".ToCharArray();
        new Random(seed).Shuffle(alphabets.AsSpan());
        return new(new() { MinLength = 10, Alphabet = new string(alphabets) });
    }

    public static string Encode(ulong id) => _sqids?.Encode(id) ?? string.Empty;

    public static ulong Decode(string source)
    {
        var values = _sqids?.Decode(source);
        return values != null ? values[0] : 0;
    }

    public static string EncodeForLoginKey(ulong id) => _sqidsForLoginKey?.Encode(id) ?? string.Empty;

    public static ulong DecodeFromLoginKey(string source)
    {
        var values = _sqidsForLoginKey?.Decode(source);
        return values != null ? values[0] : 0;
    }
}

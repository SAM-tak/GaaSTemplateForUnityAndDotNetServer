using Sqids;

namespace YourGameServer.Shared;

public static class IDCoder
{
    static SqidsEncoder<ulong>? _sqids = null;
    static SqidsEncoder<ulong>? _sqidsForLoginKey = null;

    public static void Initialize(int seed = 234456)
    {
        var alphabets = "abcdefghijknpqrstuvxyz23456789".ToCharArray();
        new Random(seed).Shuffle(alphabets.AsSpan());
        _sqids = new(new() { MinLength = 8, Alphabet = new string(alphabets) });
        var alphabetsForLoginKey = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~!@#$%^&*()-=[];',._+{}|:<>?".ToCharArray();
        new Random(seed).Shuffle(alphabetsForLoginKey.AsSpan());
        _sqidsForLoginKey = new(new() { MinLength = 4, Alphabet = new string(alphabetsForLoginKey) });
    }

    public static string Encode(ulong id) => _sqids?.Encode(id) ?? string.Empty;

    public static ulong Decode(string source) => _sqids?.Decode(source)?[0] ?? 0;

    public static string EncodeForLoginKey(ulong id) => _sqidsForLoginKey?.Encode(id) ?? string.Empty;

    public static ulong DecodeFromLoginKey(string source) => _sqidsForLoginKey?.Decode(source)?[0] ?? 0;
}

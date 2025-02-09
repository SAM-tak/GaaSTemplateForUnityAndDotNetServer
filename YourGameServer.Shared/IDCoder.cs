using Sqids;

namespace YourGameServer.Shared;

public static class IDCoder
{
    static SqidsEncoder<ulong>? _sqids = null;

    public static void Initialize(int seed = 234456)
    {
        var alphabets = "abcdefghijknpqrstuvxyz23456789".ToCharArray();
        new Random(seed).Shuffle(alphabets.AsSpan());
        // Console.WriteLine($"shuffled {new string(alphabets)}");
        _sqids = new(new() { MinLength = 10, Alphabet = new string(alphabets) });
    }

    public static string Encode(ulong id, ushort secret) => _sqids?.Encode(id, secret) ?? string.Empty;

    public static (ulong, ushort) Decode(string source)
    {
        var values = _sqids?.Decode(source);
        return values != null ? (values[0], (ushort)values[1]) : (0, 0);
    }
}

namespace YourGameServer.Extensions;

using System.Text.RegularExpressions;

public static class StringExtensions
{
    static readonly Regex _4 = new(".{4}(?!$)");

    public static string ToHyphened(this string src)
    {
        if(src.Length < 6) return src;
        return (src.Length % 4) switch {
            1 => $"{src[..3]}-{src[3..6]}-{_4.Replace(src[6..], "$0-")}",
            2 => $"{src[..3]}-{_4.Replace(src[3..], "$0-")}",
            _ => _4.Replace(src, "$0-")
        };
    }
}

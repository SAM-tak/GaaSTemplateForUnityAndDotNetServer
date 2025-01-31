using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace YourGameServer.Explorer.Extensions;

static class EnumExtension
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var enumString = enumValue.ToString();
        return enumValue.GetType()?.GetMember(enumString)
                    .First()
                    .GetCustomAttribute<DisplayAttribute>()
                    ?.Name ?? enumString;
    }

    public static string GetDescription(this Enum enumValue)
    {
        var enumString = enumValue.ToString();
        return enumValue.GetType()?.GetMember(enumString)
                    .First()
                    .GetCustomAttribute<DisplayAttribute>()
                    ?.Description ?? enumString;
    }
}

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace YourGameServer.Extensions;

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

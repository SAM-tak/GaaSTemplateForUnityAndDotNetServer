using System.Reflection;

namespace YourGameServer.Shared.Extensions;

public static class MemberInfoExtension
{
    public static T? GetAttribute<T>(this MemberInfo member, bool isRequired) where T : Attribute
    {
        var attribute = member.GetCustomAttributes(typeof(T), false).SingleOrDefault();

        if(attribute == null && isRequired) {
            throw new ArgumentException($"The {typeof(T).Name} attribute must be defined on member {member.Name}");
        }

        return (T?)attribute;
    }
}

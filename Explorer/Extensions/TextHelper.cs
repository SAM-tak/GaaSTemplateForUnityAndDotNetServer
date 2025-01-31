using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using YourGameServer.Shared.Extensions;

namespace YourGameServer.Explorer.Extensions;

static class TextHelper
{
    public static string LabelFor<T>(Expression<Func<T, object?>> propertyExpression)
    {
        var memberInfo = GetPropertyInformation(propertyExpression.Body);
        if(memberInfo is null) {
            throw new ArgumentException("No property reference expression was found.", nameof(propertyExpression));
        }

        var attr = memberInfo.GetAttribute<DisplayAttribute>(false);
        if(attr is null) {
            return memberInfo.Name;
        }

        return attr.Name ?? memberInfo.Name;
    }

    public static string DescriptionFor<T>(Expression<Func<T, object?>> propertyExpression)
    {
        var memberInfo = GetPropertyInformation(propertyExpression.Body)
            ?? throw new ArgumentException("No property reference expression was found.", nameof(propertyExpression));
        var attr = memberInfo.GetAttribute<DisplayAttribute>(false);
        if(attr is null) {
            return memberInfo.Name;
        }

        return attr.Description ?? memberInfo.Name;
    }

    public static MemberInfo? GetPropertyInformation(Expression propertyExpression)
    {
        Debug.Assert(propertyExpression is not null, "propertyExpression is not null");
        var memberExpr = propertyExpression as MemberExpression;
        if(memberExpr is null) {
            if(propertyExpression is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert) {
                memberExpr = unaryExpr.Operand as MemberExpression;
            }
        }

        if(memberExpr is not null && memberExpr.Member.MemberType == MemberTypes.Property) {
            return memberExpr.Member;
        }

        return null;
    }
}

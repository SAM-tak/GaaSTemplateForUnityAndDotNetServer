using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace YourGameServer.Extensions;

public static class TextHelper
{
    public static string LabelFor<T>(Expression<Func<T, object>> propertyExpression)
    {
        var memberInfo = GetPropertyInformation(propertyExpression.Body);
        if(memberInfo == null) {
            throw new ArgumentException("No property reference expression was found.", nameof(propertyExpression));
        }

        var attr = memberInfo.GetAttribute<DisplayAttribute>(false);
        if(attr == null) {
            return memberInfo.Name;
        }

        return attr.Name;
    }

    public static string DescriptionFor<T>(Expression<Func<T, object>> propertyExpression)
    {
        var memberInfo = GetPropertyInformation(propertyExpression.Body);
        if(memberInfo == null) {
            throw new ArgumentException("No property reference expression was found.", nameof(propertyExpression));
        }

        var attr = memberInfo.GetAttribute<DisplayAttribute>(false);
        if(attr == null) {
            return memberInfo.Name;
        }

        return attr.Description;
    }

    public static MemberInfo GetPropertyInformation(Expression propertyExpression)
    {
        Debug.Assert(propertyExpression != null, "propertyExpression != null");
        var memberExpr = propertyExpression as MemberExpression;
        if(memberExpr == null) {
            if(propertyExpression is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert) {
                memberExpr = unaryExpr.Operand as MemberExpression;
            }
        }

        if(memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property) {
            return memberExpr.Member;
        }

        return null;
    }
}

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace WebApiGateway.Api.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())[0]
            .GetCustomAttribute<DisplayAttribute>()
            ?.GetName();
    }
}
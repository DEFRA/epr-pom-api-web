﻿namespace WebApiGateway.Api.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());
        if (memberInfo.Length > 0)
        {
            var attribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
            return attribute?.GetName();
        }
        return null;
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Mpv.NET.Player
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue) => enumValue.GetType()?.GetMember(enumValue.ToString())?.First()?.GetCustomAttribute<DisplayAttribute>()?.Name ?? enumValue.ToString();
    }

    public static class StringExtensions
    {
        public static T GetEnum<T>(this string value) where T : Enum => Enum.GetValues(typeof(T)).Cast<T>().FirstOrDefault(v => v.GetDisplayName() == value);
    }
}
using System;

namespace Vostok.Hercules.Serializer.Generator.Core.Helpers;

public static class StringUtils
{
    public static string RemoveSuffix(string value, string suffix,
        StringComparison comparison = StringComparison.Ordinal) =>
        value.EndsWith(suffix, comparison)
            ? value.Substring(0, value.Length - suffix.Length)
            : value;
}
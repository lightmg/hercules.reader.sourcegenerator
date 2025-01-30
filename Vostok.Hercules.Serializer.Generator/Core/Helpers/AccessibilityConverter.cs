using System;
using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Core.Helpers;

public static class AccessibilityConverter
{
    public static string ToSrcString(this Accessibility accessibility, bool throwOnDefault = true) =>
        accessibility switch
        {
            Accessibility.NotApplicable => throwOnDefault
                ? throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, "NotApplicable is not allowed")
                : "",
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.Public => "public",
            _ => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, null)
        };
}
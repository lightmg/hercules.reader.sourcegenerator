using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Services;

public static class TypeUtilities
{
    public static ITypeSymbol UnwrapNullable(ITypeSymbol type) =>
        IsNullable(type, out var underlying) ? underlying : type;

    public static bool IsNullable(ITypeSymbol type, out ITypeSymbol underlying)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            underlying = nullable.TypeArguments[0];
            return true;
        }

        underlying = type;
        return type.NullableAnnotation == NullableAnnotation.Annotated;
    }

    public static bool IsHerculesPrimitive(ITypeSymbol symbol) =>
        symbol.SpecialType is
            SpecialType.System_Byte or
            SpecialType.System_Boolean or
            SpecialType.System_Int16 or
            SpecialType.System_Int32 or
            SpecialType.System_Int64 or
            SpecialType.System_Double or
            SpecialType.System_String or
            SpecialType.System_Single
        || symbol.ToString() == "System.Guid";
}
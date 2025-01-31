﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Services;

public static class TypeUtilities
{
    public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, AttributeTypeBuilder attribute) =>
        symbol.GetAttributes().Where(a => a.AttributeClass?.ToString() == attribute.FullName);

    public static ITypeSymbol UnwrapNullable(ITypeSymbol type) =>
        IsNullable(type, out var underlying) ? underlying : type;

    public static bool TryParseEnum<T>(TypedConstant constant, out T enumValue) where T : struct, Enum
    {
        if (!IsEnum(constant))
        {
            enumValue = default;
            return false;
        }

        if (constant.Value is null)
        {
            enumValue = default;
            return false;
        }

        var sourceEnumValues = constant.Type!
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.HasConstantValue)
            .Select(f => new KeyValuePair<string, object>(f.Name, f.ConstantValue!))
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var current in GetEnumKeysWithValues<T>())
        {
            if (sourceEnumValues.TryGetValue(current.Key, out var value) && current.Value.Equals(value)) 
                continue;
            enumValue = default;
            return false;
        }

        enumValue = (T)constant.Value;
        return true;
    }

    public static bool IsEnum(TypedConstant constant) =>
        constant.Type?.BaseType?.ToString() == "System.Enum";

    public static bool TryGetEnumValueName(TypedConstant constant, out string name)
    {
        if (IsEnum(constant))
        {
            name = string.Empty;
            return false;
        }

        var enumFieldSymbol = constant.Type?
            .GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.HasConstantValue && f.ConstantValue == constant.Value);

        if (enumFieldSymbol == null)
        {
            name = string.Empty;
            return false;
        }

        name = enumFieldSymbol.Name;
        return true;
    }

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

    public static bool HasParameterlessCtor(INamedTypeSymbol symbol, Func<IMethodSymbol, bool>? predicate = null)
    {
        foreach (var ctor in symbol.Constructors)
            if (!ctor.IsStatic && ctor.Parameters.Length == 0 && predicate?.Invoke(ctor) is not false)
                return true;

        return false;
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

    public static bool IsHerculesPrimitive(ReferencedType type) =>
        type.Is<byte>() ||
        type.Is<bool>() ||
        type.Is<short>() ||
        type.Is<int>() ||
        type.Is<long>() ||
        type.Is<double>() ||
        type.Is<string>() ||
        type.Is<float>() ||
        type.Is<Guid>();

    public static IEnumerable<KeyValuePair<string, object>> GetEnumKeysWithValues<TEnum>() where TEnum : struct, Enum
    {
        var enumType = typeof(TEnum);
        foreach (var value in Enum.GetValues(enumType))
            yield return new KeyValuePair<string, object>(
                Enum.GetName(enumType, value), 
                Convert.ChangeType(value, Enum.GetUnderlyingType(enumType))
            );
    }

    private static bool Is<T>(this ReferencedType type) =>
        type.FullName == typeof(T).FullName;
}
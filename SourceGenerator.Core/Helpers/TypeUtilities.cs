using Microsoft.CodeAnalysis;

namespace SourceGenerator.Core.Helpers;

public sealed class TypeUtilities
{
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

    public static IEnumerable<KeyValuePair<string, object>> GetEnumKeysWithValues<TEnum>() where TEnum : struct, Enum
    {
        var enumType = typeof(TEnum);
        foreach (var value in Enum.GetValues(enumType))
            yield return new KeyValuePair<string, object>(
                Enum.GetName(enumType, value),
                Convert.ChangeType(value, Enum.GetUnderlyingType(enumType))
            );
    }
}
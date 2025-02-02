using System;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Core.Primitives;

public readonly struct TypeDescriptor(string fullName) : IEquatable<TypeDescriptor>
{
    public string FullName { get; } = fullName;

    public override string ToString() =>
        FullName;

    public bool Equals(TypeDescriptor other) =>
        string.Equals(FullName, other.FullName, StringComparison.Ordinal);

    public override bool Equals(object? obj) =>
        obj is TypeDescriptor other && Equals(other);

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(FullName);

    public static bool operator ==(TypeDescriptor left, TypeDescriptor right) =>
        left.Equals(right);

    public static bool operator !=(TypeDescriptor left, TypeDescriptor right) =>
        !left.Equals(right);

    public static TypeDescriptor Void => new("void");

    public static TypeDescriptor From<T>() =>
        From(typeof(T));

    public static TypeDescriptor From(Type type) =>
        From(type.FullName!);

    public static TypeDescriptor From(ITypeSymbol type) =>
        type.SpecialType switch
        {
            SpecialType.System_Object => typeof(object),
            SpecialType.System_Void => Void,
            SpecialType.System_Boolean => typeof(bool),
            SpecialType.System_Char => typeof(char),
            SpecialType.System_SByte => typeof(sbyte),
            SpecialType.System_Byte => typeof(byte),
            SpecialType.System_Int16 => typeof(short),
            SpecialType.System_UInt16 => typeof(ushort),
            SpecialType.System_Int32 => typeof(int),
            SpecialType.System_UInt32 => typeof(uint),
            SpecialType.System_Int64 => typeof(long),
            SpecialType.System_UInt64 => typeof(ulong),
            SpecialType.System_Decimal => typeof(decimal),
            SpecialType.System_Single => typeof(float),
            SpecialType.System_Double => typeof(double),
            SpecialType.System_String => typeof(string),
            _ => From(type.ToString())
        };

    public static TypeDescriptor From(ITypeBuilder typeBuilder) =>
        From(typeBuilder.FullName);

    public static TypeDescriptor From(string ns, string name) =>
        From($"{ns}.{name}");

    public static TypeDescriptor From(string fullyQualifiedName) =>
        new(fullyQualifiedName);

    public static TypeDescriptor From(GenericTypeBuilder genericType) =>
        new(genericType.Name);

    public static implicit operator TypeDescriptor(Type type) => From(type);

    public static implicit operator TypeDescriptor(TypeBuilder typeBuilder) => From(typeBuilder);

    public static implicit operator TypeDescriptor(string fullName) => From(fullName);
}
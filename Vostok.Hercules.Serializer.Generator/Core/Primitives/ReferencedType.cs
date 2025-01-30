using System;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;

namespace Vostok.Hercules.Serializer.Generator.Core.Primitives;

public readonly struct ReferencedType(string fullName) : IEquatable<ReferencedType>
{
    public string FullName { get; } = fullName;

    public override string ToString() => 
        FullName;

    public bool Equals(ReferencedType other) => 
        FullName == other.FullName;

    public override bool Equals(object? obj) => 
        obj is ReferencedType other && Equals(other);

    public override int GetHashCode() => 
        FullName.GetHashCode();

    public static bool operator ==(ReferencedType left, ReferencedType right) => 
        left.Equals(right);

    public static bool operator !=(ReferencedType left, ReferencedType right) => 
        !left.Equals(right);

    public static ReferencedType Void => new("void");

    public static ReferencedType From<T>() =>
        From(typeof(T));

    public static ReferencedType From(Type type) =>
        From(type.FullName!);

    public static ReferencedType From(TypeBuilder typeBuilder) =>
        From(typeBuilder.FullName);

    public static ReferencedType From(string ns, string name) =>
        From($"{ns}.{name}");

    public static ReferencedType From(string fullyQualifiedName) =>
        new(fullyQualifiedName);

    public static ReferencedType From(GenericTypeBuilder genericType) =>
        new(genericType.Name);

    public static implicit operator ReferencedType(Type type) => From(type);

    public static implicit operator ReferencedType(TypeBuilder typeBuilder) => From(typeBuilder);

    public static implicit operator ReferencedType(string fullName) => From(fullName);

}
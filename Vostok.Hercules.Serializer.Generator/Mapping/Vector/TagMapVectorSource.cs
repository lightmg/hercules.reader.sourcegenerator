using System;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Vector;

public sealed class TagMapVectorSource(string key, TypeDescriptor elementType)
    : ITagMapSource, IEquatable<TagMapVectorSource>
{
    public string Key { get; } = key;

    public TypeDescriptor ElementType { get; } = elementType;

    public bool Equals(TagMapVectorSource? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key && ElementType.Equals(other.ElementType);
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || obj is TagMapVectorSource other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Key.GetHashCode();
            hashCode = (hashCode * 397) ^ ElementType.GetHashCode();
            return hashCode;
        }
    }
}
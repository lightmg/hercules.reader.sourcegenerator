using System;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Models.Sources;

public sealed class TagMapVectorSource(string key, ReferencedType targetType, ReferencedType elementType)
    : ITagMapSource, IEquatable<TagMapVectorSource>
{
    public string Key { get; } = key;

    public ReferencedType TargetType { get; } = targetType;

    public ReferencedType ElementType { get; } = elementType;

    ReferencedType ITagMapSource.Type => TargetType;

    public bool Equals(TagMapVectorSource? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key && TargetType.Equals(other.TargetType) && ElementType.Equals(other.ElementType);
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || obj is TagMapVectorSource other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Key.GetHashCode();
            hashCode = (hashCode * 397) ^ TargetType.GetHashCode();
            hashCode = (hashCode * 397) ^ ElementType.GetHashCode();
            return hashCode;
        }
    }
}
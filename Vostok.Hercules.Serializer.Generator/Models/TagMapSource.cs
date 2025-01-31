using System;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Models;

public interface ITagMapSource
{
    public ReferencedType Type { get; }
}

public sealed class TagMapKeySource(string key, ReferencedType type) : ITagMapSource, IEquatable<TagMapKeySource>
{
    public string Key { get; } = key;
    public ReferencedType Type { get; } = type;

    public bool Equals(TagMapKeySource? other) =>
        other is not null &&
        (ReferenceEquals(this, other) || Key == other.Key && Type == other.Type);

    public override bool Equals(object? obj) =>
        obj is TagMapKeySource source && Equals(source);

    public override int GetHashCode() =>
        unchecked((Key.GetHashCode() * 397) ^ Type.GetHashCode());
}

public sealed class TagMapSpecialSource(SpecialTagKind kind, ReferencedType type) 
    : ITagMapSource, IEquatable<TagMapSpecialSource>
{
    public SpecialTagKind Kind { get; } = kind;
    public ReferencedType Type { get; } = type;

    public bool Equals(TagMapSpecialSource? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Kind == other.Kind && Type.Equals(other.Type);
    }

    public override bool Equals(object? obj) =>
        obj is TagMapSpecialSource source && Equals(source);

    public override int GetHashCode() => 
        unchecked(((int)Kind * 397) ^ Type.GetHashCode());
}
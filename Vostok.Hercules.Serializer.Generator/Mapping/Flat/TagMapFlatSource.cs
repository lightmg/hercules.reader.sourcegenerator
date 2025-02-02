using System;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Flat;

public sealed class TagMapFlatSource(string key, TypeDescriptor type) : ITagMapSource, IEquatable<TagMapFlatSource>
{
    public string Key { get; } = key;

    public TypeDescriptor Type { get; } = type;

    public bool Equals(TagMapFlatSource? other) =>
        other is not null &&
        (ReferenceEquals(this, other) || Key == other.Key && Type == other.Type);

    public override bool Equals(object? obj) =>
        obj is TagMapFlatSource source && Equals(source);

    public override int GetHashCode() =>
        unchecked((Key.GetHashCode() * 397) ^ Type.GetHashCode());
}
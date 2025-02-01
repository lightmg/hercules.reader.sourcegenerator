using System;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Models.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Models.Timestamp;

public sealed class TagMapTimestampSource
    : ITagMapSource, IEquatable<TagMapTimestampSource>
{
    public ReferencedType Type { get; } = typeof(DateTimeOffset);

    public bool Equals(TagMapTimestampSource? other) => 
        other != null && Type.Equals(other.Type);

    public override bool Equals(object? obj) =>
        obj is TagMapTimestampSource source && source.Type.Equals(Type);

    public override int GetHashCode() => 
        Type.GetHashCode();
}
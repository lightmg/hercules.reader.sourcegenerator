using System;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Timestamp;

public sealed class TagMapTimestampSource : ITagMapSource, IEquatable<TagMapTimestampSource>
{
    public bool Equals(TagMapTimestampSource? other) => 
        true;

    public override bool Equals(object? obj) => 
        obj is TagMapTimestampSource other && Equals(other);

    public override int GetHashCode() =>
        0;
}
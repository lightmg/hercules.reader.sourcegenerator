using System;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Container;

public class TagMapContainerSource(string key) : ITagMapSource, IEquatable<TagMapContainerSource>
{
    public string Key { get; } = key;

    public bool Equals(TagMapContainerSource? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TagMapContainerSource)obj);
    }

    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }
}
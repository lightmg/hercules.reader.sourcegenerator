using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Extensions;
using Vostok.Hercules.Serializer.Generator.Models.Sources;

namespace Vostok.Hercules.Serializer.Generator.Models;

public class EventMapping : IEquatable<EventMapping>
{
    public EventMapping(INamedTypeSymbol type)
    {
        Type = type;
    }

    public INamedTypeSymbol Type { get; }

    public IList<TagMap> Entries { get; } = [];

    public IEnumerable<TagMap<T>> EntriesWithSource<T>() where T : ITagMapSource =>
        Entries.Where(e => e.Is<T>()).Select(e => e.Cast<T>());

    public bool Equals(EventMapping? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return SymbolEqualityComparer.Default.Equals(Type, other.Type) &&
               Entries.Equals(other.Entries);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == typeof(EventMapping) &&
               Equals((EventMapping)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (SymbolEqualityComparer.Default.GetHashCode(Type) * 397) ^ Entries.GetElementsHashCode();
        }
    }
}
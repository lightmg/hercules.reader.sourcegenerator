﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Extensions;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping;

public class EventMapping : IEquatable<EventMapping>
{
    public EventMapping(INamedTypeSymbol type)
    {
        Type = type;
    }

    public INamedTypeSymbol Type { get; }

    public IList<ITagMap> Entries { get; } = [];

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
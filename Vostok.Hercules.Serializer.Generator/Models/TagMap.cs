using System;
using Vostok.Hercules.Serializer.Generator.Models.Sources;

namespace Vostok.Hercules.Serializer.Generator.Models;

public readonly struct TagMap<TSource>(TagMap impl)
    where TSource : ITagMapSource
{
    private readonly TagMap impl = impl;

    public TSource Source => (TSource)impl.Source;
    public TagMapTarget Target => impl.Target;
    public TagMapConverter? Converter => impl.Converter;

    public static implicit operator TagMap(TagMap<TSource> map) => map.impl;
}

public class TagMap
{
    public readonly ITagMapSource Source;
    public readonly TagMapTarget Target;
    public readonly TagMapConverter? Converter;

    public TagMap(ITagMapSource source, TagMapTarget target, TagMapConverter? converter)
    {
        Source = source;
        Target = target;
        Converter = converter;
    }

    public bool Is<T>() where T : ITagMapSource =>
        Source is T;

    public TagMap<T>? As<T>() where T : ITagMapSource =>
        Is<T>() ? new(this) : null;

    public TagMap<T> Cast<T>() where T : ITagMapSource =>
        Is<T>() ? new(this) : throw new InvalidCastException();
}
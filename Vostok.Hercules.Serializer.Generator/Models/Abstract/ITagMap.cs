namespace Vostok.Hercules.Serializer.Generator.Models.Abstract;

public interface ITagMap<out TSource> : ITagMap where TSource : ITagMapSource
{
    new TSource Source { get; }
}

public interface ITagMap
{
    ITagMapSource Source { get; }

    TagMapTarget Target { get; }

    TagMapConverter? Converter { get; }
}
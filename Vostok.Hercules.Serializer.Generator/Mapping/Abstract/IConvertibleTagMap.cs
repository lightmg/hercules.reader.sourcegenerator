namespace Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

public interface IConvertibleTagMap<out TSource> : ITagMap<TSource>, IConvertibleTagMap where TSource : ITagMapSource;

public interface IConvertibleTagMap : ITagMap
{
    TagMapConverter? Converter { get; }
}
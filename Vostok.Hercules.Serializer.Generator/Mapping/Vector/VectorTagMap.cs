using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Vector;

public class VectorTagMap(TagMapVectorSource source, TagMapVectorTarget target, TagMapConverter? converter)
    : IConvertibleTagMap<TagMapVectorSource>
{
    ITagMapSource ITagMap.Source => Source;
    TagMapTarget ITagMap.Target => Target;

    public TagMapVectorSource Source { get; } = source;

    public TagMapVectorTarget Target { get; } = target;

    public TagMapConverter? Converter { get; } = converter;
}
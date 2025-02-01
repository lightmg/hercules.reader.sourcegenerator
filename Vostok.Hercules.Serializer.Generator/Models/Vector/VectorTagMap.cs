using Vostok.Hercules.Serializer.Generator.Models.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Models.Vector;

public class VectorTagMap(TagMapVectorSource source, TagMapVectorTarget target, TagMapConverter? converter)
    : ITagMap<TagMapVectorSource>
{
    ITagMapSource ITagMap.Source => Source;
    TagMapTarget ITagMap.Target => Target;

    public TagMapVectorSource Source { get; } = source;

    public TagMapVectorTarget Target { get; } = target;

    public TagMapConverter? Converter { get; } = converter;
}
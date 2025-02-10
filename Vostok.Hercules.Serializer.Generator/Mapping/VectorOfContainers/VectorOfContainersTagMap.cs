using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;
using Vostok.Hercules.Serializer.Generator.Mapping.Vector;

namespace Vostok.Hercules.Serializer.Generator.Mapping.VectorOfContainers;

public class VectorOfContainersTagMap(TagMapVectorSource source, TagMapVectorTarget target)
    : ITagMap<TagMapVectorSource>
{
    ITagMapSource ITagMap.Source => Source;
    TagMapTarget ITagMap.Target => Target;

    public TagMapVectorSource Source { get; } = source;
    public TagMapVectorTarget Target { get; } = target;
}
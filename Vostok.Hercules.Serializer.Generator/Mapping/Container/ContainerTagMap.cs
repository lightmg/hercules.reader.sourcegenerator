using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Container;

public class ContainerTagMap(TagMapContainerSource source, TagMapTarget target)
    : ITagMap<TagMapContainerSource>
{
    ITagMapSource ITagMap.Source => Source;

    public TagMapContainerSource Source { get; } = source;
    public TagMapTarget Target { get; } = target;
}
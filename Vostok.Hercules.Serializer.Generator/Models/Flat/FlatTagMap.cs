using Vostok.Hercules.Serializer.Generator.Models.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Models.Flat;

public class FlatTagMap(TagMapFlatSource source, TagMapTarget target, TagMapConverter? converter)
    : ITagMap<TagMapFlatSource>
{
    ITagMapSource ITagMap.Source => Source;

    public TagMapFlatSource Source { get; } = source;
    public TagMapTarget Target { get; } = target;
    public TagMapConverter? Converter { get; } = converter;
}
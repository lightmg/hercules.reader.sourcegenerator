using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Flat;

public class FlatTagMap(TagMapFlatSource source, TagMapTarget target, TagMapConverter? converter)
    : IConvertibleTagMap<TagMapFlatSource>
{
    ITagMapSource ITagMap.Source => Source;

    public TagMapFlatSource Source { get; } = source;
    public TagMapTarget Target { get; } = target;
    public TagMapConverter? Converter { get; } = converter;
}
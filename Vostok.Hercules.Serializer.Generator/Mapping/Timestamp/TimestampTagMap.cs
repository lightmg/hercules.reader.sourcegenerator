using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Timestamp;

public class TimestampTagMap(TagMapTimestampSource source, TagMapTarget target, TagMapConverter? converter)
    : IConvertibleTagMap<TagMapTimestampSource>
{
    ITagMapSource ITagMap.Source => Source;

    public TagMapTimestampSource Source { get; } = source;
    public TagMapTarget Target { get; } = target;
    public TagMapConverter? Converter { get; } = converter;
}
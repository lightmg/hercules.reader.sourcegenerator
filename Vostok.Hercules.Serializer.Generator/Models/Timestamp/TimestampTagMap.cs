using Vostok.Hercules.Serializer.Generator.Models.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Models.Timestamp;

public class TimestampTagMap(TagMapTimestampSource source, TagMapTarget target, TagMapConverter? converter)
    : ITagMap<TagMapTimestampSource>
{
    ITagMapSource ITagMap.Source => Source;

    public TagMapTimestampSource Source { get; } = source;
    public TagMapTarget Target { get; } = target;
    public TagMapConverter? Converter { get; } = converter;
}
namespace Vostok.Hercules.Serializer.Generator.Models;

public class TagMap
{
    public readonly ITagMapSource Source;
    public readonly TagMapTarget Target;
    public readonly TagMapConverter? Converter;

    public TagMap(ITagMapSource source, TagMapTarget target, TagMapConverter converter)
    {
        Source = source;
        Target = target;
        Converter = converter;
    }
}
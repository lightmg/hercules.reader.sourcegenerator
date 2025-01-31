namespace Vostok.Hercules.Serializer.Generator.Models;

public class TagMap
{
    public readonly ITagMapSource Source;
    public readonly TagMapTarget Target;
    public readonly TagMapConfiguration Config;

    public TagMap(ITagMapSource source, TagMapTarget target, TagMapConfiguration config)
    {
        Source = source;
        Target = target;
        Config = config;
    }
}
namespace Vostok.Hercules.Serializer.Generator.Models;

public class TagMap
{
    public readonly TagMapSource Source;
    public readonly TagMapTarget Target;
    public readonly TagMapConfiguration Config;

    public TagMap(TagMapSource source, TagMapTarget target, TagMapConfiguration config)
    {
        Source = source;
        Target = target;
        Config = config;
    }
}
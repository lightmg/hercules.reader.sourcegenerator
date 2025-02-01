using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Container;

public class TagMapContainerSource(string key) : ITagMapSource
{
    public string Key { get; } = key;
}
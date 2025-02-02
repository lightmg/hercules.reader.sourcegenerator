using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Container;

internal class ContainerMapProvider : BaseMapProvider
{
    public static ContainerTagMap Create(TagMapTarget target, string tagKey)
    {
        // TODO respect converter
        return new ContainerTagMap(new(tagKey), target);
    }
}
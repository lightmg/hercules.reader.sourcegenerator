using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Models.Sources;

public interface ITagMapSource
{
    ReferencedType Type { get; }
}
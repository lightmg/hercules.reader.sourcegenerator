using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;
using Vostok.Hercules.Serializer.Generator.Services;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Flat;

internal class FlatMapProvider : BaseMapProvider
{
    public static FlatTagMap Create(TagMapTarget target, string tagKey, TagMapConverter? converter)
    {
        var sourceType = InferSourceType(converter, target.Type);
        var source = TypeUtilities.IsNullable(sourceType, out var underlyingType)
            ? new TagMapFlatSource(tagKey, TypeDescriptor.From(underlyingType))
            : new TagMapFlatSource(tagKey, TypeDescriptor.From(sourceType));

        return new FlatTagMap(source, target, converter);
    }
}
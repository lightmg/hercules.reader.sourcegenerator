using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;
using Vostok.Hercules.Serializer.Generator.Mapping.Flat;
using Vostok.Hercules.Serializer.Generator.Services;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Vector;

internal class VectorMapProvider : BaseMapProvider
{
    public static VectorTagMap Create(
        TagMapTarget target,
        string tagKey,
        TagMapConverter? converter,
        ITypeSymbol elementType,
        VectorType vectorType
    )
    {
        var sourceType = InferSourceType(converter, elementType);
        var source = TypeUtilities.IsNullable(sourceType, out var underlyingType)
            ? new TagMapVectorSource(tagKey, TypeDescriptor.From(underlyingType))
            : new TagMapVectorSource(tagKey, TypeDescriptor.From(sourceType));

        var vectorTarget = new TagMapVectorTarget(target, elementType, vectorType);
        return new VectorTagMap(source, vectorTarget, converter);
    }
}
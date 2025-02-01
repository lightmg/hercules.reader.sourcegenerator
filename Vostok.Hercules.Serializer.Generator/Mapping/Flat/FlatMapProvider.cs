using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Services;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Flat;

internal class BaseMapProvider
{
    protected static ITypeSymbol InferSourceType(TagMapConverter? conveter, ITypeSymbol targetType) =>
        conveter?.Method.Parameters[0].Type ?? targetType;
}

internal class FlatMapProvider : BaseMapProvider
{
    public static FlatTagMap Create(TagMapTarget target, string tagKey, TagMapConverter? converter)
    {
        var sourceType = InferSourceType(converter, target.Type);
        var source = TypeUtilities.IsNullable(sourceType, out var underlyingType)
            ? new TagMapFlatSource(tagKey, ReferencedType.From(underlyingType))
            : new TagMapFlatSource(tagKey, ReferencedType.From(sourceType));

        return new FlatTagMap(source, target, converter);
    }
}
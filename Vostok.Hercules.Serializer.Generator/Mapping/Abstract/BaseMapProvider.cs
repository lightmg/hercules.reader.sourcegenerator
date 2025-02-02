using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Abstract;

internal class BaseMapProvider
{
    protected static ITypeSymbol InferSourceType(TagMapConverter? conveter, ITypeSymbol targetType) =>
        conveter?.Method.Parameters[0].Type ?? targetType;
}
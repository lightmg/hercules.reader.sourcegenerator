using System;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;
using Vostok.Hercules.Serializer.Generator.Mapping.Container;
using Vostok.Hercules.Serializer.Generator.Mapping.Vector;
using Vostok.Hercules.Serializer.Generator.Services;

namespace Vostok.Hercules.Serializer.Generator.Mapping.VectorOfContainers;

internal class VectorOfContainersMapProvider : BaseMapProvider
{
    private static readonly TypeDescriptor SourceElementType = TypeDescriptor.From(
        TypeNames.Collections.IReadOnlyList(TypeNames.Action(TypeNames.HerculesClientAbstractions.ITagsBuilder))
    );

    public static VectorOfContainersTagMap Create(
        TagMapTarget target,
        string tagKey,
        ITypeSymbol elementType,
        VectorType vectorType
    )
    {
        return new VectorOfContainersTagMap(
            new TagMapVectorSource(tagKey, SourceElementType), 
            new TagMapVectorTarget(target, elementType, vectorType)
        );
    }
}
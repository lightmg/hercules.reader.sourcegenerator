using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Models.Vector;

public class TagMapVectorTarget(ISymbol symbol, ITypeSymbol elementType, VectorType vectorType) : TagMapTarget(symbol)
{
    public TagMapVectorTarget(TagMapTarget target, ITypeSymbol elementType, VectorType vectorType) 
        : this(target.Symbol, elementType, vectorType)
    {
    }

    public readonly ITypeSymbol ElementType = elementType;

    public readonly VectorType VectorType = vectorType;
}
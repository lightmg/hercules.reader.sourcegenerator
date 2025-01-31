using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types;

namespace Vostok.Hercules.Serializer.Generator.Services;

internal static class AttributeFinder
{
    public static bool FindAttribute(ISymbol symbol, AttributeTypeBuilder attribute, MappingGeneratorContext ctx) =>
        TryGetAttribute(symbol, attribute, ctx, out AttributeData _);

    public static bool TryGetAttributeArgs(ISymbol symbol, AttributeTypeBuilder attribute,
        MappingGeneratorContext ctx, out object?[] arguments)
    {
        if (!TryGetAttribute(symbol, attribute, ctx, out AttributeData attributeData))
        {
            arguments = [];
            return false;
        }

        arguments = attributeData.ConstructorArguments.Select(x => x.Value).ToArray();
        return true;
    }

    public static bool TryGetAttribute(ISymbol symbol, AttributeTypeBuilder attribute,
        MappingGeneratorContext ctx, out AttributeData result)
    {
        var matchedAttributes = symbol.GetAttributes(attribute).ToArray();

        if (matchedAttributes.Length == 0)
        {
            result = default!;
            return false;
        }

        if (matchedAttributes.Length > 1)
        {
            ctx.AddDiagnostic(
                DiagnosticDescriptors.ConflictinAnnotations,
                symbol,
                attribute.FullName,
                $"Expected annotation to appear only once, but {matchedAttributes.Length} found"
            );
            result = default!;
            return false;
        }

        result = matchedAttributes[0];
        return true;
    }
}
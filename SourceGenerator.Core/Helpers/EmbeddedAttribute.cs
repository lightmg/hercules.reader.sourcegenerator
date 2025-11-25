using Microsoft.CodeAnalysis;
using SourceGenerator.Core.Builders.Types;
using SourceGenerator.Core.Primitives;

namespace SourceGenerator.Core.Helpers;

public static class EmbeddedAttribute
{
    public static readonly ClassBuilder Builder =
        new AttributeTypeBuilder("Microsoft.CodeAnalysis", "EmbeddedAttribute")
        {
            Accessibility = Accessibility.Internal,
            Usage = AttributeTargets.All,
            Modifier = ClassModifier.Sealed
        };
}
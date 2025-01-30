using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Models;

public readonly struct HerculesSourceType
{
    public readonly ITypeSymbol Type;
    public readonly bool Optional;

    public HerculesSourceType(ITypeSymbol type, bool optional)
    {
        Type = type;
        Optional = optional;
    }
}
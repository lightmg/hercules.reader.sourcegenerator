using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Models;

public class TagMapSource(string key, ITypeSymbol type, bool optional)
{
    public readonly string Key = key;
    public readonly ITypeSymbol Type = type;
    public readonly bool Optional = optional;
}
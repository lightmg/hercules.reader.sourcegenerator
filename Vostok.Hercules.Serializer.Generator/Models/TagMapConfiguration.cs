using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Models;

public readonly record struct TagMapConfiguration
{
    public readonly IMethodSymbol? ConverterMethod;

    public TagMapConfiguration(IMethodSymbol converterMethod)
    {
        ConverterMethod = converterMethod;
    }

    public bool IsDefault => ConverterMethod is null;

    public void Deconstruct(out IMethodSymbol? converterMethod)
    {
        converterMethod = ConverterMethod;
    }
}
using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Models;

public readonly record struct TagMapConfiguration
{
    public readonly ITypeSymbol? ConverterType;
    public readonly IMethodSymbol? ConverterMethod;

    public TagMapConfiguration(ITypeSymbol converterType)
    {
        ConverterType = converterType;
        ConverterMethod = null;
    }

    public TagMapConfiguration(IMethodSymbol converterMethod)
    {
        ConverterType = null;
        ConverterMethod = converterMethod;
    }

    public bool IsDefault => ConverterType is null && ConverterMethod is null;

    public void Deconstruct(out ITypeSymbol? converterType, out IMethodSymbol? converterMethod)
    {
        converterType = ConverterType;
        converterMethod = ConverterMethod;
    }
}
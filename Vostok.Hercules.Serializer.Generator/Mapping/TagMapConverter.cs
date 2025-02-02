using System;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Mapping;

public readonly record struct TagMapConverter
{
    public readonly IMethodSymbol Method;

    public TagMapConverter(IMethodSymbol method)
    {
        if (method.Parameters.Length != 1 || method.ReturnType.SpecialType == SpecialType.System_Void)
            throw new ArgumentException("Incompatible converter method signature", nameof(method));
        Method = method;
    }

    public TypeDescriptor InType => TypeDescriptor.From(InTypeSymbol);

    public TypeDescriptor OutType  => TypeDescriptor.From(OutTypeSymbol);

    public ITypeSymbol InTypeSymbol => Method.Parameters[0].Type;

    public ITypeSymbol OutTypeSymbol  => Method.ReturnType;

    public void Deconstruct(out IMethodSymbol? converterMethod)
    {
        converterMethod = Method;
    }
}
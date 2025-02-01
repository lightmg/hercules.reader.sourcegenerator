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

    public ReferencedType InType => ReferencedType.From(Method.Parameters[0].Type);

    public ReferencedType OutType  => ReferencedType.From(Method.ReturnType);

    public void Deconstruct(out IMethodSymbol? converterMethod)
    {
        converterMethod = Method;
    }
}
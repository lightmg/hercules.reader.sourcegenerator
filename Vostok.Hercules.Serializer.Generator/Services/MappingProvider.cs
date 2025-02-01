using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Mapping;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;
using Vostok.Hercules.Serializer.Generator.Mapping.Container;
using Vostok.Hercules.Serializer.Generator.Mapping.Flat;
using Vostok.Hercules.Serializer.Generator.Mapping.Timestamp;
using Vostok.Hercules.Serializer.Generator.Mapping.Vector;

namespace Vostok.Hercules.Serializer.Generator.Services;

internal static class MappingProvider
{
    public static EventMapping CreateMapping(
        INamedTypeSymbol containingType,
        IEnumerable<ISymbol> members,
        MappingGeneratorContext ctx)
    {
        var mapping = new EventMapping(containingType);

        foreach (var member in members)
        {
            var map = CreateMapOrNull(member, ctx);
            if (map is null)
                continue;

            mapping.Entries.Add(map);
            ValidateTagMap(map, ctx);
        }

        ValidateMapping(mapping, ctx);
        return mapping;
    }

    private static void ValidateMapping(EventMapping mapping, MappingGeneratorContext ctx)
    {
        if (!TypeUtilities.HasParameterlessCtor(mapping.Type, c => c.DeclaredAccessibility >= Accessibility.Internal))
            ctx.AddDiagnostic(DiagnosticDescriptors.MissingParameterlessCtor, mapping.Type, mapping.Type.ToString());
    }

    private static void ValidateTagMap(ITagMap map, MappingGeneratorContext ctx)
    {
        switch (map)
        {
            case FlatTagMap flatMap:
                if (!TypeUtilities.IsHerculesPrimitive(flatMap.Source.Type))
                    ctx.AddDiagnostic(DiagnosticDescriptors.UnknownType, map.Target.Symbol, flatMap.Source.Type);
                break;
            case VectorTagMap flatMap:
                if (!TypeUtilities.IsHerculesPrimitive(flatMap.Source.ElementType))
                    ctx.AddDiagnostic(DiagnosticDescriptors.UnknownType, map.Target.Symbol, flatMap.Source.ElementType);
                break;
        }
    }

    private static ITagMap? CreateMapOrNull(ISymbol symbol, MappingGeneratorContext ctx)
    {
        var target = new TagMapTarget(symbol);
        var converter = CreateConverter(target, ctx);

        if (AttributeFinder.FindAttribute(symbol, ExposedApi.HerculesTimestampTagAttribute, ctx))
            return TimestampMapProvider.Create(target, converter, ctx);

        if (!AttributeFinder.TryGetAttributeArgs(symbol, ExposedApi.HerculesTagAttribute, ctx, out var args))
            return null;

        if (args.Length != 1 || args[0] is not string tagKey)
            return null;

        if (TypeUtilities.IsContainer(target.Type))
            return ContainerMapProvider.Create(target, tagKey);

        if (TypeUtilities.IsVector(target.Type, out var elementType, out var vectorType))
            return VectorMapProvider.Create(target, tagKey, converter, elementType, vectorType);

        return FlatMapProvider.Create(target, tagKey, converter);
    }

    private static TagMapConverter? CreateConverter(TagMapTarget target, MappingGeneratorContext ctx) =>
        AttributeFinder.TryGetAttribute(target.Symbol, ExposedApi.HerculesConverterAttribute, ctx, out var attribute) &&
        GetConverterInfo(attribute) is var (containingType, methodName) &&
        GetConvertMethod(target, containingType, methodName, ctx) is { } convertMethod
            ? new TagMapConverter(convertMethod)
            : null;

    private static (ITypeSymbol type, string name)? GetConverterInfo(AttributeData attribute) =>
        attribute.AttributeConstructor?.Parameters.Length switch
        {
            1 when attribute.AttributeConstructor.Parameters[0].Name == "converterType" &&
                   attribute.ConstructorArguments[0].Value is ITypeSymbol converterType =>
                (converterType, ExposedApi.IHerculesConverter.Methods.First().Name),
            2 when attribute.AttributeConstructor.Parameters[0].Name == "convertMethodContainingType" &&
                   attribute.AttributeConstructor.Parameters[1].Name == "convertMethodName" &&
                   attribute.ConstructorArguments[0].Value is ITypeSymbol containingType &&
                   attribute.ConstructorArguments[1].Value is string methodName =>
                (containingType, methodName),
            _ => null
        };

    private static IMethodSymbol? GetConvertMethod(
        TagMapTarget target,
        ITypeSymbol containingType,
        string name,
        MappingGeneratorContext ctx)
    {
        var matchingMethods = containingType
            .GetMembers(name)
            .OfType<IMethodSymbol>()
            .Where(m => m.Parameters.Length == 1 &&
                        m.TypeParameters.Length == 0 &&
                        m.DeclaredAccessibility >= Accessibility.Internal)
            .ToList();

        if (matchingMethods.Count == 1)
            return matchingMethods[0];

        if (matchingMethods.Count == 0)
        {
            ctx.AddDiagnostic(
                DiagnosticDescriptors.ConverterMethodNotFound,
                target.Symbol,
                name, containingType
            );

            return null;
        }

        var sameReturnTypeMethods = matchingMethods
            .Where(m => m.ReturnType.Equals(target.Type, SymbolEqualityComparer.IncludeNullability))
            .ToArray();

        if (sameReturnTypeMethods.Length == 1)
            return sameReturnTypeMethods[0];

        ctx.AddDiagnostic(
            DiagnosticDescriptors.ConverterMethodAmbigious,
            target.Symbol,
            name,
            containingType,
            string.Join(", ", matchingMethods.Select(m => m.ToDisplayString()))
        );

        return null;
    }
}
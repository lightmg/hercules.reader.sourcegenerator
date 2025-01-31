using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Models;
using Vostok.Hercules.Serializer.Generator.Models.Sources;

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
            ValidateTagMap(map, member, ctx);
        }

        ValidateMapping(mapping, ctx);
        return mapping;
    }

    private static void ValidateMapping(EventMapping mapping, MappingGeneratorContext ctx)
    {
        if (!TypeUtilities.HasParameterlessCtor(mapping.Type, c => c.DeclaredAccessibility >= Accessibility.Internal))
            ctx.AddDiagnostic(DiagnosticDescriptors.MissingParameterlessCtor, mapping.Type, mapping.Type.ToString());
    }

    private static void ValidateTagMap(TagMap map, ISymbol member, MappingGeneratorContext ctx)
    {
        if (map.Source is not TagMapFlatSource)
            return;

        if (!TypeUtilities.IsHerculesPrimitive(map.Source.Type))
            ctx.AddDiagnostic(DiagnosticDescriptors.UnknownType, member, map.Source.Type);
    }

    private static TagMap? CreateMapOrNull(ISymbol symbol, MappingGeneratorContext ctx)
    {
        var target = TagMapTarget.Create(symbol);
        var converter = CreateConverter(target, ctx);

        if (AttributeFinder.FindAttribute(symbol, ExposedApi.HerculesTimestampTagAttribute, ctx))
            return CreateTimestampMap(target, converter, ctx);

        if (AttributeFinder.TryGetAttributeArgs(symbol, ExposedApi.HerculesTagAttribute, ctx, out var args))
        {
            if (args.Length != 1 || args[0] is not string tagKey)
                return null;

            return TypeUtilities.IsVector(target.Type, out var elementType)
                ? CreateVectorMap(target, tagKey, converter, elementType)
                : CreateFlatMap(target, tagKey, converter);
        }

        return null;
    }

    private static TagMap CreateVectorMap(
        TagMapTarget target,
        string tagKey,
        TagMapConverter? converter,
        ITypeSymbol elementType
    )
    {
        var sourceType = InferSourceType(converter, elementType);
        var source = TypeUtilities.IsNullable(sourceType, out var underlyingType)
            ? new TagMapVectorSource(tagKey, ReferencedType.From(target.Type), ReferencedType.From(underlyingType))
            : new TagMapVectorSource(tagKey, ReferencedType.From(target.Type), ReferencedType.From(sourceType));

        return new TagMap(source, target, converter);
    }

    private static TagMap CreateFlatMap(TagMapTarget target, string tagKey, TagMapConverter? converter)
    {
        var sourceType = InferSourceType(converter, target.Type);
        var source = TypeUtilities.IsNullable(sourceType, out var underlyingType)
            ? new TagMapFlatSource(tagKey, ReferencedType.From(underlyingType))
            : new TagMapFlatSource(tagKey, ReferencedType.From(sourceType));

        return new TagMap(source, target, converter);
    }

    private static TagMap CreateTimestampMap(TagMapTarget target, TagMapConverter? converter,
        MappingGeneratorContext ctx)
    {
        if (converter.HasValue && converter.Value.InType != typeof(DateTimeOffset))
            ctx.AddDiagnostic(DiagnosticDescriptors.InvalidTimestampTagType, target.Symbol,
                typeof(DateTimeOffset), converter.Value.InType
            );

        var source = new TagMapTimestampSource();
        return new TagMap(source, target, converter);
    }

    private static TagMapConverter? CreateConverter(TagMapTarget target, MappingGeneratorContext ctx) =>
        AttributeFinder.TryGetAttribute(target.Symbol, ExposedApi.HerculesConverterAttribute, ctx, out var attribute) &&
        GetConverterInfo(attribute) is var (containingType, methodName) &&
        GetConvertMethod(target, containingType, methodName, ctx) is { } convertMethod
            ? new TagMapConverter(convertMethod)
            : null;

    private static ITypeSymbol InferSourceType(TagMapConverter? conveter, ITypeSymbol targetType) =>
        conveter?.Method.Parameters[0].Type ?? targetType;

    private static (ITypeSymbol type, string name)? GetConverterInfo(AttributeData attribute) =>
        attribute.AttributeConstructor?.Parameters.Length switch
        {
            1 when attribute.AttributeConstructor.Parameters[0].Name == "converterType" &&
                   attribute.ConstructorArguments[0].Value is ITypeSymbol converterType =>
                (converterType, ExposedApi.HerculesConverterType.Methods.First().Name),
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
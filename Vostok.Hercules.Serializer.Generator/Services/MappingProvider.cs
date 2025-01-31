using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Extensions;
using Vostok.Hercules.Serializer.Generator.Models;

namespace Vostok.Hercules.Serializer.Generator.Services;

internal static class MappingProvider
{
    public static EventMapping CreateMapping(
        INamedTypeSymbol containingType,
        IEnumerable<ISymbol> members,
        MappingGeneratorContext ctx)
    {
        var result = new EventMapping(containingType);
        // TODO ensure parameterless ctor

        foreach (var member in members)
        {
            if (!TryCreateMap(member, ctx, out var map))
                continue;

            result.Entries.Add(map);

            if (!TypeUtilities.IsHerculesPrimitive(map.Source.Type))
                ctx.AddDiagnostic(DiagnosticDescriptors.UnknownType, member, map.Source.Type);
        }

        return result;
    }

    private static bool TryCreateMap(ISymbol memberSymbol, MappingGeneratorContext ctx, out TagMap tagMap)
    {
        var matchedAttributes = memberSymbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToString() == ExposedApi.HerculesTagAttribute.FullName)
            .ToArray();

        if (matchedAttributes.Length != 1)
        {
            ctx.AddDiagnostic(DiagnosticDescriptors.DuplicatedAnnotation,
                memberSymbol,
                ExposedApi.HerculesTagAttribute.FullName
            );

            tagMap = null!;
            return false;
        }

        var tagKey = matchedAttributes[0].ConstructorArguments[0].Value!.ToString();

        var target = TagMapTarget.Create(memberSymbol);
        var config = CreateMapConfiguration(target, ctx);
        var source = CreateSource(config, target, tagKey);

        tagMap = new TagMap(source, target, config);
        return true;
    }

    private static TagMapSource CreateSource(TagMapConfiguration config, TagMapTarget target, string tagKey)
    {
        var sourceType = InferSourceType(config, target.Type);
        return TypeUtilities.IsNullable(sourceType, out var underlyingType)
            ? new TagMapSource(tagKey, underlyingType, true)
            : new TagMapSource(tagKey, sourceType, false);
    }

    private static TagMapConfiguration CreateMapConfiguration(TagMapTarget target, MappingGeneratorContext ctx)
    {
        var matchedAttributes = target.Symbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToString() == ExposedApi.HerculesConverterAttribute.FullName)
            .ToArray();

        if (matchedAttributes.Length == 0)
            return default;

        if (matchedAttributes.Length > 1)
        {
            ctx.AddDiagnostic(
                DiagnosticDescriptors.DuplicatedAnnotation,
                target.Symbol,
                ExposedApi.HerculesConverterAttribute.FullName
            );
            return default;
        }

        if (GetConverterInfo(matchedAttributes[0]) is var (containingType, methodName) &&
            GetConvertMethod(target, containingType, methodName, ctx) is {} convertMethod)
            return new TagMapConfiguration(convertMethod);

        return default;
    }

    private static ITypeSymbol InferSourceType(TagMapConfiguration mapConfig, ITypeSymbol targetType) =>
        mapConfig.ConverterMethod != null ? mapConfig.ConverterMethod.Parameters[0].Type : targetType;

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
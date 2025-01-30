using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Extensions;
using Vostok.Hercules.Serializer.Generator.Models;

namespace Vostok.Hercules.Serializer.Generator.Services;

public static class MappingProvider
{
    public static CreateMappingResult CreateMapping(
        INamedTypeSymbol containingType,
        IEnumerable<ISymbol> members)
    {
        var result = new CreateMappingResult(new EventMapping(containingType));
        foreach (var member in members)
        {
            // todo add type validations here
            if (!TryCreateMap(member, result.Diagnostics, out var map))
                continue;

            result.Mapping.Entries.Add(map);

            if (!TypeUtilities.IsHerculesPrimitive(map.Source.Type))
                AddDiagnostic(result.Diagnostics, DiagnosticDescriptors.UnknownType, member, map.Source.Type);
        }

        return result;
    }

    private static bool TryCreateMap(ISymbol memberSymbol, IList<Diagnostic> diags, out TagMap tagMap)
    {
        var matchedAttributes = memberSymbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToString() == ExposedApi.HerculesTagAttribute.FullName)
            .ToArray();

        if (matchedAttributes.Length != 1)
        {
            AddDiagnostic(diags,
                DiagnosticDescriptors.DuplicatedAnnotation,
                memberSymbol,
                ExposedApi.HerculesTagAttribute.FullName
            );

            tagMap = null!;
            return false;
        }

        var tagKey = matchedAttributes[0].ConstructorArguments[0].Value!.ToString();

        var target = TagMapTarget.Create(memberSymbol);
        var config = CreateMapConfiguration(target, diags);
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

    private static TagMapConfiguration CreateMapConfiguration(TagMapTarget target, IList<Diagnostic> diags)
    {
        var matchedAttributes = target.Symbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToString() == ExposedApi.HerculesConverterAttribute.FullName)
            .ToArray();

        if (matchedAttributes.Length == 0)
            return default;

        if (matchedAttributes.Length > 1)
        {
            AddDiagnostic(diags,
                DiagnosticDescriptors.DuplicatedAnnotation,
                target.Symbol,
                ExposedApi.HerculesConverterAttribute.FullName
            );
            return default;
        }

        var attribute = matchedAttributes[0];
        if (attribute.AttributeConstructor?.Parameters.Length == 1 &&
            attribute.AttributeConstructor.Parameters[0].Name == "converterType" &&
            attribute.ConstructorArguments[0].Value is ITypeSymbol converterType)
            return new TagMapConfiguration(converterType);

        if (attribute.AttributeConstructor?.Parameters.Length == 2 &&
            attribute.AttributeConstructor.Parameters[0].Name == "convertMethodContainingType" &&
            attribute.AttributeConstructor.Parameters[1].Name == "convertMethodName" &&
            attribute.ConstructorArguments[0].Value is ITypeSymbol containingType &&
            attribute.ConstructorArguments[1].Value is string methodName)
        {
            var convertMethod = GetConvertMethodOrNull(target, containingType, methodName, out var diag);
            if (diag != null) 
                diags.Add(diag);

            if (convertMethod != null) 
                return new TagMapConfiguration(convertMethod);
        }

        return default;
    }

    private static ITypeSymbol InferSourceType(TagMapConfiguration mapConfig, ITypeSymbol targetType) =>
        mapConfig switch
        {
            (var converterType, null) => targetType, // TODO implement inferring
            (null, var converterMethod) => converterMethod.Parameters[0].Type,
            _ => targetType
        };

    private static IMethodSymbol? GetConvertMethodOrNull(
        TagMapTarget target,
        ITypeSymbol containingType,
        string name,
        out Diagnostic? diagnostic)
    {
        var matchingMethods = containingType
            .GetMembers(name)
            .OfType<IMethodSymbol>()
            .Where(m => m.Parameters.Length == 1 &&
                        m.TypeParameters.Length == 0 &&
                        m.DeclaredAccessibility >= Accessibility.Internal)
            .ToList();

        if (matchingMethods.Count == 0)
        {
            diagnostic = CreateDiagnostic(
                DiagnosticDescriptors.ConverterMethodNotFound,
                target.Symbol,
                name, containingType
            );

            return null;
        }

        if (matchingMethods.Count > 1)
        {
            diagnostic = CreateDiagnostic(
                DiagnosticDescriptors.ConverterMethodAmbigious,
                target.Symbol,
                name,
                containingType,
                string.Join(", ", matchingMethods.Select(m => m.ToDisplayString()))
            );

            return null;
        }

        var convertMethod = matchingMethods[0];
        if (!convertMethod.IsStatic)
        {
            diagnostic = CreateDiagnostic(
                DiagnosticDescriptors.ConverterMethodShouldBeStatic,
                target.Symbol,
                name, containingType
            );
        }

        diagnostic = null;
        return convertMethod;
    }


    private static void AddDiagnostic(IList<Diagnostic> diags, DiagnosticDescriptor descriptor,
        ISymbol member, params object[] messageArgs) =>
        diags.Add(CreateDiagnostic(descriptor, member, messageArgs));

    private static Diagnostic CreateDiagnostic(DiagnosticDescriptor descriptor, ISymbol member,
        params object[] messageArgs) =>
        Diagnostic.Create(descriptor,
            member.Locations.FirstOrDefault(),
            additionalLocations: member.Locations.Skip(1),
            messageArgs: messageArgs
        );
}

public readonly struct CreateMappingResult(EventMapping mapping) : IEquatable<CreateMappingResult>
{
    public readonly EventMapping Mapping = mapping;

    public readonly IList<Diagnostic> Diagnostics = [];

    public bool Success => Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);

    public bool Equals(CreateMappingResult other) =>
        Mapping.Equals(other.Mapping) &&
        Diagnostics.Intersect(other.Diagnostics).Count() == Diagnostics.Count;

    public override bool Equals(object? obj) =>
        obj is CreateMappingResult other && Equals(other);

    public override int GetHashCode() =>
        unchecked((Mapping.GetHashCode() * 397) ^ Diagnostics.GetElementsHashCode());
}
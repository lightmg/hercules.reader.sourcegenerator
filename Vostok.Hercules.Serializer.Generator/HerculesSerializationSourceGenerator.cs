using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Vostok.Hercules.Serializer.Generator.Extensions;
using Vostok.Hercules.Serializer.Generator.Models;
using Vostok.Hercules.Serializer.Generator.Services;

namespace Vostok.Hercules.Serializer.Generator;

[Generator]
public class HerculesSerializationSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initCtx)
    {
        initCtx.RegisterPostInitializationOutput(ctx =>
        {
            // todo embeddedattribute
            ctx.AddTypeSources(ExposedApi.All);
        });

        var mappingProvider = initCtx.SyntaxProvider
            .ForAttributeWithMetadataName(
                ExposedApi.HerculesTagAttribute.FullName,
                predicate: (node, _) => node is PropertyDeclarationSyntax or FieldDeclarationSyntax,
                transform: GetPropertyOrFieldInfo
            )
            .Where(m => m.containingType.GetAttributes()
                .Select(attr => attr.AttributeClass?.ToString())
                .Contains(ExposedApi.GenerateHerculesReaderAttribute.FullName)
            )
            .Collect()
            .SelectMany((x, _) => x.GroupBy(
                p => p.containingType,
                p => p.member,
                (type, member) => (type: (INamedTypeSymbol)type!, member),
                SymbolEqualityComparer.Default
            ))
            .Select((x, _) => MappingProvider.CreateMapping(x.type, x.member));

        initCtx.RegisterSourceOutput(mappingProvider, Generate);
    }

    private static void Generate(SourceProductionContext context, CreateMappingResult result)
    {
        foreach (var diagnostic in result.Diagnostics)
            context.ReportDiagnostic(diagnostic);

        context.AddTypeSource(HerculesConverterEmitter.CreateType(result.Mapping));
    }

    private static (ISymbol member, INamedTypeSymbol containingType) GetPropertyOrFieldInfo(
        GeneratorAttributeSyntaxContext ctx, CancellationToken token)
    {
        switch (ctx.TargetSymbol)
        {
            case IPropertySymbol propertyDeclaration:
            {
                var containingType = propertyDeclaration.ContainingType;
                return (propertyDeclaration, containingType);
            }
            case IFieldSymbol fieldDeclaration:
            {
                var containingType = fieldDeclaration.ContainingType;
                return (fieldDeclaration, containingType);
            }
            default:
                throw new InvalidOperationException($"Unexpected target symbol type: {ctx.TargetSymbol}");
        }
    }
}
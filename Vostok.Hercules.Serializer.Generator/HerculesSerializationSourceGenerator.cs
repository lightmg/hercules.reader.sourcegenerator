using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Vostok.Hercules.Serializer.Generator.Extensions;
using Vostok.Hercules.Serializer.Generator.Mapping;
using Vostok.Hercules.Serializer.Generator.Services;

namespace Vostok.Hercules.Serializer.Generator;

[Generator]
public class HerculesSerializationSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initCtx)
    {
        initCtx.RegisterPostInitializationOutput(ctx => { ctx.AddTypeSources(ExposedApi.All); });

        var iHerculesTagsBuilderProvider = initCtx.CompilationProvider
            .Select((c, _) => c.GetTypeByMetadataName(TypeNames.HerculesClientAbstractions.ITagsBuilder));

        initCtx.RegisterSourceOutput(iHerculesTagsBuilderProvider,
            (ctx, interfaceType) =>
            {
                if (interfaceType != null)
                    ctx.AddTypeSource(HerculesProxyTagsBuilderEmitter.CreateProxy(interfaceType));
            }
        );

        var mappingProvider = initCtx.SyntaxProvider
            .ForAttributeWithMetadataName(
                ExposedApi.HerculesTagAttribute.FullName,
                predicate: (node, _) => node is PropertyDeclarationSyntax or FieldDeclarationSyntax,
                transform: GetPropertyOrFieldInfo
            )
            .Collect()
            .Combine(initCtx.SyntaxProvider
                .ForAttributeWithMetadataName(
                    ExposedApi.HerculesTimestampTagAttribute.FullName,
                    predicate: (node, _) => node is PropertyDeclarationSyntax or FieldDeclarationSyntax,
                    transform: GetPropertyOrFieldInfo
                )
                .Collect())
            .SelectMany((x, _) => x.Left.Concat(x.Right))
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
            .Select((x, _) =>
            {
                var ctx = new MappingGeneratorContext();
                EventMapping? eventMapping;
                try
                {
                    eventMapping = MappingProvider.CreateMapping(x.type, x.member, ctx);
                }
                catch (Exception e)
                {
                    ctx.AddDiagnostic(DiagnosticDescriptors.UnexpectedError, x.type, e);
                    eventMapping = null;
                }

                return (ctx.Diagnostics, Mapping: eventMapping);
            });

        initCtx.RegisterSourceOutput(mappingProvider, static (ctx, result) =>
            Generate(ctx, result.Mapping, result.Diagnostics)
        );
    }

    private static void Generate(SourceProductionContext context,
        EventMapping? mapping,
        IEnumerable<Diagnostic> diagnostics
    )
    {
        foreach (var diagnostic in diagnostics)
            context.ReportDiagnostic(diagnostic);

        if (mapping != null)
            context.AddTypeSource(HerculesConverterEmitter.CreateType(mapping));
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
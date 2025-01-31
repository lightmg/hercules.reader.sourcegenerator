using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;
using Vostok.Hercules.Serializer.Generator.Core.Writer;
using Vostok.Hercules.Serializer.Generator.Core.Writer.Extensions;

namespace Vostok.Hercules.Serializer.Generator.Extensions;

internal static class GeneratorContextExtensiosn
{
    public static void AddTypeSources(this IncrementalGeneratorPostInitializationContext ctx,
        IEnumerable<ITypeBuilder> types)
    {
        foreach (var type in types)
            ctx.AddTypeSource(type);
    }

    public static void AddTypeSource(this IncrementalGeneratorPostInitializationContext ctx, ITypeBuilder type)
    {
        ctx.AddSource(
            $"{type.Name}.g.cs", 
            CodeWriter.CreateUtf8SourceText(type, static (type, w) => w.WriteType(type))
        );
    }

    public static void AddTypeSource(this SourceProductionContext ctx, ITypeBuilder type)
    {
        ctx.AddSource(
            $"{type.Name}.g.cs", 
            CodeWriter.CreateUtf8SourceText(type, static (type, w) => w.WriteType(type))
        );
    }
}
using System;
using System.Linq;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members.Abstract;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;
using Vostok.Hercules.Serializer.Generator.Core.Helpers;
using Vostok.Hercules.Serializer.Generator.Core.Writer;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations.Extensions;

public static class TypeBuilderExtensions
{
    public static TBuilder AppendEmitBody<TBuilder>(this TBuilder builder, Action<CodeWriter> emitBody)
        where TBuilder : IMethodBodyBuilder
    {
        builder.EmitBody = builder.EmitBody is null
            ? emitBody
            : builder.EmitBody + emitBody;

        return builder;
    }

    public static TBuilder PrependEmitBody<TBuilder>(this TBuilder builder, Action<CodeWriter> emitBody)
        where TBuilder : IMethodBodyBuilder
    {
        builder.EmitBody = builder.EmitBody is null
            ? emitBody
            : emitBody + builder.EmitBody;

        return builder;
    }

    public static TBuilder AddPropertiesCtorInit<TBuilder>(this TBuilder typeBuilder,
        Func<PropertyBuilder, bool> propertySelector,
        Func<ConstructorBuilder, bool>? ctorSelector = null
    ) where TBuilder : StatefulTypeBuilder, IInitializabeTypeBuilder
    {
        var constructors = ctorSelector is null
            ? typeBuilder.Constructors
            : typeBuilder.Constructors.Where(ctorSelector);

        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var property in typeBuilder.Properties.Where(propertySelector))
        foreach (var ctor in constructors)
        {
            var parameter = new ParameterBuilder(TextCaseConverter.ToLowerCamelCase(property.Name), property.Type);
            ctor.Parameters.Add(parameter);
            ctor.AppendEmitBody(w => w.AppendLine($"this.{property.Name} = {parameter.Name};"));
        }

        return typeBuilder;
    }

    public static TBuilder AddConstructor<TBuilder>(this TBuilder typeBuilder,
        Action<ConstructorBuilder>? transform = null
    ) where TBuilder : StatefulTypeBuilder, IInitializabeTypeBuilder
    {
        var ctor = new ConstructorBuilder(typeBuilder.Name);
        transform?.Invoke(ctor);
        typeBuilder.Constructors.Add(ctor);
        return typeBuilder;
    }
}
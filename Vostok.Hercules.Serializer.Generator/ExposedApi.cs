using System;
using System.Linq;
using System.Reflection;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations.Extensions;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator;

internal static class ExposedApi
{
    /* TODO converter injection
     * 
     * TODO add IHerculesSerializer
     * 
     * TODO add DI for passed converter
     *      maybe doing so should be performed via HerculesDeserializerContext which would accept all the dependencies
     *
     * Some future ideas:
     *   - specify converter type via generic attribute
     */

    private const string Namespace = "Vostok.Hercules.Serializer.Generator";

    public static readonly AttributeTypeBuilder GenerateHerculesReaderAttribute =
        new AttributeTypeBuilder(Namespace, nameof(GenerateHerculesReaderAttribute))
        {
            Usage = AttributeTargets.Class
        };

    public static readonly AttributeTypeBuilder HerculesConverterAttribute =
        new AttributeTypeBuilder(Namespace, nameof(HerculesConverterAttribute))
            {
                Usage = AttributeTargets.Property | AttributeTargets.Field
            }
            // .WithConstructor(ctor => ctor.Parameters.Add(new("converterType", typeof(Type)))) 
            .WithConstructor(ctor =>
            {
                ctor.Parameters.Add(new("convertMethodContainingType", typeof(Type)));
                ctor.Parameters.Add(new("convertMethodName", typeof(string)));
            });

    public static readonly AttributeTypeBuilder HerculesTagAttribute =
        new AttributeTypeBuilder(Namespace, nameof(HerculesTagAttribute))
        {
            Usage = AttributeTargets.Property | AttributeTargets.Field
        }.WithConstructor(ctor => ctor.Parameters.Add(new("key", typeof(string))));

    // public static readonly TypeBuilder HerculesConverterType = new TypeBuilder(Namespace, "IHerculesConverter")
    // {
    //     Kind = TypeKind.Interface,
    //     Generics = { "TValue", "THerculesValue" },
    //     Methods =
    //     {
    //         new("Deserialize")
    //         {
    //             ReturnType = "TValue",
    //             Parameters = { new("value", "THerculesValue") }
    //         }
    //     }
    // };

    internal static readonly TypeBuilder[] All = typeof(ExposedApi)
        .GetMembers(BindingFlags.Public | BindingFlags.Static)
        .Select(m => m switch
        {
            PropertyInfo p => p.GetValue(null),
            FieldInfo f => f.GetValue(null),
            _ => null
        })
        .OfType<TypeBuilder>()
        .ToArray();
}
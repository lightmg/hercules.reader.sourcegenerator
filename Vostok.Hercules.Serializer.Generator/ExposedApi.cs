using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations.Extensions;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator;

[SuppressMessage("ReSharper", "UnusedMember.Global")] // used implicitly via 'All' property
internal static class ExposedApi
{
    /* 
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
            .AddConstructor(ctor => ctor.Parameters.Add(new("converterType", typeof(Type)))) 
            .AddConstructor(ctor =>
            {
                ctor.Parameters.Add(new("convertMethodContainingType", typeof(Type)));
                ctor.Parameters.Add(new("convertMethodName", typeof(string)));
            });

    public static readonly AttributeTypeBuilder HerculesTagAttribute =
        new AttributeTypeBuilder(Namespace, nameof(HerculesTagAttribute))
        {
            Usage = AttributeTargets.Property | AttributeTargets.Field
        }.AddConstructor(ctor => ctor.Parameters.Add(new("key", typeof(string))));
    
    public static readonly TypeBuilder HerculesConverterType = new TypeBuilder(Namespace, "IHerculesConverter")
    {
        Kind = TypeKind.Interface,
        Generics = { "TValue", "THerculesValue" },
        Methods =
        {
            new("Deserialize")
            {
                ReturnType = "TValue",
                Parameters = { new("value", "THerculesValue") }
            }
        }
    };

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
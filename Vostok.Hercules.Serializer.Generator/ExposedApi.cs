using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations.Extensions;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;
using Vostok.Hercules.Serializer.Generator.Services;

namespace Vostok.Hercules.Serializer.Generator;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")] // used implicitly via 'All' property
internal static class ExposedApi
{
    /*
     * Some future ideas:
     *   - specify converter type via generic attribute
     */

    public const string Namespace = "Vostok.Hercules.Serializer.Generator";

    public static readonly AttributeTypeBuilder GenerateHerculesReaderAttribute =
        new AttributeTypeBuilder(Namespace, nameof(GenerateHerculesReaderAttribute))
        {
            Accessibility = Accessibility.Internal,
            Usage = AttributeTargets.Class
        };

    public static readonly AttributeTypeBuilder HerculesConverterAttribute =
        new AttributeTypeBuilder(Namespace, nameof(HerculesConverterAttribute))
            {
                Accessibility = Accessibility.Internal,
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
                Accessibility = Accessibility.Internal,
                Usage = AttributeTargets.Property | AttributeTargets.Field
            }
            .AddConstructor(ctor => ctor.Parameters.Add(new("key", typeof(string))));

    public static readonly AttributeTypeBuilder HerculesTimestampTagAttribute =
        new AttributeTypeBuilder(Namespace, nameof(HerculesTimestampTagAttribute))
        {
            Accessibility = Accessibility.Internal,
            Usage = AttributeTargets.Property | AttributeTargets.Field
        };

    public static readonly InterfaceBuilder IHerculesConverter =
        new InterfaceBuilder(Namespace, nameof(IHerculesConverter))
        {
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

    public static readonly ClassBuilder ValidationExceptionType =
        new ClassBuilder(Namespace, "HerculesValidationException", baseType: typeof(Exception))
            {
                Properties =
                {
                    new("PropertyName", typeof(string)) { ReadOnly = true }
                }
            }
            .AddConstructor(cb => cb.BaseCtorArgs.Add(
                "message",
                "$\"Nullability missmatch: property or field '{propertyName}' is null after reading all the tags.\""
            ))
            .AddPropertiesCtorInit(p => p.Name is "PropertyName");

    public static readonly ClassBuilder EmbeddedAttribute =
        new AttributeTypeBuilder("Microsoft.CodeAnalysis", "EmbeddedAttribute")
        {
            Accessibility = Accessibility.Internal,
            Usage = AttributeTargets.Class
        };

    public static readonly InterfaceBuilder IHerculesEventBuilderProvider =
        new InterfaceBuilder(Namespace, nameof(IHerculesEventBuilderProvider))
        {
            Accessibility = Accessibility.Internal,
            Generics = { new("THerculesEvent") {Variance = VarianceKind.Out} },
            Methods =
            {
                new MethodBuilder("Get")
                {
                    ReturnType = TypeNames.HerculesClientAbstractions.EventBuilderInterfaceType("THerculesEvent")
                }
            }
        };

    internal static readonly ITypeBuilder[] All = typeof(ExposedApi)
        .GetMembers(BindingFlags.Public | BindingFlags.Static)
        .Select(m => m switch
        {
            PropertyInfo p => p.GetValue(null),
            FieldInfo f => f.GetValue(null),
            _ => null
        })
        .OfType<ITypeBuilder>()
        .ToArray();
}
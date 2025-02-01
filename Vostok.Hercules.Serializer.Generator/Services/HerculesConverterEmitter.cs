using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations.Extensions;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types;
using Vostok.Hercules.Serializer.Generator.Core.Helpers;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Core.Writer;
using Vostok.Hercules.Serializer.Generator.Core.Writer.Extensions;
using Vostok.Hercules.Serializer.Generator.Extensions;
using Vostok.Hercules.Serializer.Generator.Mapping;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;
using Vostok.Hercules.Serializer.Generator.Mapping.Container;
using Vostok.Hercules.Serializer.Generator.Mapping.Flat;
using Vostok.Hercules.Serializer.Generator.Mapping.Timestamp;
using Vostok.Hercules.Serializer.Generator.Mapping.Vector;

namespace Vostok.Hercules.Serializer.Generator.Services;

public static class HerculesConverterEmitter
{
    private const string Namespace = "Vostok.Hercules.Client.Abstractions.Events";
    private const string DummyBuilderType = $"{Namespace}.DummyHerculesTagsBuilder";
    public const string TagsBuilderInterfaceType = $"{Namespace}.IHerculesTagsBuilder";

    public static string EventBuilderInterfaceType(string type) =>
        $"{Namespace}.IHerculesEventBuilder<{type}>";

    public static string IReadOnlyListType(string type) =>
        typeof(IReadOnlyList<>).Namespace + $".IReadOnlyList<{type}>";

    public static ClassBuilder CreateType(EventMapping eventMap)
    {
        var targetTypeFullName = eventMap.Type.ToString();
        var builder = new ClassBuilder(
            ns: eventMap.Type.ContainingNamespace.ToString(),
            name: eventMap.Type.Name + "Builder",
            baseType: DummyBuilderType
        )
        {
            Accessibility = Accessibility.Internal,
            Interfaces = { EventBuilderInterfaceType(targetTypeFullName) },
            Properties = { PropertyBuilder.ReadOnlyField("Current", targetTypeFullName, Accessibility.Public) }
        };

        AddDependencies(builder, eventMap.Entries.OfType<ContainerTagMap>()
            .Select(c => (
                GetBuilderProviderFieldName(c),
                ReferencedType.From(ExposedApi.IHerculesEventBuilderProvider.GetFullName(c.Target.Type.ToString()))
            ))
        );

        AddDependencies(builder, eventMap.Entries.OfType<IConvertibleTagMap>()
            .Select(e => e.Converter)
            .Where(c => c?.Method is { IsStatic: false })
            .Select(c => (
                GetConverterFieldName(c)!,
                ReferencedType.From(c!.Value.Method.ContainingType)
            ))
        );

        builder.AddConstructor(ctor => ctor.AppendEmitBody(w => w
            .AppendPropertyAssignment("Current", $"new {targetTypeFullName}()")
        ));
        builder.AddPropertiesCtorInit(p => p.Name != "Current");

        builder.Methods.AddRange(CreateFlatMapMethods(eventMap));
        builder.Methods.AddRange(CreateVectorMapMethods(eventMap));
        builder.Methods.Add(CreateContainerMapMethods(eventMap));
        builder.Methods.Add(CreateBuildEventMethod(eventMap));
        builder.Methods.Add(CreateSetTimestampMethod(eventMap));

        return builder;
    }

    private static void AddDependencies(
        ClassBuilder builder,
        IEnumerable<(string fieldName, ReferencedType type)> services
    )
    {
        builder.Properties.AddRange(services
            .Select(s => new PropertyBuilder(s.fieldName, s.type)
            {
                Accessibility = Accessibility.Private,
                Kind = ParameterKind.Field,
                ReadOnly = true
            })
            .DistinctBy(x => x.Name)
        );
    }

    private static string GetBuilderProviderFieldName(ITagMap map) =>
        map.Target.Name + "BuilderProvider";

    private static IEnumerable<MethodBuilder> CreateFlatMapMethods(EventMapping eventMap) =>
        eventMap.Entries.OfType<FlatTagMap>()
            .GroupBy(x => x.Source.Type, (sourceType, entries) => (
                KeyType: sourceType,
                SameKeyEntries: entries.GroupBy(y => y.Source.Key)
            ))
            .Select(group => new MethodBuilder("AddValue")
                {
                    IsNew = true,
                    Accessibility = Accessibility.Public,
                    Parameters =
                    {
                        new("key", ReferencedType.From<string>()),
                        new("value", ReferencedType.From(group.KeyType.ToString()))
                    },
                    ReturnType = TagsBuilderInterfaceType,
                }
                .PrependEmitBody(w => WriteMapMethod(w, group.SameKeyEntries, static (e, w) => w
                    .WriteJoin(e, null, static (e, ew) => WriteFlatAssignment(ew, e))
                ))
            );

    private static IEnumerable<MethodBuilder> CreateVectorMapMethods(EventMapping eventMap) =>
        eventMap.Entries.OfType<VectorTagMap>()
            .GroupBy(x => x.Source.ElementType, (elementType, entries) => (
                ElementType: elementType,
                SameKeyEntries: entries.GroupBy(y => y.Source.Key)
            ))
            .Select(group => new MethodBuilder("AddValues")
                {
                    IsNew = true,
                    Accessibility = Accessibility.Public,
                    Parameters =
                    {
                        new("key", ReferencedType.From<string>()),
                        new("values", IReadOnlyListType(group.ElementType.FullName))
                    },
                    ReturnType = TagsBuilderInterfaceType,
                }
                .PrependEmitBody(w => WriteMapMethod(w, group.SameKeyEntries, WriteVectorAssignments))
            );

    private static MethodBuilder CreateContainerMapMethods(EventMapping eventMap) =>
        new MethodBuilder("AddContainer")
            {
                IsNew = true,
                Accessibility = Accessibility.Public,
                Parameters =
                {
                    new("key", ReferencedType.From<string>()),
                    new("valueBuilder", $"System.Action<{TagsBuilderInterfaceType}>")
                },
                ReturnType = TagsBuilderInterfaceType
            }
            .PrependEmitBody(writer => WriteMapMethod(writer,
                eventMap.Entries.OfType<ContainerTagMap>().GroupBy(x => x.Source.Key),
                WriteContainerAssignments
            ));

    private static MethodBuilder CreateBuildEventMethod(EventMapping mapping) =>
        new MethodBuilder("BuildEvent")
        {
            Accessibility = Accessibility.Public,
            ReturnType = mapping.Type.ToString(),
            EmitBody = writer => writer
                .WriteIfElseBlock(mapping.Entries.Where(e => !e.Target.IsNullable),
                    writeCondition: (tag, w) => w.AppendTargetReference(tag.Target).Append(" == null"),
                    writeBody: (tag, w) => w.WriteThrowHerculesValidationException(tag.Target.Name)
                )
                .AppendLine("return this.Current;")
        };

    private static MethodBuilder CreateSetTimestampMethod(EventMapping mapping) =>
        new MethodBuilder("SetTimestamp")
        {
            Accessibility = Accessibility.Public,
            ReturnType = EventBuilderInterfaceType(mapping.Type.ToString()),
            Parameters = { new ParameterBuilder("value", ReferencedType.From<DateTimeOffset>()) },
            EmitBody = writer => writer
                .WriteJoin(
                    mapping.Entries.OfType<TimestampTagMap>(),
                    "\n",
                    (map, w) => WriteTimestampAssignment(w, map)
                )
                .AppendLine("return this;")
        };

    private static CodeWriter WriteThrowHerculesValidationException(this CodeWriter writer, string propertyName) =>
        writer.AppendLine($"throw new {ExposedApi.ValidationExceptionType.FullName}(\"{propertyName}\");");

    private static CodeWriter WriteMapMethod<T>(CodeWriter writer,
        IEnumerable<IGrouping<string, T>> entriesByKey,
        Action<IGrouping<string, T>, CodeWriter> writeEntries
    ) where T : ITagMap =>
        writer
            .WriteIfElseBlock(entriesByKey, static (g, w) => w.Append($"key == \"{g.Key}\""), writeEntries)
            .AppendLine("return this;");

    private static string? GetConverterFieldName(TagMapConverter? converter) =>
        converter?.Method.IsStatic is false
            ? TextCaseConverter.ToLowerCamelCase(converter.Value.Method.ContainingType.Name)
            : null;

    private static void WriteFlatAssignment(CodeWriter writer, FlatTagMap map) =>
        writer.WriteTagTargetAssignment(map, static (map, w) => w.WriteValueWithConversion(map.Converter));

    private static void WriteTimestampAssignment(CodeWriter writer, TimestampTagMap map) =>
        writer.WriteTagTargetAssignment(map, static (map, w) => w.WriteValueWithConversion(map.Converter));

    private static void WriteTagTargetAssignment<T>(this CodeWriter writer, T map, Action<T, CodeWriter> writeValue)
        where T : ITagMap
    {
        writer.AppendTargetReference(map.Target).Append(" = ");
        writeValue(map, writer);
        writer.AppendLine(';');
    }

    private static CodeWriter AppendTargetReference(this CodeWriter writer, TagMapTarget target) =>
        writer.Append("this.Current.").Append(target.Name);

    private static void WriteContainerAssignments(IGrouping<string, ContainerTagMap> containers, CodeWriter writer)
    {
        writer.WriteJoin(containers, null, static (container, w) => w
            .WriteVariable(container, GetBuilderVarName(container), WriteBuilderInit)
        );

        if (containers.Count() == 1)
            writer.AppendLine($"valueBuilder({GetBuilderVarName(containers.Single())});");
        else
            writer.WriteVariable(containers, "builder", static (containers, w) => w.When(containers,
                    condition: static containers => containers.Count() == 1,
                    then: static (containers, w) => WriteBuilderInit(containers.Single(), w),
                    @else: static (containers, w) => w
                        .Append("new ").Append(HerculesProxyTagsBuilderEmitter.FullName)
                        .AppendLine($"(new {TagsBuilderInterfaceType}[] {{")
                        .WriteBlock(default, containers, static (containers, w) => w
                            .WriteJoin(containers, ",\n", static (map, w) => w.Append(GetBuilderVarName(map)))
                        )
                        .AppendLine().Append("})")
                ))
                .AppendLine("valueBuilder(builder);");

        writer.WriteJoin(containers, null, static (map, w) => w
            .WriteTagTargetAssignment(map, static (map, w) => w
                .Append(GetBuilderVarName(map)).Append(".BuildEvent()")
            )
        );

        return;

        static void WriteBuilderInit(ContainerTagMap map, CodeWriter writer) =>
            writer.Append("this.").Append(GetBuilderProviderFieldName(map)).Append(".Get()");

        static string GetBuilderVarName(ContainerTagMap map) =>
            TextCaseConverter.ToLowerCamelCase(map.Target.Name) + "Builder";
    }

    private static void WriteVectorAssignments(IGrouping<string, VectorTagMap> vectors, CodeWriter writer)
    {
        writer.WriteJoin(vectors, null, static (map, w) => w
                .WriteVariable(map,
                    writeName: static (map, w) => w.Append(GetVectorName(map)),
                    writeValue: static (map, w) => w
                        .When(map, IsArrayStyleInit, static (map, w) => w.Append(ArrayType(map)))
                        .When(map, IsListStyleInit, static (map, w) => w.Append(ListType(map)))
                        .When(map, IsHashSetStyleInit, static (map, w) => w.Append(HashSetType(map)))
                ))
            .When(vectors.Any(IsArrayStyleInit), w => w.WriteVariable("index", "0"))
            .WriteForeach(vectors, "value", "values", static (vectors, w) => w
                .WriteJoin(vectors, null, static (vector, w) => w
                    .When(vector, IsArrayStyleInit,
                        then: static (vector, w) => w
                            .Append(GetVectorName(vector))
                            .Append("[index] = ")
                            .WriteValueWithConversion(vector.Converter)
                            .AppendLine(";"),
                        @else: static (vector, w) => w
                            .Append(GetVectorName(vector))
                            .Append(".Add(")
                            .WriteValueWithConversion(vector.Converter)
                            .AppendLine(");")
                    )
                )
                .When(vectors.Any(IsArrayStyleInit), w => w.AppendLine("index++;"))
            )
            .WriteJoin(vectors, null, static (map, w) => w
                .WriteTagTargetAssignment(map, static (map, w) => w.Append(GetVectorName(map)))
            );

        return;

        static bool IsArrayStyleInit(VectorTagMap map) =>
            map.Target.VectorType is
                VectorType.Array or
                VectorType.IEnumerable or
                VectorType.IReadOnlyCollection or
                VectorType.IReadOnlyList;

        static string ArrayType(VectorTagMap map) =>
            $"new {map.Target.ElementType}[values.Count]";

        static bool IsListStyleInit(VectorTagMap map) =>
            map.Target.VectorType is
                VectorType.ICollection or
                VectorType.IList or
                VectorType.List;

        static string ListType(VectorTagMap map) =>
            $"new System.Collections.Generic.List<{map.Target.ElementType}>(values.Count)";

        static bool IsHashSetStyleInit(VectorTagMap map) =>
            map.Target.VectorType is
                VectorType.ISet or
                VectorType.IReadOnlySet or
                VectorType.HashSet;

        static string HashSetType(VectorTagMap map) =>
            $"new System.Collections.Generic.HashSet<{map.Target.ElementType}>()";

        static string GetVectorName(VectorTagMap map) =>
            TextCaseConverter.ToLowerCamelCase(map.Target.Name);
    }

    private static CodeWriter WriteCast(this CodeWriter writer, ReferencedType to) =>
        writer.Append("(").AppendType(to).Append(")");

    private static CodeWriter WriteValueWithConversion(this CodeWriter writer, TagMapConverter? converter) =>
        writer.WhenNotNull(converter,
            then: (c, w) => w.WriteConverterInvoction(c),
            @else: w => w.Append("value")
        );

    private static CodeWriter WriteConverterInvoction(this CodeWriter writer, TagMapConverter converter) =>
        writer
            .Append(GetConverterFieldName(converter) ?? converter.Method.ContainingType.ToString())
            .Append('.')
            .Append(converter.Method.Name)
            .Append("(value)");

    private static CodeWriter AppendPropertyAssignment(this CodeWriter writer, string propertyName, string value) =>
        writer.Append("this.").Append(propertyName).Append(" = ").Append(value).AppendLine(";");
}
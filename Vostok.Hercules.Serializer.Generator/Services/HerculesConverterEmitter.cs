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
using Vostok.Hercules.Serializer.Generator.Mapping.VectorOfContainers;

namespace Vostok.Hercules.Serializer.Generator.Services;

public static class HerculesConverterEmitter
{
    private const string ResultPropertyName = "Current";

    public static ClassBuilder CreateType(EventMapping eventMap)
    {
        var targetTypeFullName = eventMap.Type.ToString();
        var builder = new ClassBuilder(
            ns: eventMap.Type.ContainingNamespace.ToString(),
            name: eventMap.Type.Name + "Builder",
            baseType: TypeNames.HerculesClientAbstractions.DummyBuilderType
        )
        {
            Accessibility = Accessibility.Internal,
            Interfaces = { TypeNames.HerculesClientAbstractions.EventBuilderInterfaceType(targetTypeFullName) },
            Properties = { PropertyBuilder.ReadOnlyField(ResultPropertyName, targetTypeFullName, Accessibility.Public) }
        };

        AddDependencies(builder, eventMap.Entries.OfType<ContainerTagMap>()
            .Select(c => (
                GetBuilderProviderFieldName(c),
                TypeDescriptor.From(ExposedApi.IHerculesEventBuilderProvider.GetFullName(c.Target.Type.ToString()))
            ))
        );
        AddDependencies(builder, eventMap.Entries.OfType<VectorOfContainersTagMap>()
            .Select(c => (
                GetBuilderProviderFieldName(c),
                TypeDescriptor.From(ExposedApi.IHerculesEventBuilderProvider.GetFullName(c.Target.ElementType.ToString()))
            ))
        );

        AddDependencies(builder, eventMap.Entries.OfType<IConvertibleTagMap>()
            .Select(e => e.Converter)
            .Where(c => c?.Method is { IsStatic: false })
            .Select(c => (
                GetConverterFieldName(c)!,
                TypeDescriptor.From(c!.Value.Method.ContainingType)
            ))
        );

        builder.AddConstructor(ctor => ctor.AppendEmitBody(w => w
                .AppendPropertyAssignment(ResultPropertyName, $"new {targetTypeFullName}()")
            ))
            .AddPropertiesCtorInit(p => p.Name != ResultPropertyName);

        builder.Methods.AddRange(CreateFlatMapMethods(eventMap));
        builder.Methods.AddRange(CreateVectorMapMethods(eventMap));
        builder.Methods.Add(CreateContainerMapMethod(eventMap));
        builder.Methods.Add(CreateVectorOfContainersMapMethod(eventMap));
        builder.Methods.Add(CreateBuildEventMethod(eventMap));
        builder.Methods.Add(CreateSetTimestampMethod(eventMap));

        return builder;
    }

    private static void AddDependencies(
        ClassBuilder builder,
        IEnumerable<(string fieldName, TypeDescriptor type)> services
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
                        new("key", TypeDescriptor.From<string>()),
                        new("value", group.KeyType)
                    },
                    ReturnType = TypeNames.HerculesClientAbstractions.ITagsBuilder,
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
                        new("key", TypeDescriptor.From<string>()),
                        new("values", TypeNames.Collections.IReadOnlyList(group.ElementType.FullName))
                    },
                    ReturnType = TypeNames.HerculesClientAbstractions.ITagsBuilder,
                }
                .PrependEmitBody(w => WriteMapMethod(w, group.SameKeyEntries, WriteVectorAssignments))
            );

    private static MethodBuilder CreateContainerMapMethod(EventMapping eventMap) =>
        new MethodBuilder("AddContainer")
            {
                IsNew = true,
                Accessibility = Accessibility.Public,
                Parameters =
                {
                    new("key", TypeDescriptor.From<string>()),
                    new("valueBuilder", TypeNames.Action(TypeNames.HerculesClientAbstractions.ITagsBuilder))
                },
                ReturnType = TypeNames.HerculesClientAbstractions.ITagsBuilder
            }
            .PrependEmitBody(writer => WriteMapMethod(writer,
                eventMap.Entries.OfType<ContainerTagMap>().GroupBy(x => x.Source.Key),
                WriteContainerAssignments
            ));

    private static MethodBuilder CreateVectorOfContainersMapMethod(EventMapping eventMap) =>
        new MethodBuilder("AddVectorOfContainers")
            {
                IsNew = true,
                Accessibility = Accessibility.Public,
                Parameters =
                {
                    new("key", TypeDescriptor.From<string>()),
                    new("valueBuilders", TypeNames.Collections.IReadOnlyList(
                        TypeNames.Action(TypeNames.HerculesClientAbstractions.ITagsBuilder))
                    )
                },
                ReturnType = TypeNames.HerculesClientAbstractions.ITagsBuilder
            }
            .PrependEmitBody(writer => WriteMapMethod(writer,
                eventMap.Entries.OfType<VectorOfContainersTagMap>().GroupBy(x => x.Source.Key),
                WriteVectorOfContainerAssignments
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
                .AppendLine($"return this.{ResultPropertyName};")
        };

    private static MethodBuilder CreateSetTimestampMethod(EventMapping mapping) =>
        new MethodBuilder("SetTimestamp")
        {
            Accessibility = Accessibility.Public,
            ReturnType = TypeNames.HerculesClientAbstractions.EventBuilderInterfaceType(mapping.Type.ToString()),
            Parameters = { new ParameterBuilder("value", TypeDescriptor.From<DateTimeOffset>()) },
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
        writer.Append("this.").Append(ResultPropertyName).Append(".").Append(target.Name);

    private static void WriteVectorOfContainerAssignments(IGrouping<string, VectorOfContainersTagMap> containers,
        CodeWriter writer)
    {
        writer
            .WriteJoin(containers, null, static (map, w) => w
                .WriteVariable(map,
                    writeName: static (map, w) => w.Append(GetVectorName(map)),
                    writeValue: static (map, w) => w
                        .When(map, IsArrayStyleInit, static (map, w) => w.Append(ArrayInit(map)))
                        .When(map, IsListStyleInit, static (map, w) => w.Append(ListInit(map)))
                        .When(map, IsHashSetStyleInit, static (map, w) => w.Append(HashSetInit(map)))
                ))
            .When(containers.Any(IsArrayStyleInit), w => w.WriteVariable("index", "0"))
            .WriteForeach(containers, "valueBuilder", "valueBuilders", static (containers, writer) =>
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
                                .AppendLine($"(new {TypeNames.HerculesClientAbstractions.ITagsBuilder}[] {{")
                                .WriteBlock(default, containers, static (containers, w) => w
                                    .WriteJoin(containers, ",\n", static (map, w) => w.Append(GetBuilderVarName(map)))
                                )
                                .AppendLine().Append("})")
                        ))
                        .AppendLine("valueBuilder(builder);");

                writer.WriteJoin(containers, null, static (vector, w) => w
                        .When(vector, IsArrayStyleInit,
                            then: static (map, w) => w
                                .Append(GetVectorName(map))
                                .Append("[index] = ")
                                .Append(GetBuilderVarName(map)).Append(".BuildEvent()")
                                .AppendLine(";"),
                            @else: static (vector, w) => w
                                .Append(GetVectorName(vector))
                                .Append(".Add(")
                                .Append(GetBuilderVarName(vector)).Append(".BuildEvent()")
                                .AppendLine(");")
                        )
                    )
                    .When(containers.Any(IsArrayStyleInit), w => w.AppendLine("index++;"));
            })
            .WriteJoin(containers, null, static (map, w) => w
                .WriteTagTargetAssignment(map, static (map, w) => w.Append(GetVectorName(map)))
            );

        return;

        static void WriteBuilderInit(VectorOfContainersTagMap map, CodeWriter writer) =>
            writer.Append("this.").Append(GetBuilderProviderFieldName(map)).Append(".Get()");

        static string GetBuilderVarName(VectorOfContainersTagMap map) =>
            TextCaseConverter.ToLowerCamelCase(map.Target.Name) + "Builder";

        static bool IsArrayStyleInit(VectorOfContainersTagMap map) =>
            map.Target.VectorType is
                VectorType.Array or
                VectorType.IEnumerable or
                VectorType.IReadOnlyCollection or
                VectorType.IReadOnlyList;

        static string ArrayInit(VectorOfContainersTagMap map) =>
            $"new {map.Target.ElementType}[valueBuilders.Count]";

        static bool IsListStyleInit(VectorOfContainersTagMap map) =>
            map.Target.VectorType is
                VectorType.ICollection or
                VectorType.IList or
                VectorType.List;

        static string ListInit(VectorOfContainersTagMap map) =>
            $"new {TypeNames.Collections.List(map.Target.ElementType.ToString())}(valueBuilders.Count)";

        static bool IsHashSetStyleInit(VectorOfContainersTagMap map) =>
            map.Target.VectorType is
                VectorType.ISet or
                VectorType.IReadOnlySet or
                VectorType.HashSet;

        static string HashSetInit(VectorOfContainersTagMap map) =>
            $"new {TypeNames.Collections.HashSet(map.Target.ElementType.ToString())}()";

        static string GetVectorName(VectorOfContainersTagMap map) =>
            TextCaseConverter.ToLowerCamelCase(map.Target.Name);
    }

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
                        .AppendLine($"(new {TypeNames.HerculesClientAbstractions.ITagsBuilder}[] {{")
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
                        .When(map, IsArrayStyleInit, static (map, w) => w.Append(ArrayInit(map)))
                        .When(map, IsListStyleInit, static (map, w) => w.Append(ListInit(map)))
                        .When(map, IsHashSetStyleInit, static (map, w) => w.Append(HashSetInit(map)))
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

        static string ArrayInit(VectorTagMap map) =>
            $"new {map.Target.ElementType}[values.Count]";

        static bool IsListStyleInit(VectorTagMap map) =>
            map.Target.VectorType is
                VectorType.ICollection or
                VectorType.IList or
                VectorType.List;

        static string ListInit(VectorTagMap map) =>
            $"new {TypeNames.Collections.List(map.Target.ElementType.ToString())}(values.Count)";

        static bool IsHashSetStyleInit(VectorTagMap map) =>
            map.Target.VectorType is
                VectorType.ISet or
                VectorType.IReadOnlySet or
                VectorType.HashSet;

        static string HashSetInit(VectorTagMap map) =>
            $"new {TypeNames.Collections.HashSet(map.Target.ElementType.ToString())}()";

        static string GetVectorName(VectorTagMap map) =>
            TextCaseConverter.ToLowerCamelCase(map.Target.Name);
    }

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
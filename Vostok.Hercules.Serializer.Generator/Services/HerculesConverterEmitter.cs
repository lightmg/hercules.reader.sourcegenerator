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
using Vostok.Hercules.Serializer.Generator.Models;
using Vostok.Hercules.Serializer.Generator.Models.Sources;

namespace Vostok.Hercules.Serializer.Generator.Services;

public static class HerculesConverterEmitter
{
    private const string Namespace = "Vostok.Hercules.Client.Abstractions.Events";
    private const string DummyBuilderType = $"{Namespace}.DummyHerculesTagsBuilder";
    private const string TagsBuilderInterfaceType = $"{Namespace}.IHerculesTagsBuilder";

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
            Accessibility = Accessibility.Public,
            Interfaces = { EventBuilderInterfaceType(targetTypeFullName) },
            Properties = { PropertyBuilder.ReadOnlyField("Current", targetTypeFullName, Accessibility.Public) }
        };

        if (eventMap.EntriesWithSource<TagMapVectorSource>().Any())
            builder.Usings.AddRange([
                "System.Collections.Generic",
                "System.Linq"
            ]);

        var requiredServices = eventMap.Entries
            .Select(e => e.Converter)
            .Where(c => c?.Method is { IsStatic: false })
            .Select(c => (
                Type: c!.Value.Method.ContainingType,
                FieldName: GetConverterFieldName(c.Value),
                CtorParameterName: TextCaseConverter.ToLowerCamelCase(c.Value.Method.ContainingType.Name)
            ))
            .DistinctBy(x => x.FieldName)
            .Where(e => e.FieldName != null)
            .ToList();

        builder.Properties.AddRange(requiredServices
            .Select(x => new PropertyBuilder(x.FieldName!, ReferencedType.From(x.Type))
            {
                Accessibility = Accessibility.Private,
                Kind = ParameterKind.Field,
                ReadOnly = true
            })
        );

        builder.AddConstructor(ctor =>
        {
            ctor.Parameters.AddRange(requiredServices
                .Select(x => new ParameterBuilder(x.CtorParameterName!, ReferencedType.From(x.Type)))
            );

            ctor.AppendEmitBody(w => w
                .AppendPropertyAssignment("Current", $"new {targetTypeFullName}()")
                .WriteJoin(requiredServices, null, static (serviceInfo, cw) => cw
                    .AppendPropertyAssignment(serviceInfo.FieldName!, serviceInfo.CtorParameterName)
                )
            );
        });

        builder.Methods.AddRange(CreateFlatMapMethods(eventMap));
        builder.Methods.AddRange(CreateVectorMapMethods(eventMap));
        builder.Methods.Add(CreateBuildEventMethod(eventMap));
        builder.Methods.Add(CreateSetTimestampMethod(eventMap));

        return builder;
    }

    private static IEnumerable<MethodBuilder> CreateFlatMapMethods(EventMapping eventMap) =>
        eventMap.EntriesWithSource<TagMapFlatSource>()
            .GroupBy(x => x.Source.Type, (sourceType, entries) => (
                KeyType: sourceType,
                SameKeyEntries: entries.GroupBy(y => y.Source.Key)
            ))
            .Select(group => new MethodBuilder("AddValue")
                {
                    Accessibility = Accessibility.Public,
                    Parameters =
                    {
                        new("key", ReferencedType.From<string>()),
                        new("value", ReferencedType.From(group.KeyType.ToString()))
                    },
                    ReturnType = TagsBuilderInterfaceType,
                }
                .PrependEmitBody(w => WriteFlatMapMethod(w, group.SameKeyEntries))
            );


    private static IEnumerable<MethodBuilder> CreateVectorMapMethods(EventMapping eventMap) =>
        eventMap.EntriesWithSource<TagMapVectorSource>()
            .GroupBy(x => x.Source.ElementType, (elementType, entries) => (
                ElementType: elementType,
                SameKeyEntries: entries.GroupBy(y => y.Source.Key)
            ))
            .Select(group => new MethodBuilder("AddValue")
                {
                    Accessibility = Accessibility.Public,
                    Parameters =
                    {
                        new("key", ReferencedType.From<string>()),
                        new("values", IReadOnlyListType(group.ElementType.FullName))
                    },
                    ReturnType = TagsBuilderInterfaceType,
                }
                .PrependEmitBody(w => WriteVectorMapMethod(w, group.SameKeyEntries))
            );

    private static MethodBuilder CreateBuildEventMethod(EventMapping mapping) =>
        new MethodBuilder("BuildEvent")
        {
            Accessibility = Accessibility.Public,
            ReturnType = mapping.Type.ToString(),
            EmitBody = writer => writer
                .WriteIfElseBlock(mapping.Entries.Where(e => !e.Target.IsNullable),
                    writeCondition: (tag, w) => w.Append($"this.Current.{tag.Target.Name} == null"),
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
                    mapping.EntriesWithSource<TagMapTimestampSource>(),
                    "\n",
                    (map, w) => WriteFlatAssignment(w, map)
                )
                .AppendLine("return this;")
        };

    private static CodeWriter WriteThrowHerculesValidationException(this CodeWriter writer, string propertyName) =>
        writer.AppendLine($"throw new {ExposedApi.ValidationExceptionType.FullName}(\"{propertyName}\");");

    private static void WriteVectorMapMethod(CodeWriter writer,
        IEnumerable<IGrouping<string, TagMap<TagMapVectorSource>>> entriesByKey) =>
        writer
            .WriteIfElseBlock(entriesByKey,
                writeCondition: (entries, w) => w.Append($"key == \"{entries.Key}\""),
                writeBody: (entries, w) => w.WriteJoin(entries, "\n", (e, ew) => WriteVectorAssignment(ew, e))
            )
            .AppendLine("return this;");

    private static void WriteFlatMapMethod(CodeWriter writer,
        IEnumerable<IGrouping<string, TagMap<TagMapFlatSource>>> entriesByKey) =>
        writer
            .WriteIfElseBlock(entriesByKey,
                writeCondition: (entries, w) => w.Append($"key == \"{entries.Key}\""),
                writeBody: (entries, w) => w.WriteJoin(entries, "\n", (e, ew) => WriteFlatAssignment(ew, e))
            )
            .AppendLine("return this;");

    private static string? GetConverterFieldName(TagMapConverter? converter) =>
        converter?.Method.IsStatic is false ? $"__{converter.Value.Method.ContainingType.Name}" : null;

    private static void WriteFlatAssignment(CodeWriter writer, TagMap map) =>
        writer
            .Append($"this.Current.{map.Target.Name} = ")
            .WriteValueWithConversion(map.Converter)
            .AppendLine(';');

    private static void WriteVectorAssignment(CodeWriter writer, TagMap map) =>
        writer
            .Append($"this.Current.{map.Target.Name} = values")
            .WhenNotNull(map.Converter, (c, w) => w
                .Append(".Select(value => ")
                .WriteConverterInvoction(c)
                .Append(").ToList()"))
            .AppendLine(';');

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
        writer.Append($"this.").Append(propertyName).Append(" = ").Append(value).AppendLine(";");

    private static CodeWriter WriteIfElseBlock<T>(this CodeWriter writer, IEnumerable<T> items,
        Action<T, CodeWriter> writeCondition,
        Action<T, CodeWriter> writeBody) =>
        writer.WriteJoin(items, "else ",
            writeEntry: (item, w) =>
            {
                w.Append("if (");
                writeCondition(item, w);
                w.AppendLine(")");
                w.WriteCodeBlock(bw => writeBody(item, bw));
            });
}
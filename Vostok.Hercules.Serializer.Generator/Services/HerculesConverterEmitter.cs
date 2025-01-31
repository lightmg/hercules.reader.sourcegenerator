using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations.Extensions;
using Vostok.Hercules.Serializer.Generator.Core.Helpers;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Core.Writer;
using Vostok.Hercules.Serializer.Generator.Core.Writer.Extensions;
using Vostok.Hercules.Serializer.Generator.Extensions;
using Vostok.Hercules.Serializer.Generator.Models;
using TypeKind = Vostok.Hercules.Serializer.Generator.Core.Primitives.TypeKind;

namespace Vostok.Hercules.Serializer.Generator.Services;

public static class HerculesConverterEmitter
{
    private const string Namespace = "Vostok.Hercules.Client.Abstractions.Events";
    private const string DummyBuilderType = $"{Namespace}.DummyHerculesTagsBuilder";
    private const string TagsBuilderInterfaceType = $"{Namespace}.IHerculesTagsBuilder";

    private static string EventBuilderInterfaceType(string type) => $"{Namespace}.IHerculesEventBuilder<{type}>";

    public static TypeBuilder CreateType(EventMapping eventMap)
    {
        var targetTypeFullName = eventMap.Type.ToString();
        var builder = new TypeBuilder(
            ns: eventMap.Type.ContainingNamespace.ToString(),
            name: eventMap.Type.Name + "Builder",
            baseType: DummyBuilderType
        )
        {
            Accessibility = Accessibility.Public,
            Kind = TypeKind.Class,
            Interfaces = { EventBuilderInterfaceType(targetTypeFullName) },
            Properties = { PropertyBuilder.ReadOnlyField("Current", targetTypeFullName, Accessibility.Public) }
        };

        var requiredServices = eventMap.Entries
            .Where(e => e.Config.ConverterMethod is { IsStatic: false })
            .Select(e => (
                Type: e.Config.ConverterMethod!.ContainingType,
                FieldName: GetConverterFieldName(e),
                CtorParameterName: TextCaseConverter.ToLowerCamelCase(e.Target.Name) + "Converter"
            ))
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

        builder.Methods.AddRange(CreateAddValueMethods(eventMap));
        builder.Methods.Add(CreateBuildEventMethod(eventMap));
        builder.Methods.Add(CreateSetTimestampMethod(eventMap));

        return builder;
    }

    private static IEnumerable<MethodBuilder> CreateAddValueMethods(EventMapping eventMap) =>
        eventMap.Entries
            .GroupBy(x => x.Source.Type, (sourceType, entries) => (
                KeyType: sourceType,
                SameKeyEntries: entries.GroupBy(y => y.Source.Key)
            ), SymbolEqualityComparer.Default)
            .Select(group => new MethodBuilder("AddValue")
                {
                    Accessibility = Accessibility.Public,
                    Parameters =
                    {
                        new("key", ReferencedType.From<string>()),
                        new("value", ReferencedType.From(group.KeyType!.ToString()))
                    },
                    ReturnType = TagsBuilderInterfaceType,
                }
                .PrependEmitBody(w => WriteAddValueMethod(w, group.SameKeyEntries))
            );

    private static MethodBuilder CreateBuildEventMethod(EventMapping mapping) =>
        new MethodBuilder("BuildEvent")
        {
            Accessibility = Accessibility.Public,
            ReturnType = mapping.Type.ToString(),
            EmitBody = writer => writer
                .WriteJoin(mapping.Entries.Where(e => !e.Target.IsNullable), null, (tag, w) => w
                    .AppendLine($"if (this.Current.{tag.Target.Name} == null)").WriteCodeBlock(bw => bw
                        .WriteThrowHerculesValidationException(tag.Target.Name)
                    ))
                .AppendLine("return this.Current;")
        };

    private static MethodBuilder CreateSetTimestampMethod(EventMapping mapping) =>
        new MethodBuilder("SetTimestamp")
        {
            Accessibility = Accessibility.Public,
            ReturnType = EventBuilderInterfaceType(mapping.Type.ToString()),
            Parameters = { new ParameterBuilder("timestamp", ReferencedType.From<DateTimeOffset>()) },
            EmitBody = writer => writer.AppendLine("return this;") // todo respect TimeStamp
        };

    private static CodeWriter WriteThrowHerculesValidationException(this CodeWriter writer, string propertyName) =>
        writer.AppendLine($"throw new {ExposedApi.ValidationExceptionType.FullName}(\"{propertyName}\");");

    private static void WriteAddValueMethod(CodeWriter writer, IEnumerable<IGrouping<string, TagMap>> entriesByKey) =>
        writer
            .WriteJoin(entriesByKey, "", (entries, ifWriter) => ifWriter
                .AppendLine($"""if (key == "{entries.Key}")""") // todo else if
                .WriteCodeBlock(w => w.WriteJoin(entries, "\n", (e, ew) => WriteResultPropertyAssignment(ew, e)))
            )
            .AppendLine("return this;");

    private static string? GetConverterFieldName(TagMap tagMap) =>
        tagMap.Config.ConverterMethod?.IsStatic is false ? $"__{tagMap.Target.Name}_Converter" : null;

    private static void WriteResultPropertyAssignment(CodeWriter writer, TagMap map)
    {
        writer.Append($"this.Current.{map.Target.Name} = ");
        if (map.Config.ConverterMethod is { } convertMethod)
            writer
                .Append(GetConverterFieldName(map) ?? convertMethod.ContainingType.ToString())
                .Append('.')
                .Append(convertMethod.Name)
                .Append("(value)");
        else writer.Append("value");

        writer.AppendLine(';');
    }

    private static CodeWriter AppendPropertyAssignment(this CodeWriter writer, string propertyName, string value) =>
        writer.Append($"this.").Append(propertyName).Append(" = ").Append(value).AppendLine(";");
}
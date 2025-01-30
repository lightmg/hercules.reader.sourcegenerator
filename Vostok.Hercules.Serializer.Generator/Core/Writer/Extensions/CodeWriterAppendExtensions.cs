using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;
using Vostok.Hercules.Serializer.Generator.Core.Helpers;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using TypeKind = Vostok.Hercules.Serializer.Generator.Core.Primitives.TypeKind;

namespace Vostok.Hercules.Serializer.Generator.Core.Writer.Extensions;

public static class CodeWriterAppendExtensions
{
    public static CodeWriter AppendIfNotNull(this CodeWriter writer, string? value) =>
        value is null ? writer : writer.Append(value);

    public static CodeWriter AppendParameter(this CodeWriter writer, ParameterBuilder parameter) =>
        writer
            .AppendType(parameter.Type)
            .Append(" ")
            .Append(parameter.Name)
            .WhenNotNull(parameter.DefaultValue, (param, w) => w.Append(" = ").Append(param));

    public static CodeWriter AppendKind(this CodeWriter writer, TypeKind kind) =>
        writer.Append(kind.ToString("G").ToLower());

    public static CodeWriter AppendAttribute(this CodeWriter writer, string attribute) =>
        writer
            .When(attribute, a => !a.StartsWith("["), (_, w) => w.Append("["))
            .Append(attribute)
            .When(attribute, a => !a.EndsWith("]"), (_, w) => w.Append("]"));

    public static CodeWriter AppendType(this CodeWriter writer, ReferencedType type) =>
        writer.Append(type.FullName);

    public static CodeWriter AppendNamespace(this CodeWriter writer, string ns) =>
        writer
            .Append("namespace ")
            .Append(ns);

    public static CodeWriter AppendAccessibility(this CodeWriter writer, Accessibility accessibility) =>
        writer.Append(accessibility.ToSrcString());
}
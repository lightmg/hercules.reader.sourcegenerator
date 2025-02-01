﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;
using Vostok.Hercules.Serializer.Generator.Core.Helpers;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Core.Writer.Extensions;

public static class CodeWriterAppendExtensions
{
    public static CodeWriter AppendJoin(this CodeWriter writer, string separator, IEnumerable<string> values) =>
        writer.WriteJoin(values, separator, static (current, w) => w.Append(current));

    public static CodeWriter AppendIfNotNull(this CodeWriter writer, string? value) =>
        value is null ? writer : writer.Append(value);

    public static CodeWriter AppendParameter(this CodeWriter writer, ParameterBuilder parameter) =>
        writer
            .AppendType(parameter.Type)
            .Append(" ")
            .Append(parameter.Name)
            .WhenNotNull(parameter.DefaultValue, static (param, w) => w.Append(" = ").Append(param));

    public static CodeWriter AppendAttribute(this CodeWriter writer, string attribute) =>
        writer
            .When(attribute, a => !a.StartsWith("["), static (_, w) => w.Append("["))
            .Append(attribute)
            .When(attribute, a => !a.EndsWith("]"), static (_, w) => w.Append("]"));

    public static CodeWriter AppendType(this CodeWriter writer, ReferencedType type) =>
        writer.Append(type.FullName);

    public static CodeWriter AppendNamespace(this CodeWriter writer, string ns) =>
        writer
            .Append("namespace ")
            .Append(ns);

    public static CodeWriter AppendAccessibility(this CodeWriter writer, Accessibility accessibility) =>
        writer.Append(accessibility.ToSrcString());
}
using System;
using System.Collections.Generic;

namespace Vostok.Hercules.Serializer.Generator.Core.Writer.Extensions;

public static class CodeWriterMethodBodyExtensions
{
    public static CodeWriter WriteIfElseBlock<T>(this CodeWriter writer, IEnumerable<T> items,
        Action<T, CodeWriter> writeCondition,
        Action<T, CodeWriter> writeBody) =>
        writer.WriteJoin((writeCondition, writeBody), items, "else ",
            static (writers, item, w) => w.WriteIf(item, writers.writeCondition, writers.writeBody)
        );

    public static CodeWriter WriteIf<T>(this CodeWriter writer, T arg,
        Action<T, CodeWriter> writeCondition,
        Action<T, CodeWriter> then,
        Action<T, CodeWriter>? @else = null) =>
        writer
            .WriteBlock(("if (", ")\n"), arg, writeCondition)
            .WriteCodeBlock(arg, then)
            .WhenNotNull(@else, arg, static (arg, @else, w) => w
                .AppendLine("else").WriteCodeBlock(arg, @else)
            );

    public static CodeWriter WriteForeach<T>(this CodeWriter writer, T arg,
        string entryName,
        string collectionName,
        Action<T, CodeWriter> writeBody) =>
        writer
            .AppendForeach(entryName, collectionName)
            .WriteCodeBlock(arg, writeBody);

    public static CodeWriter WriteVariable<T>(this CodeWriter writer, T arg,
        Action<T, CodeWriter> writeName,
        Action<T, CodeWriter> writeValue) =>
        writer.Append("var ").Append(arg, writeName).Append(" = ").Append(arg, writeValue).AppendLine(";");

    public static CodeWriter WriteVariable<T>(this CodeWriter writer, T arg,
        string name,
        Action<T, CodeWriter> writeValue) =>
        writer.Append("var ").Append(name).Append(" = ").Append(arg, writeValue).AppendLine(";");

    public static CodeWriter WriteVariable(this CodeWriter writer,
        Action<CodeWriter> writeName,
        Action<CodeWriter> writeValue) =>
        writer.Append("var ").Append(writeName).Append(" = ").Append(writeValue).AppendLine(";");

    public static CodeWriter WriteVariable(this CodeWriter writer, string name, string value) =>
        writer.Append("var ").Append(name).Append(" = ").Append(value).AppendLine(";");
}
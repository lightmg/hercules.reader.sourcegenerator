using System;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Vostok.Hercules.Serializer.Generator.Core.Writer;

public readonly struct CodeWriter
{
    private const string Indent = "    ";
    private readonly StringBuilder builder;
    private readonly int indent;

    public CodeWriter() : this(new(), 0)
    {
    }

    public CodeWriter(StringBuilder builder, int indent)
    {
        this.builder = builder;
        this.indent = indent < 0 ? 0 : indent;
    }

    public CodeWriter EnterBlock() =>
        new CodeWriter(builder, indent + 1);

    public CodeWriter Append(string text)
    {
        if (string.IsNullOrEmpty(text))
            return this;

        if (ShouldIndent())
            AppendIndent();

        builder.Append(text);
        return this;
    }

    public CodeWriter Append(CodeWriter writer)
    {
        if (writer.builder.Length == 0)
            return this;

        if (ShouldIndent())
            AppendIndent();

        builder.Append(writer.builder); 
        return this;
    }

    public CodeWriter Append(char symbol)
    {
        if (ShouldIndent())
            AppendIndent();

        builder.Append(symbol);
        return this;
    }

    public CodeWriter AppendLine()
    {
        builder.AppendLine();
        return this;
    }

    public CodeWriter AppendLine(string text) =>
        Append(text).AppendLine();

    public CodeWriter AppendLine(char symbol) =>
        Append(symbol).AppendLine();

    private bool ShouldIndent() =>
        builder.Length != 0 && builder[builder.Length - 1] == '\n';

    private void AppendIndent()
    {
        for (var i = 0; i < indent; i++)
            builder.Append(Indent);
    }

    public override string ToString() =>
        builder.ToString();

    public SourceText ToUtf8SourceText() =>
        SourceText.From(ToString(), Encoding.UTF8);

    public static SourceText CreateUtf8SourceText<T>(T arg, Action<T, CodeWriter> write)
    {
        var writer = new CodeWriter();
        write(arg, writer);

        return writer.ToUtf8SourceText();
    }

    public static SourceText CreateUtf8SourceText(Action<CodeWriter> write)
    {
        var writer = new CodeWriter();
        write(writer);

        return writer.ToUtf8SourceText();
    }

    public static string CreateString(Action<CodeWriter> write)
    {
        var writer = new CodeWriter();
        write(writer);

        return writer.ToString();
    }
}
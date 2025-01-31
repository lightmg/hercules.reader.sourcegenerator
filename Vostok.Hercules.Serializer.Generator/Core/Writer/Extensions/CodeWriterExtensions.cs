using System;

namespace Vostok.Hercules.Serializer.Generator.Core.Writer.Extensions;

public static class CodeWriterExtensions
{
    public static CodeWriter WhenOfType<T>(this CodeWriter writer, object? arg, Action<T, CodeWriter> transform)
    {
        if (arg is T value)
            transform(value, writer);

        return writer;
    }
    public static CodeWriter WhenNotNull<T>(this CodeWriter writer, T? arg, Action<T, CodeWriter> transform)
    {
        if (arg is not null)
            transform(arg, writer);

        return writer;
    }

    public static CodeWriter WhenNotNull<T>(this CodeWriter writer, T? arg,
        Action<T, CodeWriter> @then,
        Action<CodeWriter> @else
    )
    {
        if (arg is not null)
            then(arg, writer);
        else
            @else(writer);

        return writer;
    }

    public static CodeWriter When(this CodeWriter writer, bool condition, Action<CodeWriter> transform)
    {
        if (condition)
            transform(writer);

        return writer;
    }

    public static CodeWriter When(this CodeWriter writer, Func<bool> condition, Action<CodeWriter> transform) =>
        writer.When(condition(), transform);

    public static CodeWriter When<T>(this CodeWriter writer, T arg, Func<T, bool> condition,
        Action<T, CodeWriter> transform)
    {
        if (condition(arg))
            transform(arg, writer);
        return writer;
    }
}
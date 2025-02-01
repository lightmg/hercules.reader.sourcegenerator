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

    public static CodeWriter WhenNotNull<T>(this CodeWriter writer, T? arg,
        Action<T, CodeWriter> @then,
        Action<CodeWriter>? @else = null
    ) where T : class
    {
        if (arg is not null)
            then(arg, writer);
        else
            @else?.Invoke(writer);

        return writer;
    }

    public static CodeWriter WhenNotNull<T>(this CodeWriter writer, T? arg,
        Action<T, CodeWriter> @then,
        Action<CodeWriter>? @else = null
    ) where T : struct
    {
        if (arg.HasValue)
            then(arg.Value, writer);
        else
            @else?.Invoke(writer);

        return writer;
    }
    
    public static CodeWriter WhenNotNull<T, TArg1>(this CodeWriter writer, T? arg,
        TArg1 arg1,
        Action<TArg1, T, CodeWriter> @then,
        Action<TArg1, CodeWriter>? @else = null
    ) where T : class
    {
        if (arg is not null)
            then(arg1, arg, writer);
        else
            @else?.Invoke(arg1, writer);

        return writer;
    }

    public static CodeWriter WhenNotNull<T, TArg1>(this CodeWriter writer, T? arg,
        TArg1 arg1,
        Action<TArg1, T, CodeWriter> @then,
        Action<TArg1, CodeWriter>? @else = null
    ) where T : struct
    {
        if (arg.HasValue)
            then(arg1, arg.Value, writer);
        else
            @else?.Invoke(arg1, writer);

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
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Hercules.Serializer.Generator.Extensions;

public static class EnumerableExtensions
{
    public static int GetElementsHashCode<T>(this IEnumerable<T> source)
    {
        // Ctrl+C & Ctrl+V from https://stackoverflow.com/a/8094931
        unchecked
        {
            return source.Aggregate(19, (current, item) => current * 31 + (item?.GetHashCode() ?? 0));
        }
    }

    public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
    {
        foreach (var item in items)
            list.Add(item);
    }

    public static IEnumerable<T> PrependWhen<T>(this IEnumerable<T> source, bool condition, T value) =>
        condition ? source.Prepend(value) : source;

    public static IEnumerable<T> AppendWhen<T>(this IEnumerable<T> source, bool condition, T value) =>
        condition ? source.Append(value) : source;

    public static IEnumerable<T> PrependIfNotNull<T>(this IEnumerable<T> source, T? value) where T : struct =>
        value is null ? source : source.Prepend(value.Value);

    public static IEnumerable<T> AppendIfNotNull<T>(this IEnumerable<T> source, T? value) where T : struct =>
        value is null ? source : source.Append(value.Value);

    public static IEnumerable<T> PrependIfNotNull<T>(this IEnumerable<T> source, T? value) where T : class =>
        value is null ? source : source.Prepend(value);

    public static IEnumerable<T> AppendIfNotNull<T>(this IEnumerable<T> source, T? value) where T : class =>
        value is null ? source : source.Append(value);
}
using SourceGenerator.Core.Builders.Types.Abstract;
using SourceGenerator.Core.Helpers;
using SourceGenerator.Core.Primitives;

namespace SourceGenerator.Core.Builders.Types;

public class EnumBuilder : TypeBuilder
{
    public EnumBuilder(string ns, string name) : base(ns, name)
    {
    }

    public bool IsFlags { get; set; } = false;

    public TypeDescriptor BaseType { get; set; } = typeof(int);

    public IDictionary<string, object> Values { get; } = new Dictionary<string, object>();

    public override IEnumerable<string> Attributes => IsFlags
        ? base.Attributes.Append(typeof(FlagsAttribute).FullName)
        : base.Attributes;

    public static EnumBuilder CreateFrom<T>(string ns, string? name = null) where T : struct, Enum
    {
        var enumType = typeof(T);
        var builder = new EnumBuilder(ns, name ?? enumType.Name);

        foreach (var entry in TypeUtilities.GetEnumKeysWithValues<T>())
            builder.Values.Add(entry.Key, entry.Value);

        return builder;
    }
}
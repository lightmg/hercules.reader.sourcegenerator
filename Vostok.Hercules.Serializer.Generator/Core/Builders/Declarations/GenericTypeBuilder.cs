using System.Collections.Generic;
using System.Linq;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;

public readonly struct GenericTypeBuilder(string name)
{
    public string Name { get; } = name;

    public IList<ReferencedType> Constraints { get; } = [];

    public static string AsGenericArgsSrc(IEnumerable<GenericTypeBuilder> args) =>
        $"<{string.Join(", ", args.Select(g => g.Name))}>";

    public static string AsGenericConstraintsSrc(IEnumerable<GenericTypeBuilder> args) =>
        string.Join(" ", args
            .Where(g => g.Constraints.Any())
            .Select(g => $"where {g.Name}: {string.Join(", ", g.Constraints)}")
        );

    public static implicit operator GenericTypeBuilder(string name) => new(name);
}
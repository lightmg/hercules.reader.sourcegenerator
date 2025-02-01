using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Extensions;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Members;

public class GenericTypeBuilder(string name)
{
    public string Name { get; } = name;

    public IList<ReferencedType> Constraints { get; set; } = [];

    public bool HasNewConstraint { get; set; }

    public VarianceKind Variance { get; set; }

    public IEnumerable<string> AllConstraints =>
        Constraints.Select(c => c.FullName).PrependWhen(HasNewConstraint, "new()");

    public static string AsGenericArgsSrc(IEnumerable<GenericTypeBuilder> args) =>
        $"<{string.Join(", ", args.Select(g => g.Name))}>";

    public static string AsGenericConstraintsSrc(IEnumerable<GenericTypeBuilder> args) =>
        string.Join(" ", args
            .Where(g => g.Constraints.Any())
            .Select(g => $"where {g.Name}: {string.Join(", ", g.Constraints)}")
        );

    public static implicit operator GenericTypeBuilder(string name) => new(name);
}
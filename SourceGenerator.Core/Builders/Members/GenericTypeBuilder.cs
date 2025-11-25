using Microsoft.CodeAnalysis;
using SourceGenerator.Core.Primitives;

namespace SourceGenerator.Core.Builders.Members;

public class GenericTypeBuilder(string name)
{
    public string Name { get; } = name;

    public IList<TypeDescriptor> Constraints { get; set; } = [];

    public bool HasNewConstraint { get; set; }

    public VarianceKind Variance { get; set; }

    public IEnumerable<string> AllConstraints =>
        HasNewConstraint
            ? Constraints.Select(c => c.FullName).Prepend("new()")
            : Constraints.Select(c => c.FullName);

    public static string AsGenericArgsSrc(IEnumerable<GenericTypeBuilder> args) =>
        $"<{string.Join(", ", args.Select(g => g.Name))}>";

    public static string AsGenericConstraintsSrc(IEnumerable<GenericTypeBuilder> args) =>
        string.Join(" ", args
            .Where(g => g.Constraints.Any())
            .Select(g => $"where {g.Name}: {string.Join(", ", g.Constraints)}")
        );

    public static implicit operator GenericTypeBuilder(string name) => new(name);
}
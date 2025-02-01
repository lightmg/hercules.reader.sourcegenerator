using System.Diagnostics.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Models.Vector;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum VectorType
{
    Array,

    IEnumerable,

    ICollection,
    IReadOnlyCollection,

    IList,
    IReadOnlyList,
    List,

    ISet,
    IReadOnlySet,
    HashSet
}
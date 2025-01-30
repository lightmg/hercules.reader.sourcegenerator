using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using TypeKind = Vostok.Hercules.Serializer.Generator.Core.Primitives.TypeKind;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;

public class TypeBuilder
{
    public TypeBuilder(string ns, string name, ReferencedType? baseType = null)
    {
        Name = name;
        Namespace = ns;
        BaseType = baseType;
    }

    public string Name { get; }

    public string Namespace { get; }

    public string FullName => $"{Namespace}.{Name}";

    public Accessibility Accessibility { get; set; } = Accessibility.Public;

    public TypeKind Kind { get; set; } = TypeKind.Class;

    public ReferencedType? BaseType { get; }

    public IList<ReferencedType> Interfaces { get; } = [];

    public IList<GenericTypeBuilder> Generics { get; } = [];

    public IList<PropertyBuilder> Properties { get; } = [];

    public IList<ConstructorBuilder> Constructors { get; } = [];

    public IList<MethodBuilder> Methods { get; } = [];

    public virtual IEnumerable<string> Attributes => [];
}
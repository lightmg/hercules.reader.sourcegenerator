using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;

public abstract class TypeBuilder : ITypeBuilder
{
    protected TypeBuilder(string ns, string name)
    {
        Name = name;
        Namespace = ns;
    }

    public string Name { get; }

    public string Namespace { get; }

    public string FullName => $"{Namespace}.{Name}";

    public Accessibility Accessibility { get; set; } = Accessibility.Public;

    public virtual IEnumerable<string> Attributes => [];
}
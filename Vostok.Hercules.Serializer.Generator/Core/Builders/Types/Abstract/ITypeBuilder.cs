using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;

public interface ITypeBuilder
{
    string Name { get; }

    string Namespace { get; }

    string FullName { get; }

    Accessibility Accessibility { get; set; }

    IEnumerable<string> Attributes { get; }

    IList<string> Usings { get; }
}
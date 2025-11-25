using SourceGenerator.Core.Builders.Types.Abstract;

namespace SourceGenerator.Core.Builders.Types;

public class InterfaceBuilder : StatefulTypeBuilder
{
    public InterfaceBuilder(string ns, string name) : base(ns, name)
    {
    }
}
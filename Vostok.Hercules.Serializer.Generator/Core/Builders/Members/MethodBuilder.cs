using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members.Abstract;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Core.Writer;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Members;

public class MethodBuilder : IMethodBodyBuilder, ITypeMemberBuilder
{
    public MethodBuilder(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public TypeDescriptor? ReturnType { get; set; } = null;

    public IList<GenericTypeBuilder> Generics { get; set; } = [];

    public IList<ParameterBuilder> Parameters { get; set; } = [];

    public bool IsStatic { get; set; } = false;

    public bool IsOverride { get; set; } = false;

    public bool IsNew { get; set; } = false;

    public Action<CodeWriter>? EmitBody { get; set; }

    public Accessibility Accessibility { get; set; } = Accessibility.Public;
}
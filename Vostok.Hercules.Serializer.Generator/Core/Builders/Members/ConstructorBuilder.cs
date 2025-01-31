using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members.Abstract;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;
using Vostok.Hercules.Serializer.Generator.Core.Writer;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Members;

public class ConstructorBuilder : IMethodBodyBuilder, ITypeMemberBuilder
{
    public ConstructorBuilder(IInitializabeTypeBuilder typeBuilder) : this(typeBuilder.Name)
    {
    }

    public ConstructorBuilder(string declaringTypeName)
    {
        DeclaringTypeName = declaringTypeName;
        EmitBody = _ => { };
    }

    public string DeclaringTypeName { get; }

    public IList<ParameterBuilder> Parameters { get; } = [];

    public IDictionary<string, string> BaseCtorArgs { get; } = new Dictionary<string, string>();

    public Action<CodeWriter>? EmitBody { get; set; }

    public Accessibility Accessibility { get; set; } = Accessibility.Public;
}
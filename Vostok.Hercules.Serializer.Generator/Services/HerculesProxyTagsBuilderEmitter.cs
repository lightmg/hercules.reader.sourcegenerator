using System.Linq;
using Microsoft.CodeAnalysis;
using SourceGenerator.Core.Builders.Declarations.Extensions;
using SourceGenerator.Core.Builders.Members;
using SourceGenerator.Core.Builders.Types;
using SourceGenerator.Core.Primitives;
using SourceGenerator.Core.Writer.Extensions;

namespace Vostok.Hercules.Serializer.Generator.Services;

public static class HerculesProxyTagsBuilderEmitter
{
    public const string TypeName = "HerculesProxyTagsBuilder";

    public const string FullName = $"{Namespace}.{TypeName}";

    private const string Namespace = ExposedApi.Namespace;

    public static ClassBuilder CreateProxy(INamedTypeSymbol tagsBuilderInterface)
    {
        var builder = new ClassBuilder(ExposedApi.Namespace, TypeName)
            {
                Interfaces = { TypeDescriptor.From(tagsBuilderInterface) },
                Accessibility = Accessibility.Internal,
                Generics = tagsBuilderInterface.TypeParameters.Select(ConvertGeneric).ToList(),
                Properties =
                {
                    new("builders", TypeNames.HerculesClientAbstractions.ITagsBuilder + "[]")
                    {
                        Kind = ParameterKind.Field,
                        ReadOnly = true,
                        Accessibility = Accessibility.Private
                    }
                }
            }
            .AddConstructor()
            .AddPropertiesCtorInit(p => p.Name == "builders");

        foreach (var method in tagsBuilderInterface.GetMembers().OfType<IMethodSymbol>())
            builder.Methods.Add(new MethodBuilder(method.Name)
            {
                IsStatic = false,
                IsOverride = false,
                Accessibility = Accessibility.Public,
                ReturnType = TypeDescriptor.From(method.ReturnType),
                Generics = method.TypeParameters
                    .Select(ConvertGeneric)
                    .ToList(),
                Parameters = method.Parameters
                    .Select(p => new ParameterBuilder(p.Name, TypeDescriptor.From(p.Type)))
                    .ToList(),
                EmitBody = writer => writer
                    .WriteForeach(method, "builder", "builders", static (method, w) => w
                        .Append("builder.")
                        .Append(method.Name)
                        .Append("(")
                        .WriteJoin(method.Parameters, ", ", static (param, w) => w.Append(param.Name))
                        .AppendLine(");")
                    )
                    .AppendLine("return this;")
            });

        return builder;
    }

    private static GenericTypeBuilder ConvertGeneric(ITypeParameterSymbol t) =>
        new GenericTypeBuilder(t.Name)
        {
            HasNewConstraint = t.HasConstructorConstraint,
            Variance = t.Variance,
            Constraints = t.ConstraintTypes.Select(TypeDescriptor.From).ToList()
        };
}
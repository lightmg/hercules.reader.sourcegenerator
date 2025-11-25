using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator;
using SourceGenerator.Core.Builders.Declarations;
using SourceGenerator.Core.Builders.Declarations.Extensions;
using SourceGenerator.Core.Builders.Members;
using SourceGenerator.Core.Builders.Types;
using SourceGenerator.Core.Builders.Types.Abstract;
using SourceGenerator.Core.Primitives;
using SourceGenerator.Core.Writer;
using SourceGenerator.Core.Writer.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Vostok.Hercules.Serializer.Tests;

public class CodeWriterTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public CodeWriterTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test()
    {
        var writer = new CodeWriter()
            .WriteType(new ClassBuilder("TestNameSpace.Gen", "TestType")
                {
                    Accessibility = Accessibility.Public,
                    Properties =
                    {
                        new PropertyBuilder("Prop1", typeof(string)) { Kind = ParameterKind.Property },
                        new PropertyBuilder("Field1", typeof(int)) { Kind = ParameterKind.Field, ReadOnly = true }
                    }
                }
                .AddConstructor()
                .AddPropertiesCtorInit(x => x.Name == "Field1")
            );

        var emittedSrc = writer.ToString();
        testOutputHelper.WriteLine(emittedSrc);
    }

    public static IEnumerable<object[]> ExposedApiCases => typeof(ExposedApi)
        .GetMembers(BindingFlags.Public | BindingFlags.Static)
        .Select(m => m switch
        {
            PropertyInfo p => p.GetValue(null),
            FieldInfo p => p.GetValue(null),
            _ => null
        })
        .OfType<TypeBuilder>()
        .Select(x => new object[] { x });

    [Theory]
    [MemberData(nameof(ExposedApiCases))]
    internal void ExposedApi(TypeBuilder type)
    {
        var result = CodeWriter.CreateString(w => w.WriteType(type));
        testOutputHelper.WriteLine(result);
    }
}
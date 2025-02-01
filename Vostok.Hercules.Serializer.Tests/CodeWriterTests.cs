using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations.Extensions;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;
using Vostok.Hercules.Serializer.Generator.Core.Writer;
using Vostok.Hercules.Serializer.Generator.Core.Writer.Extensions;
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
using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Vostok.Hercules.Client.Abstractions.Values;
using Vostok.Hercules.Serializer.Generator;
using Xunit;

namespace Vostok.Hercules.Serializer.Tests;

public class HerculesSerializationSourceGeneratorTests
{
    private const string ClassText =
        $$"""
        using Vostok.Hercules.Serializer.Generator;
        using System.Collections.Generic;
        
        namespace TestNamespace;

        public static class StaticConverters {
            public static int Convert(long value) => value;
        }
        
        [GenerateHerculesReader]
        public class Model 
        {
            [HerculesTag("hsv")]
            public HashSet<string> HashSetVector { get; set; }

            [HerculesTag("sv")]
            public IReadOnlyList<string> StringVector { get; set; }

            [HerculesTag("sv2")]
            public string[] AnotherStringVector { get; set; }

            [HerculesTag(SpecialTagKind.Timestamp)]
            public DateTimeOffset Timestamp {get;set;}
            
            [HerculesTag("h"), HerculesConverter(typeof(StaticConverters), "Convert"]
            public int? NullableInt {get;set;}
            
            [HerculesTag("name")]
            public string Name { get; set; }

            [HerculesTag("naname")]
            public System.String NotAName { get; set; }

            [HerculesTag("sss")]
            public short Sss { get; set; }
            
            [HerculesTag("fff")]
            public float Fff { get; set; }
            
            [HerculesTag("id")]
            public System.Guid Id { get; set; }
        
        }
        """;

    [Fact]
    public void Test()
    {
        // Create an instance of the source generator.
        var generator = new HerculesSerializationSourceGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(generator);

        // We need to create a compilation with the required source code.
        var compilation = CSharpCompilation.Create(
            nameof(HerculesSerializationSourceGeneratorTests),
            [CSharpSyntaxTree.ParseText(ClassText)],
            [
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(HerculesValue).Assembly.Location)
            ]);
        
        var result = driver.RunGenerators(compilation);
    }
}
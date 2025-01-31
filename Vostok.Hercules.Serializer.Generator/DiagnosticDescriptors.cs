using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator;

public static class DiagnosticDescriptors
{
    private const string Category = "Vostok.Hercules.Serializer.SourceGenerator";

    public static DiagnosticDescriptor UnexpectedError => new DiagnosticDescriptor(
        id: "VHSG00",
        title: "Unexpected error",
        messageFormat: "Unexpected error occured during mapping generation: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor UnknownType => new DiagnosticDescriptor(
        id: "VHSG01",
        title: "Unknown hercules tag type",
        messageFormat: "Inferred type '{0}' is not Hercules primitive. " +
                       "Consider specifying Converter or change member type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DuplicatedAnnotation => new DiagnosticDescriptor(
        id: "VHSG02",
        title: "Unexpected duplicated annotations",
        messageFormat: "Annotation '{0}' shouldn't be duplicated",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor ConverterMethodNotFound => new DiagnosticDescriptor(
        id: "VHSG03",
        title: "Converter method not found",
        messageFormat: "Unable to find converter method '{0}' in type '{1}'. " +
                       "Ensure name is correct and method is at least internal accessible",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor ConverterMethodAmbigious => new DiagnosticDescriptor(
        id: "VHSG04",
        title: "Converter method ambigious reference",
        messageFormat: "Several suitable methods with name '{0}' are found in type '{1}'. " +
                       "Matches are: {2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor ConverterMethodShouldBeStatic => new DiagnosticDescriptor(
        id: "VHSG05",
        title: "Converter method should be static",
        messageFormat: "Non-static converter methods are not supported yet. " +
                       "Consider making method '{0}' in type '{1}' static, or choose different one.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor MissingParameterlessCtor => new DiagnosticDescriptor(
        id: "VHSG06",
        title: "Missing parameterless constructor",
        messageFormat: "Type '{0}' should have accessible parameterless constructor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor BadAnnotationArgument => new DiagnosticDescriptor(
        id: "VHSG07",
        title: "Annotation argument is out of range",
        messageFormat: "Argument for annotation '{0}' at index {1} has invalid value: {2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}
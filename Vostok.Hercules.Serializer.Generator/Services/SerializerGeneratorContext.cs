using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Services;

internal readonly struct MappingGeneratorContext()
{
    private readonly List<Diagnostic> diagnostics = new List<Diagnostic>();

    public IReadOnlyCollection<Diagnostic> Diagnostics => diagnostics;

    public void AddDiagnostic(DiagnosticDescriptor descriptor, ISymbol location, params object[] messageArgs) =>
        diagnostics.Add(Diagnostic.Create(descriptor,
            location.Locations.FirstOrDefault(),
            additionalLocations: location.Locations.Skip(1),
            messageArgs: messageArgs
        ));
}
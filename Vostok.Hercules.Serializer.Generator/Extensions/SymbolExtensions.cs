using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Extensions;

public static class SymbolExtensions
{
    public static bool Is<T>(this ITypeSymbol symbol) => 
        symbol.ToString() == typeof(T).FullName;
}
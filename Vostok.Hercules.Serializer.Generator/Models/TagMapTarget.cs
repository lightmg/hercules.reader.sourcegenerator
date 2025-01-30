using System;
using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Models;

public readonly struct TagMapTarget
{
    public readonly ISymbol Symbol;
    public readonly ITypeSymbol Type;

    private TagMapTarget(ISymbol symbol)
    {
        Symbol = symbol;
        Type = Get(static x => x.Type, static x => x.Type);
    }

    public string Name => Symbol.Name;

    public IFieldSymbol? AsField => Symbol as IFieldSymbol;

    public IPropertySymbol? AsProperty => Symbol as IPropertySymbol;

    public T Get<T>(Func<IPropertySymbol, T> getProperty, Func<IFieldSymbol, T> getField) =>
        Symbol switch
        {
            IPropertySymbol property => getProperty(property),
            IFieldSymbol field => getField(field),
            _ => throw InvalidSymbolException(Symbol)
        };

    public static TagMapTarget FromProperty(IPropertySymbol symbol) =>
        new(symbol);

    public static TagMapTarget FromField(IFieldSymbol symbol) =>
        new(symbol);

    public static TagMapTarget Create(ISymbol symbol) =>
        symbol is IPropertySymbol or IFieldSymbol
            ? new(symbol)
            : throw InvalidSymbolException(symbol);

    private static Exception InvalidSymbolException(ISymbol symbol) =>
        new InvalidOperationException($"The symbol {symbol} is not a property or field");
}
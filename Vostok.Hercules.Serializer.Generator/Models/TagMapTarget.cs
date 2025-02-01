using System;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Services;

namespace Vostok.Hercules.Serializer.Generator.Models;

public class TagMapTarget
{
    public readonly ISymbol Symbol;
    public readonly ITypeSymbol Type;

    public TagMapTarget(ISymbol symbol)
    {
        if (symbol is not (IFieldSymbol or IPropertySymbol))
            throw new ArgumentException($"Symbol '{symbol}' is not a field or property.", nameof(symbol));

        Symbol = symbol;
        Type = Get(static x => x.Type, static x => x.Type);
    }

    public string Name => Symbol.Name;

    public bool IsNullable => TypeUtilities.IsNullable(Type, out _);

    public IFieldSymbol? AsField => Symbol as IFieldSymbol;

    public IPropertySymbol? AsProperty => Symbol as IPropertySymbol;

    public T Get<T>(Func<IPropertySymbol, T> getProperty, Func<IFieldSymbol, T> getField) =>
        Symbol switch
        {
            IPropertySymbol property => getProperty(property),
            IFieldSymbol field => getField(field),
            _ => throw InvalidSymbolException(Symbol)
        };

    private static Exception InvalidSymbolException(ISymbol symbol) =>
        new InvalidOperationException($"The symbol {symbol} is not a property or field");
}
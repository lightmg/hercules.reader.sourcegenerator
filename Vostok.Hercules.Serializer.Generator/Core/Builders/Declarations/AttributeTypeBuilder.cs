using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Hercules.Serializer.Generator.Core.Helpers;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;

public class AttributeTypeBuilder(string ns, string name) 
    : TypeBuilder(ns, StringUtils.RemoveSuffix(name, "Attribute"), baseType: typeof(Attribute))
{
    public AttributeTargets Usage { get; set; } = AttributeTargets.All;

    public bool AllowMultiple { get; set; } = false;

    public bool Inherited { get; set; } = true;

    public override IEnumerable<string> Attributes => base.Attributes.Prepend(GetAttributeUsageRawSrc());

    private string GetAttributeUsageRawSrc() =>
        string.Format(
            "{0}({1}, AllowMultiple = {2}, Inherited = {3})",
            typeof(AttributeUsageAttribute).FullName, GetUsageValue(), AllowMultiple, Inherited
        );

    private string GetUsageValue()
    {
        var typeFullName = Usage.GetType().FullName + '.';
        return typeFullName + Usage
            .ToString("G")
            .Replace(", ", $"| {typeFullName}");
    }
}
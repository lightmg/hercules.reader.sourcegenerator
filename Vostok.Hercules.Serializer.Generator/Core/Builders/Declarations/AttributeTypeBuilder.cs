using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;

public class AttributeTypeBuilder(string ns, string name) 
    : TypeBuilder(ns, name, baseType: typeof(Attribute))
{
    public AttributeTargets Usage { get; set; } = AttributeTargets.All;

    public bool AllowMultiple { get; set; } = false;

    public bool Inherited { get; set; } = true;

    public override IEnumerable<string> Attributes => base.Attributes
        .Prepend(GetAttributeUsageRawSrc())
        .Prepend(ExposedApi.EmbeddedAttribute.FullName);

    private string GetAttributeUsageRawSrc() =>
        string.Format(
            "{0}({1}, AllowMultiple = {2}, Inherited = {3})",
            typeof(AttributeUsageAttribute).FullName, GetUsageValue(), BoolString(AllowMultiple), BoolString(Inherited)
        );

    private string GetUsageValue()
    {
        var typeFullName = Usage.GetType().FullName + '.';
        return typeFullName + Usage
            .ToString("G")
            .Replace(", ", $"| {typeFullName}");
    }

    private static string BoolString(bool flag) => 
        flag ? "true" : "false";
}
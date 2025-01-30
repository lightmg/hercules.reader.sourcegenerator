using System;
using System.Linq;
using System.Reflection;

namespace Vostok.Hercules.Serializer.Generator.Models;

public readonly struct TargetTypeInfo(string name, string ns) : IEquatable<TargetTypeInfo>
{
    public readonly string Name = name;
    public readonly string Namespace = ns;

    public string FullName => $"{Namespace}.{Name}";

    public bool IsPrimitive => Primitives.All.Contains(this);

    #region Equality

    public bool Equals(TargetTypeInfo other) =>
        Name == other.Name && Namespace == other.Namespace;

    public override bool Equals(object? obj) =>
        obj is TargetTypeInfo other && Equals(other);

    public override int GetHashCode() =>
        unchecked(Name.GetHashCode() * 397 ^ Namespace.GetHashCode());

    public static bool operator ==(TargetTypeInfo left, TargetTypeInfo right) =>
        left.Equals(right);

    public static bool operator !=(TargetTypeInfo left, TargetTypeInfo right) =>
        !left.Equals(right);

    #endregion Equality

    public static TargetTypeInfo From<T>() => From(typeof(T));

    public static TargetTypeInfo From(Type type) => new(type.Name, type.Namespace!);

    public static class Primitives
    {
        public static readonly TargetTypeInfo Byte = From<byte>();
        public static readonly TargetTypeInfo Boolean = From<bool>();
        public static readonly TargetTypeInfo Short = From<short>();
        public static readonly TargetTypeInfo Int = From<int>();
        public static readonly TargetTypeInfo Long = From<long>();
        public static readonly TargetTypeInfo Double = From<double>();
        public static readonly TargetTypeInfo Float = From<float>();
        public static readonly TargetTypeInfo String = From<string>();
        public static readonly TargetTypeInfo Guid = From<Guid>();

        // what about nullables?
        public static readonly TargetTypeInfo[] All = typeof(Primitives)
            .GetMembers(BindingFlags.Static | BindingFlags.Public)
            .Select(m => m switch
            {
                PropertyInfo p => p.GetValue(null) as TargetTypeInfo?,
                FieldInfo f => f.GetValue(null) as TargetTypeInfo?,
                _ => null
            })
            .Where(x => x != null)
            .Select(x => x!.Value)
            .ToArray();
    }
}
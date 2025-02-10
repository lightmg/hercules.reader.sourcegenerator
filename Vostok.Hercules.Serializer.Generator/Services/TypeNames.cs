using System.Diagnostics.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Services;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class TypeNames
{
    public static string Action(string type) =>
        $"System.Action<{type}>";

    public static class Collections
    {
        public const string Namespace = "System.Collections.Generic";

        public static string HashSet(string type) =>
            $"System.Collections.Generic.HashSet<{type}>";

        public static string List(string type) =>
            $"System.Collections.Generic.List<{type}>";

        public static string IReadOnlyList(string type) =>
            $"{Namespace}.IReadOnlyList<{type}>";
    }

    public static class HerculesClientAbstractions
    {
        private const string Namespace = "Vostok.Hercules.Client.Abstractions.Events";
        public const string DummyBuilderType = $"{Namespace}.DummyHerculesTagsBuilder";
        public const string ITagsBuilder = $"{Namespace}.IHerculesTagsBuilder";
        
        public static string EventBuilderInterfaceType(string type) =>
            $"{Namespace}.IHerculesEventBuilder<{type}>";
    }
}
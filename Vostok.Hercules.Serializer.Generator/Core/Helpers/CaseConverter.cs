namespace Vostok.Hercules.Serializer.Generator.Core.Helpers;

public static class TextCaseConverter
{
    public static string ToLowerCamelCase(string upperCamelCase) => 
        char.ToLower(upperCamelCase[0]) + upperCamelCase.Substring(1, upperCamelCase.Length - 1);
}
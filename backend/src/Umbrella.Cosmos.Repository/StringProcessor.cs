using System.Text.RegularExpressions;

namespace Umbrella.Cosmos.Repository;

public static class StringProcessor
{
    public static string ToSnake(string input)
    {
        return Regex.Replace(input, @"(\p{Ll})(\p{Lu})", "$1-$2").ToLower();
    }
}
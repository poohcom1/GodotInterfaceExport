using System.Globalization;
using System.Text.RegularExpressions;

namespace GodotSharp.SourceGenerators
{
    internal static class StringExtensions
    {
        public static string Truncate(this string source, int maxChars) =>
            source.Length <= maxChars ? source : source.Substring(0, maxChars);
    }
}

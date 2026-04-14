using System.Text.RegularExpressions;

namespace WikipediaPlaywrightTests.Utilities;

public static partial class TextNormalizer
{
    [GeneratedRegex(@"\[[^\]]+\]", RegexOptions.Compiled)]
    private static partial Regex ReferenceRegex();

    [GeneratedRegex(@"[^a-z0-9\s]", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex MultiWhitespaceRegex();

    public static string Normalize(string input)
    {
        var withoutReferences = ReferenceRegex().Replace(input, " ");
        var lowered = withoutReferences.ToLowerInvariant();
        var withoutPunctuation = NonAlphaNumericRegex().Replace(lowered, " ");
        return MultiWhitespaceRegex().Replace(withoutPunctuation, " ").Trim();
    }

    public static int CountUniqueWords(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return 0;
        }

        return normalizedText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .Count();
    }
}

using System.Text.RegularExpressions;

namespace WikipediaPlaywrightTests.Utilities;

public static partial class ColorParser
{
    [GeneratedRegex(@"\d+(\.\d+)?", RegexOptions.Compiled)]
    private static partial Regex NumberRegex();

    public static double GetBrightness(string cssColor)
    {
        var numbers = NumberRegex()
            .Matches(cssColor)
            .Select(match => double.Parse(match.Value))
            .ToArray();

        if (numbers.Length < 3)
        {
            throw new InvalidOperationException($"Unable to parse the color value '{cssColor}'.");
        }

        return (numbers[0] * 299 + numbers[1] * 587 + numbers[2] * 114) / 1000;
    }
}

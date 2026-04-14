namespace WikipediaPlaywrightTests.Models;

public sealed record SectionWordCountResult(
    string Source,
    string RawText,
    string NormalizedText,
    int UniqueWordCount);

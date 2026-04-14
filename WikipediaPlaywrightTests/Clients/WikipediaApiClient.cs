using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;
using WikipediaPlaywrightTests.Infrastructure;
using WikipediaPlaywrightTests.Models;
using WikipediaPlaywrightTests.Utilities;

namespace WikipediaPlaywrightTests.Clients;

public sealed class WikipediaApiClient
{
    private readonly HttpClient _httpClient;

    public WikipediaApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WikipediaPlaywrightTests/1.0 (automation exercise)");
        _httpClient.DefaultRequestHeaders.Add("Api-User-Agent", "WikipediaPlaywrightTests/1.0 (automation exercise)");
    }

    public async Task<SectionWordCountResult> GetDebuggingFeaturesSectionAsync()
    {
        var sectionsUri = $"{TestSettings.ApiBaseUrl}?action=parse&page=Playwright_(software)&prop=sections&format=json&formatversion=2";
        var sectionsResponse = await _httpClient.GetFromJsonAsync<ParseSectionsResponse>(sectionsUri)
            ?? throw new InvalidOperationException("Unable to parse the sections response.");

        var targetSection = sectionsResponse.Parse.Sections
            .SingleOrDefault(section => string.Equals(section.Line, TestSettings.DebuggingFeaturesHeading, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Section '{TestSettings.DebuggingFeaturesHeading}' was not found via the MediaWiki Parse API.");

        var sectionUri = $"{TestSettings.ApiBaseUrl}?action=parse&page=Playwright_(software)&prop=text&section={targetSection.Index}&format=json&formatversion=2";
        var sectionResponse = await _httpClient.GetFromJsonAsync<ParseTextResponse>(sectionUri)
            ?? throw new InvalidOperationException("Unable to parse the section content response.");

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(sectionResponse.Parse.Text);
        RemoveNodes(htmlDocument, "//*[contains(@class,'mw-heading') or contains(@class,'reflist') or contains(@class,'reference')]");
        var rawText = HtmlEntity.DeEntitize(htmlDocument.DocumentNode.InnerText).Trim();
        var normalizedText = TextNormalizer.Normalize(rawText);

        return new SectionWordCountResult(
            "API",
            rawText,
            normalizedText,
            TextNormalizer.CountUniqueWords(normalizedText));
    }

    private static void RemoveNodes(HtmlDocument document, string xpath)
    {
        var nodes = document.DocumentNode.SelectNodes(xpath);
        if (nodes is null)
        {
            return;
        }

        foreach (var node in nodes)
        {
            node.Remove();
        }
    }

    private sealed class ParseSectionsResponse
    {
        [JsonPropertyName("parse")]
        public required ParseSectionContainer Parse { get; init; }
    }

    private sealed class ParseSectionContainer
    {
        [JsonPropertyName("sections")]
        public required List<ParseSectionRecord> Sections { get; init; }
    }

    private sealed class ParseSectionRecord
    {
        [JsonPropertyName("index")]
        public required string Index { get; init; }

        [JsonPropertyName("line")]
        public required string Line { get; init; }
    }

    private sealed class ParseTextResponse
    {
        [JsonPropertyName("parse")]
        public required ParseTextContainer Parse { get; init; }
    }

    private sealed class ParseTextContainer
    {
        [JsonPropertyName("text")]
        public required string Text { get; init; }
    }
}

using WikipediaPlaywrightTests.Clients;
using WikipediaPlaywrightTests.Infrastructure;
using WikipediaPlaywrightTests.Pages;
using WikipediaPlaywrightTests.Utilities;

namespace WikipediaPlaywrightTests.Tests;

[TestFixture]
public sealed class WikipediaTests : UiTestBase
{
    private static readonly string[] TestingAndDebuggingTechnologyNames =
    [
        "CodeView",
        "OneFuzz",
        "Playwright",
        "Script Debugger",
        "WinDbg",
        "Windows Package Manager",
        "Microsoft Store"
    ];

    [Test]
    public async Task DebuggingFeatures_UiAndApi_ShouldHaveEqualUniqueWordCounts()
    {
        var articlePage = new WikipediaArticlePage(Page);
        await articlePage.OpenAsync();

        using var httpClient = new HttpClient();
        var apiClient = new WikipediaApiClient(httpClient);

        var uiResult = await articlePage.GetDebuggingFeaturesSectionAsync();
        var apiResult = await apiClient.GetDebuggingFeaturesSectionAsync();

        TestContext.WriteLine($"UI normalized text: {uiResult.NormalizedText}");
        TestContext.WriteLine($"API normalized text: {apiResult.NormalizedText}");
        TestContext.WriteLine($"UI unique words: {uiResult.UniqueWordCount}");
        TestContext.WriteLine($"API unique words: {apiResult.UniqueWordCount}");

        Assert.That(uiResult.UniqueWordCount, Is.EqualTo(apiResult.UniqueWordCount),
            "The UI and API unique word counts for the Debugging features section should match after normalization.");
    }

    [TestCaseSource(nameof(TestingAndDebuggingTechnologyNames))]
    public async Task TestingAndDebuggingItemShouldBeALink(string technologyName)
    {
        var articlePage = new WikipediaArticlePage(Page);
        await articlePage.OpenAsync();

        var isLinked = await articlePage.IsTestingAndDebuggingItemLinkedAsync(technologyName);
        TestContext.WriteLine($"{technologyName} linked: {isLinked}");

        Assert.That(isLinked, Is.True,
            $"Expected '{technologyName}' under '{TestSettings.TestingAndDebuggingHeading}' to be a text link.");
    }

    [Test]
    public async Task Appearance_ColorBeta_ShouldSwitchToDarkMode()
    {
        var articlePage = new WikipediaArticlePage(Page);
        await articlePage.OpenAsync();

        var beforeScreenshotPath = Artifacts.GetTimestampedScreenshotPath("DarkMode_Before");
        await Page.ScreenshotAsync(new()
        {
            Path = beforeScreenshotPath,
            FullPage = true
        });
        TestContext.WriteLine($"Before screenshot: {beforeScreenshotPath}");
        TestContext.AddTestAttachment(beforeScreenshotPath, "Dark mode before screenshot");

        var initialThemeState = await articlePage.GetThemeStateAsync();
        await articlePage.SetDarkThemeAsync();
        var updatedThemeState = await articlePage.GetThemeStateAsync();

        var afterScreenshotPath = Artifacts.GetTimestampedScreenshotPath("DarkMode_After");
        await Page.ScreenshotAsync(new()
        {
            Path = afterScreenshotPath,
            FullPage = true
        });
        TestContext.WriteLine($"After screenshot: {afterScreenshotPath}");
        TestContext.AddTestAttachment(afterScreenshotPath, "Dark mode after screenshot");

        TestContext.WriteLine($"Initial theme state: {initialThemeState}");
        TestContext.WriteLine($"Updated theme state: {updatedThemeState}");

        Assert.Multiple(() =>
        {
            Assert.That(updatedThemeState.BackgroundColor, Is.Not.EqualTo(initialThemeState.BackgroundColor),
                "The page background color should change after switching to dark mode.");
            Assert.That(ColorParser.GetBrightness(updatedThemeState.BackgroundColor), Is.LessThan(ColorParser.GetBrightness(initialThemeState.BackgroundColor)),
                "The page background should become darker after switching to dark mode.");
        });
    }
}

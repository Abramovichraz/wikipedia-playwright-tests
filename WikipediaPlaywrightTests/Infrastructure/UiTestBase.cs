using Microsoft.Playwright;

namespace WikipediaPlaywrightTests.Infrastructure;

[Parallelizable(ParallelScope.None)]
public abstract class UiTestBase
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    protected IBrowserContext Context = null!;
    protected IPage Page = null!;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    [SetUp]
    public async Task SetUpAsync()
    {
        Context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1600, Height = 1200 }
        });

        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        var status = TestContext.CurrentContext.Result.Outcome.Status;
        string? screenshotPath = null;

        if (Page is not null)
        {
            screenshotPath = Artifacts.GetFilePath(Path.Combine("screenshots", $"{SanitizeFileName(TestContext.CurrentContext.Test.Name)}.png"));
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });
        }

        if (Context is not null)
        {
            await Context.CloseAsync();
        }

        SimpleHtmlReport.AddResult(new TestResultRecord(
            TestContext.CurrentContext.Test.Name,
            status.ToString(),
            TestContext.CurrentContext.Result.Message,
            GetFailureReason(TestContext.CurrentContext.Result.Message),
            screenshotPath,
            null));
    }

    [OneTimeTearDown]
    public async Task GlobalTearDown()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
        SimpleHtmlReport.WriteReport();
    }

    private static string SanitizeFileName(string value)
    {
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidCharacter, '_');
        }

        return value;
    }

    private static string? GetFailureReason(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var lines = message
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count == 0)
        {
            return null;
        }

        return lines[0];
    }
}

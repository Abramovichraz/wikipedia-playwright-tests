using System.Text.Json;
using Microsoft.Playwright;
using WikipediaPlaywrightTests.Infrastructure;
using WikipediaPlaywrightTests.Models;
using WikipediaPlaywrightTests.Utilities;

namespace WikipediaPlaywrightTests.Pages;

public sealed class WikipediaArticlePage
{
    private readonly IPage _page;

    public WikipediaArticlePage(IPage page)
    {
        _page = page;
    }

    public async Task OpenAsync()
    {
        await _page.GotoAsync(TestSettings.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await _page.Locator("body").WaitForAsync();
    }

    public async Task<SectionWordCountResult> GetDebuggingFeaturesSectionAsync()
    {
        var rawText = await _page.EvaluateAsync<string>(
            @"headingId => {
                const heading = document.getElementById(headingId);

                if (!heading) {
                    throw new Error(`Heading '${headingId}' was not found.`);
                }

                const currentLevel = Number.parseInt(heading.tagName.substring(1), 10);
                const headingContainer = heading.closest('.mw-heading') ?? heading;
                const parts = [];
                let node = headingContainer.nextElementSibling;

                while (node) {
                    if (node.matches('.mw-heading')) {
                        const nextHeading = node.querySelector('h2, h3, h4, h5, h6');
                        if (nextHeading) {
                            const nextLevel = Number.parseInt(nextHeading.tagName.substring(1), 10);
                            if (nextLevel <= currentLevel) {
                                break;
                            }
                        }
                    }

                    parts.push(node.innerText || '');
                    node = node.nextElementSibling;
                }

                return parts.join('\n').trim();
            }",
            "Debugging_features");

        var normalizedText = TextNormalizer.Normalize(rawText);

        return new SectionWordCountResult(
            "UI",
            rawText,
            normalizedText,
            TextNormalizer.CountUniqueWords(normalizedText));
    }

    public async Task<List<string>> GetTestingAndDebuggingItemsWithoutLinksAsync()
    {
        await _page.Locator(".navbox").Last.ScrollIntoViewIfNeededAsync();

        var json = await _page.EvaluateAsync<string>(
            @"() => {
                const navbox = document.querySelector('.navbox[aria-labelledby^=""Microsoft_development_tools""]');

                if (!navbox) {
                    throw new Error('The Microsoft development tools navbox was not found.');
                }

                const testingHeader = Array.from(navbox.querySelectorAll('th.navbox-group'))
                    .find(element => {
                        const text = element.textContent?.replace(/\s+/g, ' ').trim() ?? '';
                        return text.includes('Testing and') && text.includes('debugging');
                    });

                if (!testingHeader) {
                    throw new Error('The Testing and debugging subsection was not found.');
                }

                const cell = testingHeader.parentElement?.querySelector('td.navbox-list');
                if (!cell) {
                    throw new Error('The Testing and debugging subsection body was not found.');
                }

                const itemsWithoutLinks = Array.from(cell.querySelectorAll('li'))
                    .map(item => {
                        const itemText = item.textContent?.replace(/\s+/g, ' ').trim() ?? '';
                        const topLevelAnchors = Array.from(item.querySelectorAll(':scope > a[href]'))
                            .map(anchor => anchor.textContent?.replace(/\s+/g, ' ').trim() ?? '')
                            .filter(Boolean);

                        const isLinked = topLevelAnchors.includes(itemText);
                        return { itemText, isLinked };
                    })
                    .filter(result => result.itemText && !result.isLinked)
                    .map(result => result.itemText);

                return JSON.stringify(itemsWithoutLinks);
            }");

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    public async Task<bool> IsTestingAndDebuggingItemLinkedAsync(string technologyName)
    {
        var itemsWithoutLinks = await GetTestingAndDebuggingItemsWithoutLinksAsync();
        return !itemsWithoutLinks.Contains(technologyName, StringComparer.Ordinal);
    }

    public async Task SetDarkThemeAsync()
    {
        var darkRadio = _page.GetByRole(AriaRole.Radio, new() { Name = TestSettings.DarkThemeLabel, Exact = true });

        if (!await darkRadio.IsVisibleAsync())
        {
            var appearanceButton = _page.GetByRole(AriaRole.Button, new() { Name = "Appearance" });
            if (await appearanceButton.IsVisibleAsync())
            {
                await appearanceButton.ClickAsync();
            }
        }

        await _page.GetByText(TestSettings.ColorSectionHeading, new PageGetByTextOptions { Exact = true }).ScrollIntoViewIfNeededAsync();
        await darkRadio.CheckAsync();
        await _page.WaitForTimeoutAsync(1500);
    }

    public async Task<ThemeState> GetThemeStateAsync()
    {
        return await _page.EvaluateAsync<ThemeState>(
            @"() => {
                const root = document.documentElement;
                const body = document.body;
                const bodyStyle = window.getComputedStyle(body);
                const rootStyle = window.getComputedStyle(root);

                return {
                    backgroundColor: bodyStyle.backgroundColor || rootStyle.backgroundColor,
                    textColor: bodyStyle.color || rootStyle.color,
                    themeAttribute: root.getAttribute('data-mw-theme') ?? root.getAttribute('data-theme') ?? document.body.className
                };
            }");
    }
}

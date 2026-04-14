# Wikipedia Playwright Test Framework

## Test Results (Latest Run)

- ✅ **Task 1 Passed**: UI and MediaWiki API extraction of `Debugging features` produced matching normalized unique word counts.
- ✅ **Task 3 Passed**: `Color (beta)` was switched to `Dark`, and the rendered theme change was successfully validated.
- ⚠️ **Task 2 Partially Passed**: `8/9` technology-name link checks passed under `Microsoft development tools` -> `Testing and debugging`.
- ⚠️ **Single failing case**: `TestingAndDebuggingItemShouldBeALink("Playwright")`
- ⚠️ **Reason**: On the live Wikipedia page, `Playwright` is currently rendered as a self-link without a standard `href` hyperlink.
- ✅ **Assessment**: The failure reflects a real data inconsistency on the live site, not a defect in the automation code.
- ⚠️ **Live site dependency**: This suite validates a live Wikipedia page, so page content and markup may change over time and affect future results.

Lightweight UI + API automation framework built in `C#` with `Playwright` and `NUnit`.

The project validates selected behavior on the Wikipedia page for Playwright software:

- Target page: `https://en.wikipedia.org/wiki/Playwright_(software)`
- Main section under test: `Debugging features`
- UI automation style: `Page Object Model`
- API source: `MediaWiki Parse API`

## Assignment coverage

### Task 1

The framework:

- extracts the `Debugging features` section through the UI
- extracts the same section through the MediaWiki Parse API
- normalizes both texts
- counts unique words
- asserts that both counts are equal

### Task 2

The framework navigates to the `Microsoft development tools` section and validates the technology names under:

- `Testing and debugging`

This task is implemented as **multiple parameterized tests**, one test per technology name, so the report shows exactly which item passed or failed.

### Task 3

The framework changes the Wikipedia appearance setting:

- `Color (beta)` -> `Dark`

Then it validates that the page theme really changed by comparing the rendered page colors before and after the action.

## Visual Validation (Dark Mode)

Screenshots are captured before and after switching to Dark mode to visually validate the UI change.

Before:
![Before](artifacts/screenshots/example_before.png)

After:
![After](artifacts/screenshots/example_after.png)

## Tech stack

- `.NET 8`
- `C#`
- `Microsoft.Playwright`
- `NUnit`
- `HtmlAgilityPack`

## Solution structure

```text
genpact_test/
├── .gitignore
├── README.md
├── WikipediaPlaywrightTests.sln
├── artifacts/
│   ├── report.html
│   └── screenshots/
├── WikipediaPlaywrightTests/
│   ├── Clients/
│   │   └── WikipediaApiClient.cs
│   ├── Infrastructure/
│   │   ├── Artifacts.cs
│   │   ├── SimpleHtmlReport.cs
│   │   ├── TestResultRecord.cs
│   │   ├── TestSettings.cs
│   │   └── UiTestBase.cs
│   ├── Models/
│   │   ├── SectionWordCountResult.cs
│   │   └── ThemeState.cs
│   ├── Pages/
│   │   └── WikipediaArticlePage.cs
│   ├── Tests/
│   │   └── WikipediaTests.cs
│   ├── Utilities/
│   │   ├── ColorParser.cs
│   │   └── TextNormalizer.cs
│   └── WikipediaPlaywrightTests.csproj
```

## Architecture

### `Tests`

Contains the test scenarios only.

- `DebuggingFeatures_UiAndApi_ShouldHaveEqualUniqueWordCounts`
- `TestingAndDebuggingItemShouldBeALink`
- `Appearance_ColorBeta_ShouldSwitchToDarkMode`

### `Pages`

Contains the UI interaction logic for the Wikipedia article page.

- page navigation
- UI text extraction
- technology link validation
- appearance change flow

### `Clients`

Contains API integration logic.

- calls MediaWiki Parse API
- resolves the `Debugging features` section
- strips extra HTML/reference nodes
- returns normalized content for assertions

### `Utilities`

Contains reusable helpers.

- text normalization
- unique word counting
- color brightness comparison

### `Infrastructure`

Contains shared test infrastructure.

- Playwright browser/context lifecycle
- screenshot capture
- simple HTML report generation
- project-wide constants

## Test design notes

### Task 1 normalization strategy

To make UI and API results comparable, the text is normalized by:

- converting to lowercase
- removing reference markers such as `[15]`
- removing punctuation
- collapsing repeated whitespace

After normalization, unique words are counted and compared.

### Task 2 design choice

Task 2 is intentionally implemented as **parameterized tests** instead of a single aggregated test.

Why:

- each technology name appears as a separate test in the runner
- failures are easier to understand
- reporting is cleaner for reviewers

### Task 3 validation strategy

Instead of checking only whether the radio button changed, the test validates the real user-visible result:

- capture page theme state before the change
- switch to dark mode
- compare actual rendered background brightness before and after

## How to run

Important:

- Copy only the command inside each code block.
- Do not copy the Markdown fence such as ````powershell```` or the closing ``` line.
- Do not type `powershell` by itself unless the command explicitly starts with it.

### Prerequisites

Make sure the following are installed:

- `.NET SDK 8`
- internet access to reach Wikipedia and NuGet
- Playwright browser dependencies

### 1. Restore packages

```powershell
dotnet restore WikipediaPlaywrightTests\WikipediaPlaywrightTests.csproj
```

### 2. Build the project

```powershell
dotnet build WikipediaPlaywrightTests\WikipediaPlaywrightTests.csproj
```

### 3. Install Playwright Chromium

Run this after the first build:

```powershell
powershell -ExecutionPolicy Bypass -File WikipediaPlaywrightTests\bin\Debug\net8.0\playwright.ps1 install chromium
```

### 4. Execute all tests

```powershell
dotnet test WikipediaPlaywrightTests\WikipediaPlaywrightTests.csproj --logger "console;verbosity=normal"
```

If you are already inside Windows PowerShell, run only the command above.

Incorrect example:

```text
powershell
dotnet test WikipediaPlaywrightTests\WikipediaPlaywrightTests.csproj --logger "console;verbosity=normal"
```

That starts a new PowerShell session instead of directly running the test command.

Correct example:

```powershell
dotnet test WikipediaPlaywrightTests\WikipediaPlaywrightTests.csproj --logger "console;verbosity=normal"
```

## Expected execution result

Because the project runs against the **live Wikipedia page**, the result depends on the current content.

At the time of implementation, the observed outcome was:

- Task 1 passes
- Task 3 passes
- Task 2 has one real failing case: `Playwright`

The failing Task 2 case is expected with the current live page because under `Microsoft development tools` -> `Testing and debugging`, `Playwright` is rendered as a self-link without a normal `href` link, so the validation correctly fails that item.

### Example expected summary

When the live page structure matches the current implementation, a typical run should end with:

- `Total tests: 9`
- `Passed: 8`
- `Failed: 1`

Expected failing test:

- `TestingAndDebuggingItemShouldBeALink("Playwright")`

Expected failure reason:

- `Expected 'Playwright' under 'Testing and debugging' to be a text link.`

## HTML report

After execution, the framework generates:

- `artifacts/report.html`

The report includes:

- test names
- pass/fail status
- failure message
- screenshot link for each executed test

Open it locally after the run:

```powershell
start artifacts\report.html
```

## Useful commands

Run all tests:

```powershell
dotnet test WikipediaPlaywrightTests\WikipediaPlaywrightTests.csproj
```

Run a single test by filter:

```powershell
dotnet test WikipediaPlaywrightTests\WikipediaPlaywrightTests.csproj --filter "FullyQualifiedName~DebuggingFeatures_UiAndApi_ShouldHaveEqualUniqueWordCounts"
```

Run only Task 2 parameterized tests:

```powershell
dotnet test WikipediaPlaywrightTests\WikipediaPlaywrightTests.csproj --filter "FullyQualifiedName~TestingAndDebuggingItemShouldBeALink"
```

## Assumptions and limitations

- The project validates the current live Wikipedia markup, so markup changes may require selector updates.
- Task 2 uses the rendered navigation box content currently shown on the page.
- The custom HTML report is lightweight by design and is not a third-party reporting framework.
- Screenshots are captured for every test execution to make debugging easier.

## Submission note

This repository is structured as a clean, maintainable automation solution rather than a single test script.  
The main goals were:

- readable test intent
- separation of concerns
- stable reusable helpers
- clear reporting

## Conclusion

The automation framework was implemented successfully for all requested tasks.

- Task 1 is implemented and passes.
- Task 2 is implemented correctly and currently reports one real failure on the live Wikipedia page.
- Task 3 is implemented and passes.

Current observed result on the live page:

- `9` total tests
- `8` passed
- `1` failed

The single failing test is:

- `TestingAndDebuggingItemShouldBeALink("Playwright")`

This failure is expected and valid for the current live Wikipedia content, because under `Microsoft development tools` -> `Testing and debugging`, `Playwright` is not rendered as a standard text link with an `href`, and the task explicitly requires the test to fail in that case.

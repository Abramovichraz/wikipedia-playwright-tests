namespace WikipediaPlaywrightTests.Infrastructure;

public sealed record TestResultRecord(
    string TestName,
    string Status,
    string? Message,
    string? FailureReason,
    string? ScreenshotPath,
    string? TracePath);

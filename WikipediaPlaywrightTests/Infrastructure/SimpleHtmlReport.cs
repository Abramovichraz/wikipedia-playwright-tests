using System.Net;
using System.Text;

namespace WikipediaPlaywrightTests.Infrastructure;

public static class SimpleHtmlReport
{
    private static readonly object Sync = new();
    private static readonly List<TestResultRecord> Results = [];

    public static void AddResult(TestResultRecord result)
    {
        lock (Sync)
        {
            Results.Add(result);
        }
    }

    public static void WriteReport()
    {
        lock (Sync)
        {
            var reportPath = Artifacts.GetFilePath("report.html");
            var html = new StringBuilder();
            var passedCount = Results.Count(result => string.Equals(result.Status, "Passed", StringComparison.OrdinalIgnoreCase));
            var failedResults = Results
                .Where(result => string.Equals(result.Status, "Failed", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var failedCount = failedResults.Count;
            var otherCount = Results.Count - passedCount - failedCount;

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head><meta charset=\"utf-8\" /><title>Wikipedia Playwright Test Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Segoe UI, sans-serif; margin: 32px; background: #f7f7f9; color: #222; }");
            html.AppendLine(".summary { display: flex; gap: 16px; margin: 20px 0 24px; flex-wrap: wrap; }");
            html.AppendLine(".card { background: #fff; border: 1px solid #ddd; border-radius: 10px; padding: 16px 18px; min-width: 140px; }");
            html.AppendLine(".card strong { display: block; font-size: 24px; margin-top: 6px; }");
            html.AppendLine(".failure-list { background: #fff; border: 1px solid #ddd; border-radius: 10px; padding: 18px; margin: 0 0 24px; }");
            html.AppendLine(".failure-list li { margin-bottom: 12px; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; background: #fff; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 12px; text-align: left; vertical-align: top; }");
            html.AppendLine("th { background: #111827; color: #fff; }");
            html.AppendLine(".Passed { color: #166534; font-weight: 700; }");
            html.AppendLine(".Failed { color: #991b1b; font-weight: 700; }");
            html.AppendLine(".Skipped, .Inconclusive { color: #92400e; font-weight: 700; }");
            html.AppendLine("a { color: #1d4ed8; }");
            html.AppendLine("</style></head><body>");
            html.AppendLine($"<h1>Wikipedia Playwright Test Report</h1><p>Generated at {DateTime.UtcNow:u}</p>");
            html.AppendLine("<div class=\"summary\">");
            html.AppendLine($"<div class=\"card\"><span>Total</span><strong>{Results.Count}</strong></div>");
            html.AppendLine($"<div class=\"card\"><span class=\"Passed\">Passed</span><strong>{passedCount}</strong></div>");
            html.AppendLine($"<div class=\"card\"><span class=\"Failed\">Failed</span><strong>{failedCount}</strong></div>");
            html.AppendLine($"<div class=\"card\"><span>Other</span><strong>{otherCount}</strong></div>");
            html.AppendLine("</div>");

            if (failedResults.Count > 0)
            {
                html.AppendLine("<div class=\"failure-list\">");
                html.AppendLine("<h2>Failures And Reasons</h2>");
                html.AppendLine("<ul>");

                foreach (var failedResult in failedResults)
                {
                    html.AppendLine("<li>");
                    html.AppendLine($"<strong>{WebUtility.HtmlEncode(failedResult.TestName)}</strong><br/>");
                    html.AppendLine($"{WebUtility.HtmlEncode(failedResult.FailureReason ?? failedResult.Message ?? "Unknown failure")}");
                    html.AppendLine("</li>");
                }

                html.AppendLine("</ul>");
                html.AppendLine("</div>");
            }

            html.AppendLine("<table><thead><tr><th>Test</th><th>Status</th><th>Message</th><th>Screenshot</th><th>Trace</th></tr></thead><tbody>");

            foreach (var result in Results)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{WebUtility.HtmlEncode(result.TestName)}</td>");
                html.AppendLine($"<td class=\"{WebUtility.HtmlEncode(result.Status)}\">{WebUtility.HtmlEncode(result.Status)}</td>");
                html.AppendLine($"<td>{WebUtility.HtmlEncode(result.FailureReason ?? result.Message ?? string.Empty)}</td>");
                html.AppendLine("<td>" + BuildArtifactLink(result.ScreenshotPath, "Open screenshot") + "</td>");
                html.AppendLine("<td>" + BuildArtifactLink(result.TracePath, "Open trace") + "</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table></body></html>");
            File.WriteAllText(reportPath, html.ToString());
        }
    }

    private static string BuildArtifactLink(string? absolutePath, string label)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath))
        {
            return string.Empty;
        }

        var reportDirectory = Path.GetDirectoryName(Artifacts.GetFilePath("report.html"))!;
        var relativePath = Path.GetRelativePath(reportDirectory, absolutePath).Replace('\\', '/');
        return $"<a href=\"{WebUtility.HtmlEncode(relativePath)}\">{WebUtility.HtmlEncode(label)}</a>";
    }
}

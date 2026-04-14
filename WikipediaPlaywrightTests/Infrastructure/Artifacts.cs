using System.Reflection;

namespace WikipediaPlaywrightTests.Infrastructure;

public static class Artifacts
{
    private static readonly string Root = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
        "..",
        "..",
        "..",
        "..",
        "artifacts");

    public static string EnsureDirectory(string relativePath)
    {
        var directory = Path.GetFullPath(Path.Combine(Root, relativePath));
        Directory.CreateDirectory(directory);
        return directory;
    }

    public static string GetFilePath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(Root, relativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        return fullPath;
    }
}

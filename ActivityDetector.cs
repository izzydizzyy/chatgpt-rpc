using System.Diagnostics;

namespace ChatGPTRpc;

internal sealed record DetectedActivity(string Mode, string Title);

internal static class ActivityDetector
{
    private static readonly string[] BrowserProcesses =
    [
        "chrome", "msedge", "firefox", "brave", "vivaldi", "opera"
    ];

    public static DetectedActivity? Detect()
    {
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var title = process.MainWindowTitle?.Trim();
                if (string.IsNullOrWhiteSpace(title))
                    continue;

                var processName = process.ProcessName;
                var looksLikeBrowser = BrowserProcesses.Contains(processName, StringComparer.OrdinalIgnoreCase);
                var looksLikeOpenAIApp =
                    processName.Contains("chatgpt", StringComparison.OrdinalIgnoreCase) ||
                    processName.Contains("codex", StringComparison.OrdinalIgnoreCase);

                if (!looksLikeBrowser && !looksLikeOpenAIApp)
                    continue;

                if (title.Contains("Codex", StringComparison.OrdinalIgnoreCase))
                    return new DetectedActivity("Codex", CleanTitle(title, "Codex"));

                if (title.Contains("ChatGPT", StringComparison.OrdinalIgnoreCase))
                    return new DetectedActivity("Chat", CleanTitle(title, "ChatGPT"));
            }
            catch
            {
                // Some Windows processes deny access. Ignore them and keep scanning.
            }
            finally
            {
                process.Dispose();
            }
        }

        return null;
    }

    private static string CleanTitle(string title, string appName)
    {
        var cleaned = title
            .Replace(" - ChatGPT", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" — ChatGPT", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" | ChatGPT", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (cleaned.Equals(appName, StringComparison.OrdinalIgnoreCase))
            return "";

        return cleaned.Length > 100 ? cleaned[..100] : cleaned;
    }
}

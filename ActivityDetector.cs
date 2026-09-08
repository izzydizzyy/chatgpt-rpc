using System.Diagnostics;

namespace ChatGPTRpc;

internal sealed record DetectedActivity(string Mode, string Title, string ProcessName);

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
                var browser = BrowserProcesses.Contains(processName, StringComparer.OrdinalIgnoreCase);
                var openAiApp =
                    processName.Contains("chatgpt", StringComparison.OrdinalIgnoreCase) ||
                    processName.Contains("codex", StringComparison.OrdinalIgnoreCase);

                if (!browser && !openAiApp)
                    continue;

                if (title.Contains("Codex", StringComparison.OrdinalIgnoreCase))
                    return new DetectedActivity("Codex", CleanTitle(title, "Codex"), processName);

                if (title.Contains("ChatGPT", StringComparison.OrdinalIgnoreCase))
                    return new DetectedActivity("Chat", CleanTitle(title, "ChatGPT"), processName);
            }
            catch
            {
                // Some Windows processes deny access. Keep scanning.
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
        var cleaned = title;
        var suffixes = new[]
        {
            " - ChatGPT", " — ChatGPT", " | ChatGPT",
            " - Codex", " — Codex", " | Codex",
            " - Google Chrome", " - Microsoft Edge", " — Mozilla Firefox",
            " - Brave", " - Vivaldi", " - Opera"
        };

        foreach (var suffix in suffixes)
        {
            if (cleaned.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned[..^suffix.Length].Trim();
        }

        if (cleaned.Equals(appName, StringComparison.OrdinalIgnoreCase))
            return "";

        return cleaned.Length > 100 ? cleaned[..100] : cleaned;
    }
}

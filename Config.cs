using System.Text.Json;

namespace ChatGPTRpc;

internal sealed class AppConfig
{
    public string ClientId { get; set; } = "";
    public string LargeImageKey { get; set; } = "chatgpt";
    public string LargeImageText { get; set; } = "ChatGPT RPC";
    public bool ShowSessionTitle { get; set; } = true;
    public bool StartWithWindows { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = 2;
}

internal static class ConfigStore
{
    private static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ChatGPT-RPC");

    private static readonly string FilePath = Path.Combine(Folder, "config.json");

    public static string ConfigFolder => Folder;

    public static AppConfig Load()
    {
        try
        {
            Directory.CreateDirectory(Folder);
            if (!File.Exists(FilePath))
                return new AppConfig();

            return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(FilePath)) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public static void Save(AppConfig config)
    {
        Directory.CreateDirectory(Folder);
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}

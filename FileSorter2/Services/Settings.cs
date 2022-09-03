using System.IO;
using System.Text.Json;
using WPF.Common;

namespace FileSorter;

public class Settings : PropertyChangedNotifier
{
    private string _sourceFolder = "";
    private string _targetFoldersFolder = "";
    private bool _askBeforeFileDeletion = true;

    public string SourceFolder
    {
        get => _sourceFolder;
        set => SetProperty(ref _sourceFolder, value);
    }

    public string TargetFoldersFolder
    {
        get => _targetFoldersFolder;
        set => SetProperty(ref _targetFoldersFolder, value);
    }

    public bool AskBeforeFileDeletion
    {
        get => _askBeforeFileDeletion;
        set => SetProperty(ref _askBeforeFileDeletion, value);
    }
}

public static class SettingsReader
{
    public static string GetSettingsFilePath() => Path.Combine(Directory.GetCurrentDirectory(), "settings.json");

    public static readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
    };

    public static Settings? GetSettingsFromFile()
    {
        var filePath = GetSettingsFilePath();
        if (!File.Exists(filePath))
            return null;

        var json = File.ReadAllText(filePath);

        try
        {
            return JsonSerializer.Deserialize<Settings>(json, options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static async Task SafeToDisk(Settings settings)
    {
        var json = JsonSerializer.Serialize(settings, options);
        await File.WriteAllTextAsync(GetSettingsFilePath(), json);
    }
}

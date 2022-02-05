using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WPF.Common;

namespace FileSorter
{
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

        public static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
        };

        public static async Task<Settings?> GetSettingsFromFile()
        {
            var filePath = GetSettingsFilePath();
            if (!File.Exists(filePath))
                return null;

            string json = await File.ReadAllTextAsync(filePath);

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
            string json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(GetSettingsFilePath(), json);
        }
    }
}

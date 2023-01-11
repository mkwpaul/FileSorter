﻿using Serilog;
using System.IO;
using System.Text.Json;

namespace FileSorter;

public class SettingsReader
{
    internal static ILogger _log = null!;
    public static string GetSettingsFilePath() => Path.Combine(Directory.GetCurrentDirectory(), "settings.json");

    public static readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
    };

    public static Settings? GetSettingsFromFile()
    {
        _log.Information("Reading settings from disk...");
        var filePath = GetSettingsFilePath();
        if (!File.Exists(filePath))
        {
            _log.Information("No Settings could be found at path {file}", filePath);
            return null;
        }

        var json = File.ReadAllText(filePath);

        try
        {
            var settings = JsonSerializer.Deserialize<Settings>(json, options);
            _log.Information("Settings loaded from disk. Settings: {settings}", settings);
            return settings;
        }
        catch (JsonException ex)
        {
            _log.Error(ex, "Could not read Settings from {file}. Invalid Json: {json}", filePath, json);
            return null;
        }
    }

    public static async Task SafeToDisk(Settings settings)
    {
        var path = GetSettingsFilePath();
        _log.Information("Writing settings to disk... Settings: {settings}; File: {file}", settings, path);
        var json = JsonSerializer.Serialize(settings, options);
        try
        {
            await File.WriteAllTextAsync(path, json);
        }
        catch (IOException ex)
        {
            _log.Error(ex, "IO Error: Could not write settings to disk. Path: {path}", path);
        }
    }
}

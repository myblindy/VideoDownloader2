using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace VideoDownloader2.Services;

public class SettingsService
{
    static readonly string baseLocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    const string customLocalAppDataPath = "VideoDownloader2";
    static readonly string fullConfigPath;

    static SettingsService()
    {
        var fullConfigDirectory = Path.Combine(baseLocalAppDataPath, customLocalAppDataPath);
        if (!Directory.Exists(fullConfigDirectory))
            Directory.CreateDirectory(fullConfigDirectory);
        fullConfigPath = Path.Combine(fullConfigDirectory, "config.json");
    }

    class JsonModel
    {
        public string? LastDestingationFolder { get; set; }
    }
    JsonModel model;

    public SettingsService()
    {
        ReadSettings();
    }

    public string? LastDestingationFolder
    {
        get => model.LastDestingationFolder;
        set
        {
            model.LastDestingationFolder = value;
            SaveSettings();
        }
    }

    [MemberNotNull(nameof(model))]
    void ReadSettings()
    {
        try
        {
            using var stream = File.OpenRead(fullConfigPath);
            model = JsonSerializer.Deserialize<JsonModel>(stream) ?? new();
        }
        catch
        {
            model = new();
        }
    }

    void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullConfigPath)!);
        }
        catch { }

        try
        {
            using var stream = File.Create(fullConfigPath);
            JsonSerializer.Serialize(new Utf8JsonWriter(stream), model);
        }
        catch
        {
        }
    }
}

using System;
using System.IO;
using System.Text.Json;
using RoboCopy_X.Models;

namespace RoboCopy_X.Services
{
    public class SettingsService
    {
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsPath;

        public SettingsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RoboCopy-X"
            );
            
            Directory.CreateDirectory(appDataPath);
            _settingsPath = Path.Combine(appDataPath, SettingsFileName);
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }

            return new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}

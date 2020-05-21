using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace LOIN.Comments
{
    internal class Settings
    {
        public string LastIFC { get; set; }
        public string LastComments { get; set; }

        public string User { get; set; }

        public void Save()
        {
            var data = JsonSerializer.Serialize<Settings>(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetPath(), data);
        }

        public static Settings Open()
        {
            var path = GetPath();

            if (!File.Exists(path))
                return new Settings();

            var data = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<Settings>(data);
            return settings;
        }

        private static string GetPath()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            var path = Path.Combine(dir, "LOIN.Comments");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return Path.Combine(path, "settings.json");
        }

    }
}

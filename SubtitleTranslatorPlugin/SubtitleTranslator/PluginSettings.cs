using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SubtitleTranslator
{
    [Serializable]
    public class PluginSettings
    {
        public string ProgramPath { get; set; }

        public static PluginSettings Load()
        {
            var settingsPath = GetSettingsPath();

            //MessageBox.Show(settingsPath, "settings path");

            var serializer = new XmlSerializer(typeof(PluginSettings));
            return (PluginSettings)serializer.Deserialize(File.OpenRead(settingsPath));
        }

        public static void Save(PluginSettings settings){
            var settingsPath = GetSettingsPath();

            var serializer = new XmlSerializer(typeof(PluginSettings));
            using(var strm = new FileStream(settingsPath, FileMode.Create, FileAccess.Write)){
            serializer.Serialize(strm, settings);
            }
        }

        private static string GetSettingsPath()
        {
            var asmLoc = typeof(PluginSettings).Assembly.Location;
            var asmDir = Path.GetDirectoryName(asmLoc);
            var pluginsDir = Path.Combine(asmDir, @"Plugins\");
            var xmlFile = Path.Combine(pluginsDir, "SubtitleTranslator.xml");

            if (!Directory.Exists(pluginsDir))
                Directory.CreateDirectory(pluginsDir);

            return xmlFile;
        }
    }
}

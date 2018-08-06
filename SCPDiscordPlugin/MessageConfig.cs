using Newtonsoft.Json.Linq;
using SCPDiscord.Properties;
using YamlDotNet.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Core;

namespace SCPDiscord
{
    class MessageConfig
    {
        private SCPDiscordPlugin plugin;

        public JObject root = null;

        private readonly string languagesPath = FileManager.AppFolder + "SCPDiscord_Languages/";

        // All default languages included in the .dll
        private readonly Dictionary<string, string> defaultLanguages = new Dictionary<string, string>
        {
            { "english", Encoding.UTF8.GetString(Resources.english) }
        };

        public MessageConfig(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
            SaveDefaultLanguages();
            ReadLanguage();
        }

        /// <summary>
        /// Saves all default language files included in the .dll
        /// </summary>
        public void SaveDefaultLanguages()
        {
            plugin.Info("Creating language files...");
            foreach(KeyValuePair<string, string> language in defaultLanguages)
            {
                try
                {
                    File.WriteAllText((languagesPath + language.Key + ".yml"), language.Value);
                }
                catch (DirectoryNotFoundException)
                {
                    DirectoryInfo di = Directory.CreateDirectory(languagesPath);
                    File.WriteAllText((languagesPath + language.Key + ".yml"), language.Value);
                }
            }
        }

        /// <summary>
        /// This function makes me want to die too, don't worry.
        /// 
        /// Parses a yaml file into a yaml object, parses the yaml object into a json string, parses the json string into a json object
        /// </summary>
        public void ReadLanguage()
        {
            try
            {
                // Reads file contents into FileStream
                FileStream stream = File.OpenRead(languagesPath + plugin.GetConfigString("discord_language") + ".yml");

                // Converts the FileStream into a YAML Dictionary object
                var deserializer = new DeserializerBuilder().Build();
                var yamlObject = deserializer.Deserialize(new StreamReader(stream));

                // Converts the YAML Dictionary into JSON String
                var serializer = new SerializerBuilder()
                    .JsonCompatible()
                    .Build();
                string jsonString = serializer.Serialize(yamlObject);

                root = JObject.Parse(jsonString);
                plugin.Info("Successfully loaded language file.");
            }
            catch (Exception e)
            {
                if(e is DirectoryNotFoundException)
                {
                    plugin.Error("Language directory not found.");
                }
                else if (e is UnauthorizedAccessException)
                {
                    plugin.Error("Language file access denied.");
                }
                else if (e is FileNotFoundException)
                {
                    plugin.Error("'" + plugin.GetConfigString("discord_language") + ".yml' was not found.");
                }
                else if(e is JsonReaderException || e is YamlException)
                {
                    plugin.Error("'" + plugin.GetConfigString("discord_language") + ".yml' formatting error.");
                }
                plugin.Error("Error reading language file '" + plugin.GetConfigString("discord_language") + ".yml'. Deactivating plugin...");
                plugin.Debug(e.ToString());
                plugin.Disable();
            }
        }

    }
}

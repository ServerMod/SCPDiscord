using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SCPDiscord.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SCPDiscord
{
    internal class Language
    {
        private readonly SCPDiscordPlugin plugin;

        private JObject primary = null;
        private JObject backup = null;

        private readonly string languagesPath = FileManager.AppFolder + "SCPDiscord_Languages/";

        // All default languages included in the .dll
        private readonly Dictionary<string, string> defaultLanguages = new Dictionary<string, string>
        {
            { "english", Encoding.UTF8.GetString(Resources.english) },
            { "russian", Encoding.UTF8.GetString(Resources.russian) },
            { "french",  Encoding.UTF8.GetString(Resources.french)  }
        };

        public Language(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
            plugin.language = this;

            Thread.Sleep(2500);

            // Save default language files
            SaveDefaultLanguages();

            // Read primary language file
            plugin.Info("Loading primary language file...");
            try
            {
                ReadLanguage(plugin.GetConfigString("discord_language"), false);
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException)
                {
                    plugin.Error("Language directory not found.");
                }
                else if (e is UnauthorizedAccessException)
                {
                    plugin.Error("Primary language file access denied.");
                }
                else if (e is FileNotFoundException)
                {
                    plugin.Error("'" + plugin.GetConfigString("discord_language") + ".yml' was not found.");
                }
                else if (e is JsonReaderException || e is YamlException)
                {
                    plugin.Error("'" + plugin.GetConfigString("discord_language") + ".yml' formatting error.");
                }
                plugin.Error("Error reading language file '" + plugin.GetConfigString("discord_language") + ".yml'. Attempting to initialize backup system...");
                plugin.Debug(e.ToString());
            }

            // Read backup language file if not the same as the primary
            if (plugin.GetConfigString("discord_language") != "english")
            {
                plugin.Info("Loading backup language file...");
                try
                {
                    ReadLanguage("english", true);
                }
                catch (Exception e)
                {
                    if (e is DirectoryNotFoundException)
                    {
                        plugin.Error("Language directory not found.");
                    }
                    else if (e is UnauthorizedAccessException)
                    {
                        plugin.Error("Backup language file access denied.");
                    }
                    else if (e is FileNotFoundException)
                    {
                        plugin.Error("'" + plugin.GetConfigString("discord_language") + ".yml' was not found.");
                    }
                    else if (e is JsonReaderException || e is YamlException)
                    {
                        plugin.Error("'" + plugin.GetConfigString("discord_language") + ".yml' formatting error.");
                    }
                    plugin.Error("Error reading backup language file 'english.yml'.");
                    plugin.Debug(e.ToString());
                }
            }
            if(primary == null && backup == null)
            {
                plugin.Error("NO LANGUAGE FILE LOADED! DEACTIVATING SCPDISCORD.");
                plugin.Disable();
            }

            //Runs until the server has connected once
            Thread connectionThread = new Thread(new ThreadStart(() => new ConnectToBot(plugin)));
            connectionThread.Start();

            //Runs the listener
            Thread botListenerThread = new Thread(new ThreadStart(() => new BotListener(plugin)));
            botListenerThread.Start();

            //Keeps running to auto-reconnect if needed
            Thread watchdogThread = new Thread(new ThreadStart(() => new StartConnectionWatchdog(plugin)));
            watchdogThread.Start();
        }

        /// <summary>
        /// Saves all default language files included in the .dll
        /// </summary>
        public void SaveDefaultLanguages()
        {
            plugin.Info("Creating language files...");
            foreach (KeyValuePair<string, string> language in defaultLanguages)
            {
                try
                {
                    File.WriteAllText((languagesPath + language.Key + ".yml"), language.Value);
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(languagesPath);
                    File.WriteAllText((languagesPath + language.Key + ".yml"), language.Value);
                }
            }
        }

        /// <summary>
        /// This function makes me want to die too, don't worry.
        /// Parses a yaml file into a yaml object, parses the yaml object into a json string, parses the json string into a json object
        /// </summary>
        public void ReadLanguage(string language, bool isBackup)
        {
            // Reads file contents into FileStream
            FileStream stream = File.OpenRead(languagesPath + language + ".yml");

            // Converts the FileStream into a YAML Dictionary object
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(new StreamReader(stream));

            // Converts the YAML Dictionary into JSON String
            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();
            string jsonString = serializer.Serialize(yamlObject);

            string identifier = "";
            if(isBackup)
            {
                identifier = "backup";
                backup = JObject.Parse(jsonString);
            }
            else
            {
                identifier = "primary";
                primary = JObject.Parse(jsonString);
            }
            plugin.Info("Successfully loaded " + identifier + " language file '" + language + "'.");
        }

        /// <summary>
        /// Gets a string from the primary or backup language file
        /// </summary>
        /// <param name="path">The path to the node</param>
        /// <returns></returns>
        public string GetString(string path)
        {
            if (primary == null && backup == null)
            {
                throw new ArgumentNullException();
            }
            try
            {
                return plugin.language.primary.SelectToken(path).Value<string>();
            }
            // This exception means the node does not exist in the language file, the plugin attempts to find it in the backup file
            catch (Exception primaryException)
            {
                if(primaryException is NullReferenceException || primaryException is ArgumentNullException || primaryException is InvalidCastException || primaryException is JsonException)
                {
                    plugin.Warn("Error reading dictionary '" + path + "' from primary language file, switching to backup...");
                    try
                    {
                        return plugin.language.backup.SelectToken(path).Value<string>();
                    }
                    // The node also does not exist in the backup file
                    catch (NullReferenceException e)
                    {
                        plugin.Error("Error: Language config node '" + path + "' does not exist. Message can not be sent." + e);
                        throw;
                    }
                    catch (ArgumentNullException e)
                    {
                        plugin.Error("Error: Language config node '" + path + "' does not exist. Message can not be sent." + e);
                        throw;
                    }
                    catch (InvalidCastException e)
                    {
                        plugin.Error(e.ToString());
                        throw;
                    }
                    catch (JsonException e)
                    {
                        plugin.Error(e.ToString());
                        throw;
                    }
                }
                else
                {
                    plugin.Error(primaryException.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets a regex disctionary from the primary or backup language file
        /// </summary>
        /// <param name="path">The path to the node</param>
        /// <returns></returns>
        public Dictionary<string,string> GetRegexDictionary(string path)
        {
            if (primary == null && backup == null)
            {
                throw new NullReferenceException();
            }
            try
            {
                JArray jsonArray = plugin.language.primary.SelectToken(path).Value<JArray>();
                return jsonArray.ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());
            }
            // This exception means the node does not exist in the language file, the plugin attempts to find it in the backup file
            catch (Exception primaryException)
            {
                if(primaryException is NullReferenceException || primaryException is InvalidCastException || primaryException is JsonException)
                {
                    plugin.Warn("Error reading dictionary '" + path + "' from primary language file, switching to backup...");
                    try
                    {
                        return plugin.language.backup.SelectToken(path).Value<JArray>().ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());
                    }
                    // The node exists but the array is empty
                    catch (ArgumentNullException)
                    {
                        return new Dictionary<string, string>();
                    }
                    // The node also does not exist in the backup file
                    catch (NullReferenceException e)
                    {
                        plugin.Error("Error: Language config node '" + path + "' does not exist. Message can not be sent. " + e);
                        throw;
                    }
                    catch (InvalidCastException e)
                    {
                        plugin.Error(e.ToString());
                        throw;
                    }
                    catch (JsonException e)
                    {
                        plugin.Error(e.ToString());
                        throw;
                    }
                }
                else if(primaryException is ArgumentNullException)
                {
                    return new Dictionary<string, string>();
                }
                else
                {
                    plugin.Error(primaryException.ToString());
                    throw;
                }
            }
        }
    }
}
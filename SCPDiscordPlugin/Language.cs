using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SCPDiscord.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SCPDiscord
{
    internal static class Language
    {
        private static SCPDiscord plugin;
        public static bool ready = false;

        private static JObject primary = null;
        private static JObject backup = null;

        private static readonly string languagesPath = FileManager.GetAppFolder() + "SCPDiscord/Languages/";

        // All default languages included in the .dll
        private static readonly Dictionary<string, string> defaultLanguages = new Dictionary<string, string>
        {
            { "english",        Encoding.UTF8.GetString(Resources.english)      },
            { "russian",        Encoding.UTF8.GetString(Resources.russian)      },
            { "french",         Encoding.UTF8.GetString(Resources.french)       },
            { "polish",         Encoding.UTF8.GetString(Resources.polish)       },
            { "italian",        Encoding.UTF8.GetString(Resources.italian)      },
            { "englishemote",   Encoding.UTF8.GetString(Resources.englishemote) },
            { "russianemote",   Encoding.UTF8.GetString(Resources.russianemote) },
            { "frenchemote",    Encoding.UTF8.GetString(Resources.frenchemote)  }
        };

        private static readonly List<string> messageNodes = new List<string>
        {
            "round.onroundstart",
            "round.onconnect",
            "round.ondisconnect.default",
            "round.ondisconnect.banned",
            "round.oncheckroundend",
            "round.onroundend",
            "round.onwaitingforplayers",
            "round.onroundrestart",
            "round.onsetservername",
            "round.onscenechanged",

            "environment.onscp914activate",
            "environment.onstartcountdown.noplayer",
            "environment.onstartcountdown.resumed",
            "environment.onstartcountdown.initiated",
            "environment.onstopcountdown.default",
            "environment.onstopcountdown.noplayer",
            "environment.ondetonate",
            "environment.ondecontaminate",
            "environment.onsummonvehicle.chaos",
            "environment.onsummonvehicle.mtf",

            "player.onplayerhurt.noattacker",
            "player.onplayerhurt.friendlyfire",
            "player.onplayerhurt.default",
            "player.onplayerdie.nokiller",
            "player.onplayerdie.friendlyfire",
            "player.onplayerdie.default",
            "player.onplayerpickupitem",
            "player.onplayerdropitem",
            "player.onplayerjoin",
            "player.onnicknameset",
            "player.onassignteam",
            "player.onsetrole",
            "player.oncheckescape.allowed",
            "player.oncheckescape.denied",
            "player.onspawn",
            "player.ondooraccess.allowed",
            "player.ondooraccess.denied",
            "player.onintercom",
            "player.onintercomcooldowncheck",
            "player.onpocketdimensionexit",
            "player.onpocketdimensionenter",
            "player.onpocketdimensiondie",
            "player.onthrowgrenade",
            "player.onplayerinfected",
            "player.onspawnragdoll",
            "player.onlure",
            "player.oncontain106",
            "player.onmedkituse",
            "player.onshoot.notarget",
            "player.onshoot.friendlyfire",
            "player.onshoot.default",
            "player.on106createportal",
            "player.on106teleport",
            "player.onelevatoruse",
            "player.onhandcuff.default",
            "player.onhandcuff.nootherplayer",
            "player.onplayertriggertesla.default",
            "player.onplayertriggertesla.ignored",
            "player.onscp914changeknob",
            "player.onplayerradioswitch",
            "player.onmakenoise",
            "player.onrecallzombie",
            "player.oncallcommand",
            "player.onreload",
            "player.ongrenadeexplosion",
            "player.ongrenadehitplayer",

            "admin.onadminquery",
            "admin.onauthcheck",
            "admin.onban.admin.kick",
            "admin.onban.console.kick",
            "admin.onban.admin.ban",
            "admin.onban.console.ban",

            "team.ondecideteamrespawnqueue",
            "team.onteamrespawn.mtf",
            "team.onteamrespawn.ci",
            "team.onsetrolemaxhp",
            "team.onsetscpconfig",
            "team.onsetntfunitname",

            "botmessages.connectedtobot",
            "botmessages.reconnectedtobot",

            "botresponses.missingarguments",
            "botresponses.invalidsteamid",
            "botresponses.invalidduration",
            "botresponses.playerbanned",
            "botresponses.consolecommandfeedback",
            "botresponses.invalidsteamidorip",
            "botresponses.playerunbanned",
            "botresponses.playerkicked",
            "botresponses.playernotfound",
            "botresponses.exit",
            "botresponses.help",
            "botresponses.kickall",
            "botresponses.toggletag.notinstalled",
            "botresponses.vpnshield.notinstalled",

            "topic"
        };

        public static void Reload()
        {
            ready = false;
            plugin = SCPDiscord.plugin;

            // Save default language files
            SaveDefaultLanguages();

            // Read primary language file
            plugin.Info("Loading primary language file...");
            try
            {
                LoadLanguageFile(Config.GetString("settings.language"), false);
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
                    plugin.Error("'" + Config.GetString("settings.language") + ".yml' was not found.");
                }
                else if (e is JsonReaderException || e is YamlException)
                {
                    plugin.Error("'" + Config.GetString("settings.language") + ".yml' formatting error.");
                }
                plugin.Error("Error reading language file '" + Config.GetString("settings.language") + ".yml'. Attempting to initialize backup system...");
                plugin.Debug(e.ToString());
            }

            // Read backup language file if not the same as the primary
            if (Config.GetString("settings.language") != "english")
            {
                plugin.Info("Loading backup language file...");
                try
                {
                    LoadLanguageFile("english", true);
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
                        plugin.Error("'" + Config.GetString("settings.language") + ".yml' was not found.");
                    }
                    else if (e is JsonReaderException || e is YamlException)
                    {
                        plugin.Error("'" + Config.GetString("settings.language") + ".yml' formatting error.");
                    }
                    plugin.Error("Error reading backup language file 'english.yml'.");
                    plugin.Debug(e.ToString());
                }
            }
            if (primary == null && backup == null)
            {
                plugin.Error("NO LANGUAGE FILE LOADED! DEACTIVATING SCPDISCORD.");
                plugin.Disable();
            }

            ValidateLanguageStrings();

            ready = true;
        }

        /// <summary>
        /// Saves all default language files included in the .dll
        /// </summary>
        public static void SaveDefaultLanguages()
        {
            foreach (KeyValuePair<string, string> language in defaultLanguages)
            {
                if (!File.Exists(languagesPath + language.Key + ".yml"))
                {
                    plugin.Info("Creating language file " + language.Key + ".yml...");
                    try
                    {
                        File.WriteAllText((languagesPath + language.Key + ".yml"), language.Value);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        plugin.Warn("Could not create language file: Language directory does not exist, attempting to create it... ");
                        Directory.CreateDirectory(languagesPath);
                        plugin.Info("Creating language file " + language.Key + ".yml...");
                        File.WriteAllText((languagesPath + language.Key + ".yml"), language.Value);
                    }
                }
            }
        }

        /// <summary>
        /// This function makes me want to die too, don't worry.
        /// Parses a yaml file into a yaml object, parses the yaml object into a json string, parses the json string into a json object
        /// </summary>
        public static void LoadLanguageFile(string language, bool isBackup)
        {
            // Reads file contents into FileStream
            FileStream stream = File.OpenRead(languagesPath + language + ".yml");

            // Converts the FileStream into a YAML Dictionary object
            IDeserializer deserializer = new DeserializerBuilder().Build();
            object yamlObject = deserializer.Deserialize(new StreamReader(stream));

            // Converts the YAML Dictionary into JSON String
            ISerializer serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();
            string jsonString = serializer.Serialize(yamlObject);

            string identifier = "";
            if (isBackup)
            {
                identifier = "backup";
                backup = JObject.Parse(jsonString);
            }
            else
            {
                identifier = "primary";
                primary = JObject.Parse(jsonString);
            }
            plugin.Info("Successfully loaded " + identifier + " language file '" + language + ".yml'.");
        }

        public static void ValidateLanguageStrings()
        {
            foreach (string node in messageNodes)
            {
                try
                {
                    Language.primary.SelectToken(node + ".message").Value<string>();
                }
                catch (Exception)
                {
                    plugin.Warn("Your SCPDiscord language file \"" + Config.GetString("settings.language") + ".yml\" does not contain the node \"" + node + ".message\".\nEither add it to your language file or turn on the discord_overwrite_language config setting to use the default language.");
                }
            }
        }

        /// <summary>
        /// Gets a string from the primary or backup language file
        /// </summary>
        /// <param name="path">The path to the node</param>
        /// <returns></returns>
        public static string GetString(string path)
        {
            if (primary == null && backup == null)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Warn("Tried to send Discord message before loading languages.");
                }
                return null;
            }
            try
            {
                return Language.primary.SelectToken(path).Value<string>();
            }
            catch (Exception primaryException)
            {
                // This exception means the node does not exist in the language file, the plugin attempts to find it in the backup file
                if (primaryException is NullReferenceException || primaryException is ArgumentNullException || primaryException is InvalidCastException || primaryException is JsonException)
                {
                    plugin.Warn("Error reading string '" + path + "' from primary language file, switching to backup...");
                    try
                    {
                        return Language.backup.SelectToken(path).Value<string>();
                    }
                    // The node also does not exist in the backup file
                    catch (NullReferenceException e)
                    {
                        plugin.Error("Error: Language language string '" + path + "' does not exist. Message can not be sent." + e);
                        return null;
                    }
                    catch (ArgumentNullException e)
                    {
                        plugin.Error("Error: Language language string '" + path + "' does not exist. Message can not be sent." + e);
                        return null;
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
        public static Dictionary<string, string> GetRegexDictionary(string path)
        {
            if (primary == null && backup == null)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Warn("Tried to read regex dictionary before loading languages.");
                }
                return new Dictionary<string, string>();
            }
            try
            {
                JArray jsonArray = Language.primary.SelectToken(path).Value<JArray>();
                return jsonArray.ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());
            }
            catch (NullReferenceException e)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Warn("Error: Language regex dictionary '" + path + "' does not exist." + e);
                }
                return new Dictionary<string, string>();
            }
            catch (ArgumentNullException)
            {
                // Regex array is empty
                return new Dictionary<string, string>();
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
    }
}
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace SCPDiscord
{
    public static class Config
    {
        public static bool ready;

        private static readonly Dictionary<string, string> configStrings = new Dictionary<string, string>
        {
            { "bot.ip",             "127.0.0.1" },

            { "settings.language",  "english"   },
            { "settings.timestamp", ""          }
        };

        private static readonly Dictionary<string, bool> configBools = new Dictionary<string, bool>
        {
            { "settings.playercount",       true    },
            { "settings.verbose",           true    },
            { "settings.debug",             false   },
            { "settings.metrics",           true    },
            { "settings.configvalidation",  true    }
        };

        private static readonly Dictionary<string, int> configInts = new Dictionary<string, int>
        {
            { "bot.port", 8888 }
        };

        private static readonly Dictionary<string, string[]> configArrays = new Dictionary<string, string[]>
        {
            // Bot messages
            { "channels.statusmessages",            new string[]{ } },
            { "channels.topic",                     new string[]{ } },

            // Round events
            { "channels.onroundstart",              new string[]{ } },
            { "channels.onconnect",                 new string[]{ } },
            { "channels.ondisconnect.default",      new string[]{ } },
            { "channels.ondisconnect.banned",       new string[]{ } },
            { "channels.onlatedisconnect",          new string[]{ } },
            { "channels.oncheckroundend",           new string[]{ } },
            { "channels.onroundend",                new string[]{ } },
            { "channels.onwaitingforplayers",       new string[]{ } },
            { "channels.onroundrestart",            new string[]{ } },
            { "channels.onsetservername",           new string[]{ } },
            { "channels.onscenechanged",            new string[]{ } },

            // Environment events
            { "channels.onscp914activate",          new string[]{ } },
            { "channels.onstartcountdown.noplayer", new string[]{ } },
            { "channels.onstartcountdown.initiated",new string[]{ } },
            { "channels.onstartcountdown.resumed",  new string[]{ } },
            { "channels.onstopcountdown.default",   new string[]{ } },
            { "channels.onstopcountdown.noplayer",  new string[]{ } },
            { "channels.ondetonate",                new string[]{ } },
            { "channels.ondecontaminate",           new string[]{ } },
            { "channels.onsummonvehicle.chaos",     new string[]{ } },
            { "channels.onsummonvehicle.mtf",       new string[]{ } },

            // Player events
            { "channels.onplayerhurt.default",      new string[]{ } },
            { "channels.onplayerhurt.friendlyfire", new string[]{ } },
            { "channels.onplayerhurt.noattacker",   new string[]{ } },
            { "channels.onplayerdie.default",       new string[]{ } },
            { "channels.onplayerdie.friendlyfire",  new string[]{ } },
            { "channels.onplayerdie.nokiller",      new string[]{ } },
            { "channels.onplayerpickupitem",        new string[]{ } },
            { "channels.onplayerdropitem",          new string[]{ } },
            { "channels.onplayerjoin",              new string[]{ } },
            { "channels.onnicknameset",             new string[]{ } },
            { "channels.onassignteam",              new string[]{ } },
            { "channels.onsetrole",                 new string[]{ } },
            { "channels.oncheckescape.allowed",     new string[]{ } },
            { "channels.oncheckescape.denied",      new string[]{ } },
            { "channels.onspawn",                   new string[]{ } },
            { "channels.ondooraccess.allowed",      new string[]{ } },
            { "channels.ondooraccess.denied",       new string[]{ } },
            { "channels.onintercom",                new string[]{ } },
            { "channels.onintercomcooldowncheck",   new string[]{ } },
            { "channels.onpocketdimensionexit",     new string[]{ } },
            { "channels.onpocketdimensionenter",    new string[]{ } },
            { "channels.onpocketdimensiondie",      new string[]{ } },
            { "channels.onthrowgrenade",            new string[]{ } },
            { "channels.onplayerinfected",          new string[]{ } },
            { "channels.onspawnragdoll",            new string[]{ } },
            { "channels.onlure",                    new string[]{ } },
            { "channels.oncontain106",              new string[]{ } },
            { "channels.onmedkituse",               new string[]{ } },
            { "channels.onshoot.default",           new string[]{ } },
            { "channels.onshoot.friendlyfire",      new string[]{ } },
            { "channels.onshoot.notarget",          new string[]{ } },
            { "channels.on106createportal",         new string[]{ } },
            { "channels.on106teleport",             new string[]{ } },
            { "channels.onelevatoruse",             new string[]{ } },
            { "channels.onhandcuff.default",        new string[]{ } },
            { "channels.onhandcuff.nootherplayer",  new string[]{ } },
            { "channels.onplayertriggertesla.default",  new string[]{ } },
            { "channels.onplayertriggertesla.ignored",  new string[]{ } },
            { "channels.onscp914changeknob",        new string[]{ } },
            { "channels.onplayerradioswitch",       new string[]{ } },
            { "channels.onmakenoise",               new string[]{ } },
            { "channels.onrecallzombie",            new string[]{ } },
            { "channels.oncallcommand",             new string[]{ } },
            { "channels.onreload",                  new string[]{ } },
            { "channels.ongrenadeexplosion",        new string[]{ } },
            { "channels.ongrenadehitplayer",        new string[]{ } },
            { "channels.ongeneratorunlock",         new string[]{ } },
            { "channels.ongeneratoraccess.closed",  new string[]{ } },
            { "channels.ongeneratoraccess.opened",  new string[]{ } },
            { "channels.ongeneratorinserttablet",   new string[]{ } },
            { "channels.ongeneratorejecttablet",    new string[]{ } },
            { "channels.on079door.closed",          new string[]{ } },
            { "channels.on079door.opened",          new string[]{ } },
            { "channels.on079lock.unlocked",        new string[]{ } },
            { "channels.on079lock.locked",          new string[]{ } },
            { "channels.on079elevator.up",          new string[]{ } },
            { "channels.on079elevator.down",        new string[]{ } },
            { "channels.on079teslagate",            new string[]{ } },
            { "channels.on079addexp",               new string[]{ } },
            { "channels.on079levelup",              new string[]{ } },
            { "channels.on079unlockdoors",          new string[]{ } },
            { "channels.on079camerateleport",       new string[]{ } },
            { "channels.on079startspeaker",         new string[]{ } },
            { "channels.on079stopspeaker",          new string[]{ } },
            { "channels.on079lockdown",             new string[]{ } },
            { "channels.on079elevatorteleport",     new string[]{ } },

            // Admin events
            { "channels.onadminquery",              new string[]{ } },
            { "channels.onauthcheck",               new string[]{ } },
            { "channels.onban.admin.kick",          new string[]{ } },
            { "channels.onban.console.kick",        new string[]{ } },
            { "channels.onban.admin.ban",           new string[]{ } },
            { "channels.onban.console.ban",         new string[]{ } },

            // Team events
            { "channels.ondecideteamrespawnqueue",  new string[]{ } },
            { "channels.onsetrolemaxhp",            new string[]{ } },
            { "channels.onteamrespawn.mtf",         new string[]{ } },
            { "channels.onteamrespawn.ci",          new string[]{ } },
            { "channels.onsetscpconfig",            new string[]{ } },
            { "channels.onsetntfunitname",          new string[]{ } },
        };

        private static readonly Dictionary<string, Dictionary<string, string>> configDicts = new Dictionary<string, Dictionary<string, string>>
        {
            { "aliases", new Dictionary<string, string>() }
        };

        internal static void Reload(SCPDiscord plugin)
        {
            ready = false;
            plugin.SetUpFileSystem();

            // Reads file contents into FileStream
            FileStream stream = File.OpenRead(FileManager.GetAppFolder(plugin.GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml");

            // Converts the FileStream into a YAML Dictionary object
            IDeserializer deserializer = new DeserializerBuilder().Build();
            object yamlObject = deserializer.Deserialize(new StreamReader(stream));

            // Converts the YAML Dictionary into JSON String
            ISerializer serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();
            string jsonString = serializer.Serialize(yamlObject);

            JObject json = JObject.Parse(jsonString);

            // Reads the configvalidation node first as it is used for reading the others
            try
            {
                configBools["settings.configvalidation"] = json.SelectToken("settings.configvalidation").Value<bool>();
            }
            catch (ArgumentNullException)
            {
                if (GetBool("settings.configvalidation"))
                {
                    plugin.Warn("Config bool 'settings.configvalidation' not found, using default value: true");
                }
            }

            // Read config strings
            foreach (KeyValuePair<string, string> node in configStrings.ToList())
            {
                try
                {
                    configStrings[node.Key] = json.SelectToken(node.Key).Value<string>();
                }
                catch (ArgumentNullException)
                {
                    if (GetBool("settings.configvalidation"))
                    {
                        plugin.Warn("Config string '" + node.Key + "' not found, using default value: \"" + node.Value + "\"");
                    }
                }
            }

            // Read config ints
            foreach (KeyValuePair<string, int> node in configInts.ToList())
            {
                try
                {
                    configInts[node.Key] = json.SelectToken(node.Key).Value<int>();
                }
                catch (ArgumentNullException)
                {
                    if (GetBool("settings.configvalidation"))
                    {
                        plugin.Warn("Config int '" + node.Key + "' not found, using default value: \"" + node.Value + "\"");
                    }
                }
            }

            // Read config bools
            foreach (KeyValuePair<string, bool> node in configBools.ToList().Where(kvm => kvm.Key != "settings.configvalidation"))
            {
                try
                {
                    configBools[node.Key] = json.SelectToken(node.Key).Value<bool>();
                }
                catch (ArgumentNullException)
                {
                    if (GetBool("settings.configvalidation"))
                    {
                        plugin.Warn("Config bool '" + node.Key + "' not found, using default value: " + node.Value);
                    }
                }
            }

            // Read config arrays
            foreach (KeyValuePair<string, string[]> node in configArrays.ToList())
            {
                try
                {
                    configArrays[node.Key] = json.SelectToken(node.Key).Value<JArray>().Values<string>().ToArray();
                }
                catch (ArgumentNullException)
                {
                    if (GetBool("settings.configvalidation"))
                    {
                        plugin.Warn("Config array '" + node.Key + "' not found, using default value: []");
                    }
                }
            }

            // Read config dictionaries
            foreach (KeyValuePair<string, Dictionary<string, string>> node in configDicts.ToList())
            {
                try
                {
                    configDicts[node.Key] = json.SelectToken(node.Key).Value<JArray>().ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());
                }
                catch (ArgumentNullException)
                {
                    if (GetBool("settings.configvalidation"))
                    {
                        plugin.Warn("Config dictionary '" + node.Key + "' not found, using default value: []");
                    }
                }
            }

            if (GetBool("settings.configvalidation"))
            {
                ValidateConfig(plugin);
            }

            ready = true;
        }

        public static bool GetBool(string node)
        {
            return configBools[node];
        }

        public static string GetString(string node)
        {
            return configStrings[node];
        }

        public static int GetInt(string node)
        {
            return configInts[node];
        }

        public static string[] GetArray(string node)
        {
            return configArrays[node];
        }

        public static Dictionary<string, string> GetDict(string node)
        {
            return configDicts[node];
        }

        public static void SetBool(string key, bool value)
        {
            configBools[key] = value;
        }

        public static void SetString(string key, string value)
        {
            configStrings[key] = value;
        }

        public static void SetInt(string key, int value)
        {
            configInts[key] = value;
        }

        public static void SetArray(string key, string[] value)
        {
            configArrays[key] = value;
        }

        public static void SetDict(string key, Dictionary<string, string> value)
        {
            configDicts[key] = value;
        }

        public static void ValidateConfig(SCPDiscord plugin)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n||||||||||||| SCPDiscord config validator ||||||||||||||\n");
            sb.Append("------------ Config strings ------------\n");
            foreach (KeyValuePair<string, string> node in configStrings)
            {
                sb.Append(node.Key + ": " + node.Value + "\n");
            }

            sb.Append("------------ Config ints ------------\n");
            foreach (KeyValuePair<string, bool> node in configBools)
            {
                sb.Append(node.Key + ": " + node.Value + "\n");
            }

            sb.Append("------------ Config bools ------------\n");
            foreach (KeyValuePair<string, bool> node in configBools)
            {
                sb.Append(node.Key + ": " + node.Value + "\n");
            }

            sb.Append("------------ Config arrays ------------\n");
            foreach (KeyValuePair<string, string[]> node in configArrays)
            {
                sb.Append(node.Key + ": [ " + string.Join(", ", node.Value) + " ]\n");
                if (node.Key.StartsWith("channels."))
                {
                    foreach (string s in node.Value)
                    {
                        if (!GetDict("aliases").ContainsKey(s))
                        {
                            sb.Append("WARNING: Channel alias '" + s + "' does not exist!\n");
                        }
                    }
                }
            }

            sb.Append("------------ Config dictionaries ------------\n");
            foreach (KeyValuePair<string, Dictionary<string, string>> node in configDicts)
            {
                sb.Append(node.Key + ":\n");
                foreach (KeyValuePair<string, string> subNode in node.Value)
                {
                    if (!Regex.IsMatch(subNode.Value, @"^\d+$"))
                    {
                        sb.Append("WARNING: Invalid channel ID: " + subNode.Value + "!\n");
                    }
                    sb.Append("    " + subNode.Key + ": " + subNode.Value + "\n");
                }
            }
            sb.Append("|||||||||||| End of config validation ||||||||||||");
            plugin.Info(sb.ToString());
        }
    }
}
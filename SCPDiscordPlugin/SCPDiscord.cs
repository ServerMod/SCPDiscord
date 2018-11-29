using Smod2;
using Smod2.Attributes;
using Smod2.Commands;

using System.Net.Sockets;
using System;

using System.Threading;
using System.IO;
using System.Collections.Generic;
using Smod2.Events;
using SCPDiscord.Properties;
using System.Diagnostics;
using System.Text;
using YamlDotNet.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using YamlDotNet.Core;

namespace SCPDiscord
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "SCPDiscord",
        description = "SCP:SL - Discord bridge.",
        id = "karlofduty.scpdiscord",
        version = "0.4.0",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 22
    )]
    public class SCPDiscord : Plugin
    {
        public Stopwatch serverStartTime = new Stopwatch();

        internal static SCPDiscord plugin;

        public RoleSync roleSync;

        public bool shutdown = false;

        public override void Register()
        {
            // Event handlers
            this.AddEventHandlers(new RoundEventListener(this), Priority.Highest);
            this.AddEventHandlers(new PlayerEventListener(this), Priority.Highest);
            this.AddEventHandlers(new AdminEventListener(this), Priority.Highest);
            this.AddEventHandlers(new EnvironmentEventListener(this), Priority.Highest);
            this.AddEventHandlers(new TeamEventListener(this), Priority.Highest);
            this.AddEventHandlers(new TickCounter(), Priority.Highest);

            this.AddConfig(new Smod2.Config.ConfigSetting("max_players", "20", Smod2.Config.SettingType.STRING, true, "Gets the max players without reserved slots."));

            this.AddConfig(new Smod2.Config.ConfigSetting("scpdiscord_config", "config.yml", Smod2.Config.SettingType.STRING, true, "Name of the config file to use, by default 'config.yml'"));
        }

        class ReconnectCommand : ICommandHandler
        {
            private SCPDiscord plugin;
            public ReconnectCommand(SCPDiscord plugin)
            {
                this.plugin = plugin;
            }

            public string GetCommandDescription()
            {
                return "Attempts to close the connection to the Discord bot and reconnect.";
            }

            public string GetUsage()
            {
                return "scpd_rc/scpd_reconnect";
            }

            public string[] OnCall(ICommandSender sender, string[] args)
            {
                if(NetworkSystem.IsConnected())
                {
                    NetworkSystem.Disconnect();
                    return new string[] { "Connection closed, reconnecting will begin shortly." };
                }
                else
                {
                    return new string[] { "Connection was already closed, reconnecting is in progress." };
                }
            }
        }

        public override void OnEnable()
        {
            Thread.Sleep(2000);
            plugin = this;

            serverStartTime.Start();
            this.AddCommand("scpd_rc", new ReconnectCommand(this));
            this.AddCommand("scpd_reconnect", new ReconnectCommand(this));

            SetUpFileSystem();
            LoadConfig();
            roleSync = new RoleSync(this);

            Language.Initialise();
            Thread connectionThread = new Thread(new ThreadStart(() => new StartNetworkSystem(plugin)));
            connectionThread.Start();
            this.Info("SCPDiscord " + this.Details.version + " enabled.");
        }

        public void SetUpFileSystem()
        {
            if (!Directory.Exists(FileManager.GetAppFolder() + "SCPDiscord"))
            {
                Directory.CreateDirectory(FileManager.GetAppFolder() + "SCPDiscord");
            }
            
            if (!File.Exists(FileManager.GetAppFolder() + "SCPDiscord/" + GetConfigString("scpdiscord_config")))
            {
                this.Info("Config file " + GetConfigString("scpdiscord_config") + " does not exist, creating...");
                File.WriteAllText(FileManager.GetAppFolder() + "SCPDiscord/" + GetConfigString("scpdiscord_config"), Encoding.UTF8.GetString(Resources.config));
            }
        }

        public void LoadConfig()
        {
            try
            {
                // Reads file contents into FileStream
                FileStream stream = File.OpenRead(FileManager.GetAppFolder() + "SCPDiscord/" + GetConfigString("scpdiscord_config"));

                // Converts the FileStream into a YAML Dictionary object
                var deserializer = new DeserializerBuilder().Build();
                var yamlObject = deserializer.Deserialize(new StreamReader(stream));

                // Converts the YAML Dictionary into JSON String
                var serializer = new SerializerBuilder()
                    .JsonCompatible()
                    .Build();
                string jsonString = serializer.Serialize(yamlObject);
                Config.DeserialiseJSON(this, JObject.Parse(jsonString));
                this.Info("Successfully loaded config.");
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException)
                {
                    this.Error("Config directory not found.");
                }
                else if (e is UnauthorizedAccessException)
                {
                    this.Error("Primary language file access denied.");
                }
                else if (e is FileNotFoundException)
                {
                    this.Error("'" + GetConfigString("scpdiscord_config") + "' was not found.");
                }
                else if (e is JsonReaderException || e is YamlException)
                {
                    this.Error("'" + GetConfigString("scpdiscord_config") + "' formatting error.");
                }
                this.Error("Error reading config file '" + GetConfigString("scpdiscord_config") + "'. Aborting startup." + e);
                Disable();
            }
        }

        public void Disable()
        {
            pluginManager.DisablePlugin(this);
        }

        public override void OnDisable()
        {
            shutdown = true;
            NetworkSystem.Disconnect();
            this.Info("SCPDiscord disabled.");
        }

        public void QueueMessage(string[] channelAliases, string message)
        {
            foreach (string channel in channelAliases)
            {
                if (Config.GetDict("aliases").ContainsKey(channel))
                {
                    NetworkSystem.QueueMessage(channel + message);
                }
            }
        }

        public void SendMessage(string[] channelAliases, string messagePath, Dictionary<string, string> variables = null)
        {
            foreach(string channel in channelAliases)
            {
                if(Config.GetDict("aliases").ContainsKey(channel))
                {
                    Thread messageThread = new Thread(new ThreadStart(() => new ProcessMessageAsync(Config.GetDict("aliases")[channel], messagePath, variables)));
                    messageThread.Start();
                }
            }
        }

        public void SendMessage(string channelid, string messagePath, Dictionary<string, string> variables = null)
        {
            Thread messageThread = new Thread(new ThreadStart(() => new ProcessMessageAsync(channelid, messagePath, variables)));
            messageThread.Start();
        }

        /// <summary>
        /// Kicks a player by SteamID.
        /// </summary>
        /// <param name="steamID">SteamID of player to be kicked.</param>
        /// <param name="message">Message to be displayed to kicked user.</param>
        /// <returns>True if player was found, false if not.</returns>
        public bool KickPlayer(string steamID, string message = "Kicked from server")
        {
            foreach (Smod2.API.Player player in pluginManager.Server.GetPlayers())
            {
                if (player.SteamId == steamID)
                {
                    player.Ban(0, message);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a player name by SteamID.
        /// </summary>
        /// <param name="steamID">SteamID of a player.</param>
        /// <param name="name">String that will be set to the player name.</param>
        /// <returns>True if player was found, false if not.</returns>
        public bool GetPlayerName(string steamID, ref string name)
        {
            foreach (Smod2.API.Player player in pluginManager.Server.GetPlayers())
            {
                if (player.SteamId == steamID)
                {
                    name = player.Name;
                    return true;
                }
            }
            return false;
        }
    }
}
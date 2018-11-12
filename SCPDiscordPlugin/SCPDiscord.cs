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
    internal class SCPDiscordPlugin : Plugin
    {
        //Sends outgoing messages
        public TcpClient clientSocket = new TcpClient();

        public bool hasConnectedOnce = false;

        public Stopwatch serverStartTime = new Stopwatch();

        internal static SCPDiscordPlugin instance;

        public override void Register()
        {
            // Event handlers
            this.AddEventHandlers(new RoundEventListener(this), Priority.Highest);
            this.AddEventHandlers(new PlayerEventListener(this), Priority.Highest);
            this.AddEventHandlers(new AdminEventListener(this), Priority.Highest);
            this.AddEventHandlers(new EnvironmentEventListener(this), Priority.Highest);
            this.AddEventHandlers(new TeamEventListener(this), Priority.Highest);
            this.AddEventHandlers(new StatusUpdater(this), Priority.Highest);

            this.AddConfig(new Smod2.Config.ConfigSetting("max_players", "20", Smod2.Config.SettingType.STRING, true, "Gets the max players without reserved slots."));

            this.AddConfig(new Smod2.Config.ConfigSetting("scpdiscord_config", "config.yml", Smod2.Config.SettingType.STRING, true, "Name of the config file to use, by default 'config.yml'"));
        }

        class ReconnectCommand : ICommandHandler
        {
            private SCPDiscordPlugin plugin;
            public ReconnectCommand(SCPDiscordPlugin plugin)
            {
                this.plugin = plugin;
            }

            public string GetCommandDescription()
            {
                return "Attempts to close the connection to the Discord bot and reconnect.";
            }

            public string GetUsage()
            {
                return "discord_reconnect";
            }

            public string[] OnCall(ICommandSender sender, string[] args)
            {
                if(plugin.clientSocket.Connected)
                {
                    plugin.clientSocket.Close();
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
            instance = this;
            this.Info("SCPDiscord " + this.Details.version + " enabled.");
            serverStartTime.Start();
            this.AddCommand("discord_reconnect", new ReconnectCommand(this));

            SetUpFileSystem();
            LoadConfig();

            Thread messageThread = new Thread(new ThreadStart(() => new StartThreads()));
            messageThread.Start();
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
                // TODO: Add config name confinguration here
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
                Config.Deserialise(this, JObject.Parse(jsonString));
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
                this.Error("Error reading config file '" + GetConfigString("scpdiscord_config") + "'. Aborting startup.");
                e.ToString();
                Disable();
            }
        }

        public void Disable()
        {
            pluginManager.DisablePlugin(this);
        }

        public override void OnDisable()
        {
            this.Info("SCPDiscord disabled.");
            clientSocket.Close();
        }

        public void SendMessageToBot(string[] channels, string messagePath, Dictionary<string, string> variables = null)
        {
            foreach(string channel in channels)
            {
                if(Config.aliases.ContainsKey(channel))
                {
                    Thread messageThread = new Thread(new ThreadStart(() => new SendMessageToBot(this, Config.aliases[channel], messagePath, variables)));
                    messageThread.Start();
                }
            }
        }


        public void RefreshBotActivity()
        {
            Thread messageThread = new Thread(new ThreadStart(() => new RefreshBotActivity(this)));
            messageThread.Start();
        }

        public void RefreshChannelTopic(float tps)
        {
            foreach (string channel in Config.channels.topic)
            {
                if (Config.aliases.ContainsKey(channel))
                {
                    Thread messageThread = new Thread(new ThreadStart(() => new RefreshChannelTopic(this, Config.aliases[channel], tps)));
                    messageThread.Start();
                }
            }
        }

        /// <summary>
        /// Kicks a player by SteamID.
        /// </summary>
        /// <param name="steamID">SteamID of player to be kicked.</param>
        /// <param name="message">Message to be displayed to kicked user.</param>
        /// <returns>True if player was found, false if not.</returns>
        public bool KickPlayer(string steamID, string message = "Kicked from server")
        {
            foreach (Smod2.API.Player player in this.pluginManager.Server.GetPlayers())
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
            foreach (Smod2.API.Player player in this.pluginManager.Server.GetPlayers())
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
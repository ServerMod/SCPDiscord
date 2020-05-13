using Newtonsoft.Json;
using SCPDiscord.Properties;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Commands;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using SCPDiscord.Commands;
using SCPDiscord.EventListeners;
using Smod2.EventHandlers;
using Smod2.Piping;
using YamlDotNet.Core;

namespace SCPDiscord
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "SCPDiscord",
        description = "SCP:SL - Discord bridge.",
        id = "karlofduty.scpdiscord",
        version = "1.4.0",
        SmodMajor = 3,
        SmodMinor = 7,
        SmodRevision = 0
    )]

    public class SCPDiscord : Plugin
    {
        public readonly Stopwatch serverStartTime = new Stopwatch();

        internal static SCPDiscord plugin;

        public bool roundStarted = false;

        public RoleSync roleSync;

        public bool shutdown;

        public int maxPlayers = 20;

        public override void Register()
        {
            // Event handlers
            AddEventHandlers(new RoundEventListener(this), Priority.Highest);
            AddEventHandlers(new PlayerEventListener(this), Priority.Highest);
            AddEventHandlers(new AdminEventListener(this), Priority.Highest);
            AddEventHandlers(new EnvironmentEventListener(this), Priority.Highest);
            AddEventHandlers(new TeamEventListener(this), Priority.Highest);
            AddEventHandlers(new TickCounter(), Priority.Highest);
            AddEventHandlers(new SyncPlayerRole(), Priority.Highest);

            AddConfig(new Smod2.Config.ConfigSetting("max_players", 20, true, "Gets the max players without reserved slots."));

            AddConfig(new Smod2.Config.ConfigSetting("scpdiscord_config_global", false, true, "Whether or not the config should be placed in the global config directory."));
            AddConfig(new Smod2.Config.ConfigSetting("scpdiscord_rolesync_global", true, true, "Whether or not the rolesync file should be placed in the global config directory."));
            AddConfig(new Smod2.Config.ConfigSetting("scpdiscord_languages_global", true, true, "Whether or not the languages should be placed in the global config directory."));
        }

        public override void OnEnable()
        {
            plugin = this;

            serverStartTime.Start();
            AddCommand("scpd_rc", new ReconnectCommand());
            AddCommand("scpd_reconnect", new ReconnectCommand());
            AddCommand("scpd_reload", new ReloadCommand());
            AddCommand("scpd_unsync", new UnsyncCommand());
            AddCommand("scpd_verbose", new VerboseCommand());
            AddCommand("scpd_debug", new DebugCommand());
			AddCommand("scpd_validate", new ValidateCommand());
			AddCommand("scpd_grs", new GrantReservedSlotCommand());
			AddCommand("scpd_rrs", new RemoveReservedSlotCommand());
			AddCommand("scpd_grantreservedslot", new GrantReservedSlotCommand());
			AddCommand("scpd_removereservedslot", new RemoveReservedSlotCommand());
			AddCommand("scpd_gvr", new GrantVanillaRankCommand());
			AddCommand("scpd_grantvanillarank", new GrantVanillaRankCommand());

			SetUpFileSystem();
            this.roleSync = new RoleSync(this);
            LoadConfig();
            if (this.Server.Port == Config.GetInt("bot.port"))
            {
				this.Error("ERROR: Server is running on the same port as the plugin, aborting...");
				this.Disable();
            }
            Language.Reload();

            new Thread(() => new StartNetworkSystem(plugin)).Start();

            this.maxPlayers = GetConfigInt("max_players");
            Info("SCPDiscord " + this.Details.version + " enabled.");
        }

        private class SyncPlayerRole : IEventHandlerPlayerJoin
        {
	        public void OnPlayerJoin(PlayerJoinEvent ev)
	        {
		        plugin.roleSync.SendRoleQuery(ev.Player.SteamId);
	        }
        }

		/// <summary>
		/// Makes sure all plugin files exist.
		/// </summary>
		public void SetUpFileSystem()
        {
            if (!Directory.Exists(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_config_global")) + "SCPDiscord"))
            {
                Directory.CreateDirectory(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_config_global")) + "SCPDiscord");
            }

            if (!Directory.Exists(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord"))
            {
                Directory.CreateDirectory(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord");
            }

            if (!Directory.Exists(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_languages_global")) + "SCPDiscord"))
            {
                Directory.CreateDirectory(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_languages_global")) + "SCPDiscord");
            }

            if (!File.Exists(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml"))
            {
                this.Info("Config file '" + FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml' does not exist, creating...");
                File.WriteAllText(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml", Encoding.UTF8.GetString(Resources.config));
            }

            if (!File.Exists(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json"))
            {
                plugin.Info("Config file rolesync.json does not exist, creating...");
                File.WriteAllText(FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json", "[]");
            }
        }

		/// <summary>
		/// Loads all config options from the plugin config file.
		/// </summary>
        public void LoadConfig()
        {
            try
            {
                Config.Reload(plugin);
                this.Info("Successfully loaded config '" + FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml'.");
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
                    this.Error("'" + FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml' was not found.");
                }
                else if (e is JsonReaderException || e is YamlException)
                {
                    this.Error("'" + FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml' formatting error.");
                }
                this.Error("Error reading config file '" + FileManager.GetAppFolder(true, GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml'. Aborting startup." + e);
                Disable();
            }
        }

        public void Disable()
        {
            PluginManager.DisablePlugin(this);
        }

        public override void OnDisable()
        {
            shutdown = true;
            NetworkSystem.Disconnect();
            this.Info("SCPDiscord disabled.");
        }

		/// <summary>
		/// Logging functions
		/// </summary>

		public void Verbose(string message)
		{
			if (Config.GetBool("settings.verbose"))
			{
				plugin.Info(message);
			}
		}

		public void VerboseWarn(string message)
		{
			if (Config.GetBool("settings.verbose"))
			{
				plugin.Warn(message);
			}
		}

		public void VerboseError(string message)
		{
			if (Config.GetBool("settings.verbose"))
			{
				plugin.Error(message);
			}
		}

		public new void Debug(string message)
		{
			if (Config.GetBool("settings.debug"))
			{
				plugin.Info(message);
			}
		}

		public void DebugWarn(string message)
		{
			if (Config.GetBool("settings.debug"))
			{
				plugin.Warn(message);
			}
		}

		public void DebugError(string message)
		{
			if (Config.GetBool("settings.debug"))
			{
				plugin.Error(message);
			}
		}

		// TODO: Set message functions back to void whenever piping void functions is fixed.

		/// <summary>
		/// Enqueue a string to be sent to Discord.
		/// </summary>
		/// <param name="channelAliases">The user friendly name of the channel, set in the config.</param>
		/// <param name="message">The message to be sent.</param>
		[PipeMethod]
		public bool SendString(IEnumerable<string> channelAliases, string message)
        {
            foreach (string channel in channelAliases)
            {
                if (Config.GetDict("aliases").ContainsKey(channel))
                {
                    NetworkSystem.QueueMessage(Config.GetDict("aliases")[channel] + message);
                }
            }

            return true;
        }
		[PipeMethod]
		public bool SendStringByID(string channelID, string message)
		{
			NetworkSystem.QueueMessage(channelID + message);
			return true;
		}

		/// <summary>
		/// Sends a message from the loaded language file.
		/// </summary>
		/// <param name="channelAliases">A collection of channel aliases, set in the config.</param>
		/// <param name="messagePath">The language node of the message to send.</param>
		/// <param name="variables">Variables to support in the message as key value pairs.</param>
		[PipeMethod]
		public bool SendMessage(IEnumerable<string> channelAliases, string messagePath, Dictionary<string, string> variables = null)
        {
            foreach (string channel in channelAliases)
            {
                if (Config.GetDict("aliases").ContainsKey(channel))
                {
	                Thread messageThread = new Thread(() => new ProcessMessageAsync(Config.GetDict("aliases")[channel], messagePath, variables));
                    messageThread.Start();
                }
            }

            return true;
        }

		/// <summary>
		/// Sends a message from the loaded language file to a specific channel by channel ID. Usually used for replies to Discord messages.
		/// </summary>
		/// <param name="channelID">The ID of the channel to send to.</param>
		/// <param name="messagePath">The language node of the message to send.</param>
		/// <param name="variables">Variables to support in the message as key value pairs.</param>
		[PipeMethod]
		public bool SendMessageByID(string channelID, string messagePath, Dictionary<string, string> variables = null)
        {
	        new Thread(() => new ProcessMessageAsync(channelID, messagePath, variables)).Start();
            return true;
        }

        /// <summary>
        /// Kicks a player by SteamID.
        /// </summary>
        /// <param name="steamID">SteamID of player to be kicked.</param>
        /// <param name="message">Message to be displayed to kicked user.</param>
        /// <returns>True if player was found, false if not.</returns>
        public bool KickPlayer(string steamID, string message = "Kicked from server")
        {
            foreach (Player player in PluginManager.Server.GetPlayers())
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
            foreach (Player player in PluginManager.Server.GetPlayers())
            {
	            if (player.SteamId == steamID)
                {
                    name = player.Name;
                    return true;
                }
            }
            return false;
        }

		/// <summary>
		/// Runs a console command.
		/// </summary>
		/// <param name="user">The user to run the command as, null for the server itself.</param>
		/// <param name="command">The name of the command to run.</param>
		/// <param name="arguments">Command arguments split up into individual strings.</param>
		/// <returns></returns>
        public string ConsoleCommand(ICommandSender user, string command, string[] arguments)
		{
			if(user == null)
			{
				user = Server;
			}

			string[] feedback = plugin.PluginManager.CommandManager.CallCommand(user, command, arguments);

			StringBuilder builder = new StringBuilder();
			foreach (string line in feedback)
			{
				builder.Append(line + "\n");
			}
			return builder.ToString();
		}
    }
}
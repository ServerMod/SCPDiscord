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
using System.Threading.Tasks;
using SCPDiscord.EventListeners;
using YamlDotNet.Core;

namespace SCPDiscord
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "SCPDiscord",
        description = "SCP:SL - Discord bridge.",
        id = "karlofduty.scpdiscord",
        version = "1.3.0",
        SmodMajor = 3,
        SmodMinor = 4,
        SmodRevision = 0
    )]
    // ReSharper disable once ClassNeverInstantiated.Global
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

            Task.Run(async () =>
            {
                await Task.Delay(4000);
                SetUpFileSystem();
                LoadConfig();
                this.roleSync = new RoleSync(this);

                Language.Reload();
                // ReSharper disable once ObjectCreationAsStatement
                Thread connectionThread = new Thread(() => new StartNetworkSystem(plugin));
                connectionThread.Start();

                this.maxPlayers = GetConfigInt("max_players");
                Info("SCPDiscord " + this.Details.version + " enabled.");
            });
        }

        private class ReconnectCommand : ICommandHandler
        {
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
                if (sender is Player player)
                {
                    if (!player.HasPermission("scpdiscord.reconnect"))
                    {
                        return new[] { "You don't have permission to use that command." };
                    }
                }

                if (NetworkSystem.IsConnected())
                {
                    NetworkSystem.Disconnect();
                    return new[] { "Connection closed, reconnecting will begin shortly." };
                }
                else
                {
                    return new[] { "Connection was already closed, reconnecting is in progress." };
                }
            }
        }

        private class ReloadCommand : ICommandHandler
        {
            public string GetCommandDescription()
            {
                return "Reloads all plugin configs and data files and then reconnects.";
            }

            public string GetUsage()
            {
                return "scpd_reload";
            }

            public string[] OnCall(ICommandSender sender, string[] args)
            {
                if (sender is Player player)
                {
                    if (!player.HasPermission("scpdiscord.reload"))
                    {
                        return new[] { "You don't have permission to use that command." };
                    }
                }

                plugin.Info("Reloading plugin...");
                plugin.LoadConfig();
                Language.Reload();
                plugin.roleSync.Reload();
                if (NetworkSystem.IsConnected())
                {
                    NetworkSystem.Disconnect();
                }

                return new[] { "Reload complete." };
            }
        }

        private class UnsyncCommand : ICommandHandler
        {
            public string GetCommandDescription()
            {
                return "Removes a user from having their discord role synced to the server.";
            }

            public string GetUsage()
            {
                return "scpd_unsync <discord id>";
            }

            public string[] OnCall(ICommandSender sender, string[] args)
            {
                if (sender is Player player)
                {
                    if (!player.HasPermission("scpdiscord.unsync"))
                    {
                        return new[] { "You don't have permission to use that command." };
                    }
                }

                if (args.Length > 0)
                {
                    return new[] { plugin.roleSync.RemovePlayer(args[0]) };
                }
                else
                {
                    return new[] { "Not enough arguments." };
                }
            }
        }

        private class VerboseCommand : ICommandHandler
        {
            public string GetCommandDescription()
            {
                return "Toggles verbose messages.";
            }

            public string GetUsage()
            {
                return "scpd_verbose";
            }

            public string[] OnCall(ICommandSender sender, string[] args)
            {
                if (sender is Player player)
                {
                    if (!player.HasPermission("scpdiscord.verbose"))
                    {
                        return new[] { "You don't have permission to use that command." };
                    }
                }
                Config.SetBool("settings.verbose", !Config.GetBool("settings.verbose"));
                return new[] { "Verbose messages: " + Config.GetBool("settings.verbose") };
            }
        }

        private class DebugCommand : ICommandHandler
        {
            public string GetCommandDescription()
            {
                return "Toggles debug messages.";
            }

            public string GetUsage()
            {
                return "scpd_debug";
            }

            public string[] OnCall(ICommandSender sender, string[] args)
            {
                if (sender is Player player)
                {
                    if (!player.HasPermission("scpdiscord.debug"))
                    {
                        return new[] { "You don't have permission to use that command." };
                    }
                }
                Config.SetBool("settings.debug", !Config.GetBool("settings.debug"));
                return new[] { "Debug messages: " + Config.GetBool("settings.debug") };
            }
        }

        private class ValidateCommand : ICommandHandler
        {
	        public string GetCommandDescription()
	        {
		        return "Creates a config validation report.";
	        }

	        public string GetUsage()
	        {
		        return "scpd_validate";
	        }

	        public string[] OnCall(ICommandSender sender, string[] args)
	        {
		        if (sender is Player player)
		        {
			        if (!player.HasPermission("scpdiscord.validate"))
			        {
				        return new[] { "You don't have permission to use that command." };
			        }
		        }

		        Config.ValidateConfig(plugin);

				return new[] { "<End of validation report.>" };
	        }
        }

		/// <summary>
		/// Makes sure all plugin files exist.
		/// </summary>
		public void SetUpFileSystem()
        {
            if (!Directory.Exists(FileManager.GetAppFolder(GetConfigBool("scpdiscord_config_global")) + "SCPDiscord"))
            {
                Directory.CreateDirectory(FileManager.GetAppFolder(GetConfigBool("scpdiscord_config_global")) + "SCPDiscord");
            }

            if (!Directory.Exists(FileManager.GetAppFolder(GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord"))
            {
                Directory.CreateDirectory(FileManager.GetAppFolder(GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord");
            }

            if (!Directory.Exists(FileManager.GetAppFolder(GetConfigBool("scpdiscord_languages_global")) + "SCPDiscord"))
            {
                Directory.CreateDirectory(FileManager.GetAppFolder(GetConfigBool("scpdiscord_languages_global")) + "SCPDiscord");
            }

            if (!File.Exists(FileManager.GetAppFolder(GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml"))
            {
                this.Info("Config file '" + FileManager.GetAppFolder(GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml' does not exist, creating...");
                File.WriteAllText(FileManager.GetAppFolder(GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml", Encoding.UTF8.GetString(Resources.config));
            }

            // ReSharper disable once InvertIf
            if (!File.Exists(FileManager.GetAppFolder(GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json"))
            {
                plugin.Info("Config file rolesync.json does not exist, creating...");
                File.WriteAllText(FileManager.GetAppFolder(GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json", "[]");
            }
        }

		/// <summary>
		/// Loads all config options from the plugin config file.
		/// </summary>
        private void LoadConfig()
        {
            try
            {
                Config.Reload(plugin);
                this.Info("Successfully loaded config '" + FileManager.GetAppFolder(GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml'.");
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
                    this.Error("'" + FileManager.GetAppFolder(GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml' was not found.");
                }
                else if (e is JsonReaderException || e is YamlException)
                {
                    this.Error("'" + FileManager.GetAppFolder(GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml' formatting error.");
                }
                this.Error("Error reading config file '" + FileManager.GetAppFolder(GetConfigBool("scpdiscord_config_global")) + "SCPDiscord/config.yml'. Aborting startup." + e);
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

		/// <summary>
		/// Enqueue a string to be sent to Discord
		/// </summary>
		/// <param name="channelAliases">The user friendly name of the channel, set in the config.</param>
		/// <param name="message">The message to be sent.</param>
		public void SendString(IEnumerable<string> channelAliases, string message)
        {
            foreach (string channel in channelAliases)
            {
                if (Config.GetDict("aliases").ContainsKey(channel))
                {
                    NetworkSystem.QueueMessage(channel + message);
                }
            }
        }

		/// <summary>
		/// Sends a message from the loaded language file.
		/// </summary>
		/// <param name="channelAliases">A collection of channel aliases, set in the config.</param>
		/// <param name="messagePath">The language node of the message to send.</param>
		/// <param name="variables">Variables to support in the message as key value pairs.</param>
        public void SendMessage(IEnumerable<string> channelAliases, string messagePath, Dictionary<string, string> variables = null)
        {
            foreach (string channel in channelAliases)
            {
                if (Config.GetDict("aliases").ContainsKey(channel))
                {
	                // ReSharper disable once ObjectCreationAsStatement
	                Thread messageThread = new Thread(() => new ProcessMessageAsync(Config.GetDict("aliases")[channel], messagePath, variables));
                    messageThread.Start();
                }
            }
        }

		/// <summary>
		/// Sends a message from the loaded language file to a specific channel by channel ID. Usually used for replies to Discord messages.
		/// </summary>
		/// <param name="channelID">The ID of the channel to send to.</param>
		/// <param name="messagePath">The language node of the message to send.</param>
		/// <param name="variables">Variables to support in the message as key value pairs.</param>
        public void SendMessage(string channelID, string messagePath, Dictionary<string, string> variables = null)
        {
	        // ReSharper disable once ObjectCreationAsStatement
	        Thread messageThread = new Thread(() => new ProcessMessageAsync(channelID, messagePath, variables));
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
            foreach (Player player in PluginManager.Server.GetPlayers())
            {
	            // ReSharper disable once InvertIf
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
	            // ReSharper disable once InvertIf
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
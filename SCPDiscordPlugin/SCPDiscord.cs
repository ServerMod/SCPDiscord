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
    public class SCPDiscord : Plugin
    {
        public Stopwatch serverStartTime = new Stopwatch();

        internal static SCPDiscord plugin;

        public bool roundStarted = false;

        public RoleSync roleSync;

        public bool shutdown = false;

        public int maxPlayers = 20;

        public override void Register()
        {
            // Event handlers
            this.AddEventHandlers(new RoundEventListener(this), Priority.Highest);
            this.AddEventHandlers(new PlayerEventListener(this), Priority.Highest);
            this.AddEventHandlers(new AdminEventListener(this), Priority.Highest);
            this.AddEventHandlers(new EnvironmentEventListener(this), Priority.Highest);
            this.AddEventHandlers(new TeamEventListener(this), Priority.Highest);
            this.AddEventHandlers(new TickCounter(), Priority.Highest);
            this.AddEventHandlers(new SyncPlayerRole(), Priority.Highest);

            this.AddConfig(new Smod2.Config.ConfigSetting("max_players", 20, true, "Gets the max players without reserved slots."));

            this.AddConfig(new Smod2.Config.ConfigSetting("scpdiscord_config_global", false, true, "Whether or not the config should be placed in the global config directory."));
            this.AddConfig(new Smod2.Config.ConfigSetting("scpdiscord_rolesync_global", true, true, "Whether or not the rolesync file should be placed in the global config directory."));
            this.AddConfig(new Smod2.Config.ConfigSetting("scpdiscord_languages_global", true, true, "Whether or not the languages should be placed in the global config directory."));
        }

        public override void OnEnable()
        {
            plugin = this;

            serverStartTime.Start();
            this.AddCommand("scpd_rc", new ReconnectCommand(this));
            this.AddCommand("scpd_reconnect", new ReconnectCommand(this));
            this.AddCommand("scpd_reload", new ReloadCommand(this));
            this.AddCommand("scpd_unsync", new UnsyncCommand(this));
            this.AddCommand("scpd_verbose", new VerboseCommand(this));
            this.AddCommand("scpd_debug", new DebugCommand(this));

            Task.Run(async () =>
            {
                await Task.Delay(4000);
                SetUpFileSystem();
                LoadConfig();
                roleSync = new RoleSync(this);

                Language.Reload();
                Thread connectionThread = new Thread(new ThreadStart(() => new StartNetworkSystem(plugin)));
                connectionThread.Start();

                this.maxPlayers = GetConfigInt("max_players");
                this.Info("SCPDiscord " + this.Details.version + " enabled.");
            });
        }

        private class ReconnectCommand : ICommandHandler
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
                if (sender is Player player)
                {
                    if (!player.HasPermission("scpdiscord.reconnect"))
                    {
                        return new string[] { "You don't have permission to use that command." };
                    }
                }

                if (NetworkSystem.IsConnected())
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

        private class ReloadCommand : ICommandHandler
        {
            private SCPDiscord plugin;

            public ReloadCommand(SCPDiscord plugin)
            {
                this.plugin = plugin;
            }

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
                        return new string[] { "You don't have permission to use that command." };
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

                return new string[] { "Reload complete." };
            }
        }

        private class UnsyncCommand : ICommandHandler
        {
            private SCPDiscord plugin;

            public UnsyncCommand(SCPDiscord plugin)
            {
                this.plugin = plugin;
            }

            public string GetCommandDescription()
            {
                return "Removes a user from having their discord role synced to the server.";
            }

            public string GetUsage()
            {
                return "scpd_unsync <discordid>";
            }

            public string[] OnCall(ICommandSender sender, string[] args)
            {
                if (sender is Player player)
                {
                    if (!player.HasPermission("scpdiscord.unsync"))
                    {
                        return new string[] { "You don't have permission to use that command." };
                    }
                }

                if (args.Length > 0)
                {
                    return new string[] { plugin.roleSync.RemovePlayer(args[0]) };
                }
                else
                {
                    return new string[] { "Not enough arguments." };
                }
            }
        }

        private class VerboseCommand : ICommandHandler
        {
            private SCPDiscord plugin;

            public VerboseCommand(SCPDiscord plugin)
            {
                this.plugin = plugin;
            }

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
                        return new string[] { "You don't have permission to use that command." };
                    }
                }
                Config.SetBool("settings.verbose", !Config.GetBool("settings.verbose"));
                return new string[] { "Verbose messages: " + Config.GetBool("settings.verbose") };
            }
        }

        private class DebugCommand : ICommandHandler
        {
            private SCPDiscord plugin;

            public DebugCommand(SCPDiscord plugin)
            {
                this.plugin = plugin;
            }

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
                        return new string[] { "You don't have permission to use that command." };
                    }
                }
                Config.SetBool("settings.debug", !Config.GetBool("settings.debug"));
                return new string[] { "Debug messages: " + Config.GetBool("settings.debug") };
            }
        }

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

            if (!File.Exists(FileManager.GetAppFolder(GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json"))
            {
                plugin.Info("Config file rolesync.json does not exist, creating...");
                File.WriteAllText(FileManager.GetAppFolder(GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json", "[]");
            }
        }

        public void LoadConfig()
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
            foreach (string channel in channelAliases)
            {
                if (Config.GetDict("aliases").ContainsKey(channel))
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
            foreach (Smod2.API.Player player in PluginManager.Server.GetPlayers())
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
            foreach (Smod2.API.Player player in PluginManager.Server.GetPlayers())
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
using Smod2;
using Smod2.Attributes;

using System.Net.Sockets;
using System;

using System.Threading;
using System.IO;
using System.Collections.Generic;
using Smod2.Events;
using SCPDiscord.Properties;

namespace SCPDiscord
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "SCPDiscord",
        description = "SCP:SL - Discord bridge.",
        id = "karlofduty.scpdiscord",
        version = "0.2.3",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 12
        )]
    internal class SCPDiscordPlugin : Plugin
    {
        //Sends outgoing messages
        public TcpClient clientSocket = new TcpClient();

        public bool hasConnectedOnce = false;

        public MessageConfig messageConfig;

        public override void Register()
        {
            //Event handlers
            this.AddEventHandlers(new RoundEventListener(this), Priority.Highest);
            this.AddEventHandlers(new PlayerEventListener(this), Priority.Highest);
            this.AddEventHandlers(new AdminEventListener(this), Priority.Highest);
            this.AddEventHandlers(new EnvironmentEventListener(this), Priority.Highest);
            this.AddEventHandlers(new TeamEventListener(this), Priority.Highest);
            this.AddEventHandlers(new StatusUpdater(this), Priority.Highest);

            //Connection settings
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_bot_ip", "127.0.0.1", Smod2.Config.SettingType.STRING, true, "IP of the discord bot."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_bot_port", 8888, Smod2.Config.SettingType.NUMERIC, true, "Port to send messages to the bot on."));

            //Round events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onroundstart", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onconnect", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondisconnect", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_oncheckroundend", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onroundend", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onwaitingforplayers", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onroundrestart", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetservername", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onscenechanged", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));

            //Environment events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onscp914activate", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onstartcountdown", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onstopcountdown", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondetonate", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondecontaminate", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));

            //Player events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerdie", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerhurt", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerpickupitem", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerdropitem", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerjoin", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onnicknameset", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onassignteam", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetrole", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_oncheckescape", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onspawn", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondooraccess", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onintercom", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onintercomcooldowncheck", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onpocketdimensionexit", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onpocketdimensionenter", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onpocketdimensiondie", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onthrowgrenade", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerinfected", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onspawnragdoll", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onlure", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_oncontain106", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onmedkituse", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onshoot", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_on106createportal", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_on106teleport", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onelevatoruse", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));

            //Admin events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onadminquery", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onauthcheck", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onban", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));

            //Team events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondecideteamrespawnqueue", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetrolemaxhp", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onteamrespawn", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetscpconfig", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));

            //Message options
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_language", "english", Smod2.Config.SettingType.STRING, true, "Name of the language config to use."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_formatting_date", "HH:mm:ss", Smod2.Config.SettingType.STRING, true, "Discord time formatting, 'off' to remove."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_verbose", false, Smod2.Config.SettingType.BOOL, true, "Log every message sent to discord in the console."));
        }

        public override void OnEnable()
        {
            this.Info("SCPDiscord " + this.Details.version + " enabled.");

            // Fucks with things until the plugin works - hopefully I remember to add a more elegant fix in the future
            Thread messageThread = new Thread(new ThreadStart(() => new MessageConfig(this)));
            messageThread.Start();
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

        /// <summary>
        /// Gets a message from the language file, parses it and sends it.
        /// </summary>
        /// <param name="prefix">The channel ID to post the message in. Also can be a keyword such as default, off or bot function such as playercount.</param>
        /// <param name="messagePath">The JSON JPath describing the message node location.</param>
        /// <param name="variables">Variables to be parsed into the string.</param>
        public void SendToBot(string prefix, string messagePath, Dictionary<string, string> variables = null)
        {
            if (prefix != "off")
            {
                Thread messageThread = new Thread(new ThreadStart(() => new SendToBot(this, prefix, messagePath, variables)));
                messageThread.Start();
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
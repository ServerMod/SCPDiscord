using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;

using System.Net;
using System.Net.Sockets;
using System;

using System.Threading;

namespace SCPDiscord
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "SCPDiscord",
        description = "Plugin which outputs server events to Discord.",
        id = "karlofduty.scpdiscord",
        version = "0.0.1",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 8
        )]
    class SCPDiscordPlugin : Plugin
    {
        public TcpClient clientSocket = new TcpClient();

        public override void Register()
        {
            //Event handlers
            this.AddEventHandlers(new RoundEventHandler(this));
            this.AddEventHandlers(new PlayerEventHandler(this));
            this.AddEventHandlers(new AdminEventHandler(this));
            this.AddEventHandlers(new EnvironmentEventHandler(this));
            this.AddEventHandlers(new TeamEventHandler(this));

            //Round events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onroundstart", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onroundend", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onroundrestart", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onconnect", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondisconnect", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetservername", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onwaitingforplayers", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));

            //Environment events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondecontaminate", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondetonate", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onstartcountdown", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onstopcountdown", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onscp914activate", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));

            //Player events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onassignteam", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_oncheckescape", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondooraccess", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onintercom", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onnicknameset", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerdie", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerdropitem", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerhurt", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerjoin", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onplayerpickupitem", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetrole", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onspawn", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));

            //Admin events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onadminquery", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onauthcheck", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onban", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));

            //Team events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondecideteamrespawnqueue", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetrolemaxhp", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onteamrespawn", 0, Smod2.Config.SettingType.NUMERIC, true, "Discord channel to post event messages in."));
        }

        public override void OnEnable()
        {
            try
            {
                clientSocket.Connect("127.0.0.1", 8888);
            }
            catch(SocketException e)
            {
                this.Info("Error occured while connecting to discord bot server.\n" + e.ToString());
                this.pluginManager.DisablePlugin(this);
            }
            catch (ObjectDisposedException e)
            {
                this.Info("TCP client was unexpectedly closed.\n" + e.ToString());
                this.pluginManager.DisablePlugin(this);
            }
            catch (ArgumentOutOfRangeException e)
            {
                this.Info("Invalid port.\n" + e.ToString());
                this.pluginManager.DisablePlugin(this);
            }
            catch (ArgumentNullException e)
            {
                this.Info("IP address is null.\n" + e.ToString());
                this.pluginManager.DisablePlugin(this);
            }

            this.Info("SCPDiscord enabled.");
            SendMessageAsync("Plugin Enabled.");
        }

        public void SendMessageAsync(string message)
        {
            Thread messageThread = new Thread(new ThreadStart(() => new AsyncMessage(this, message)));
            messageThread.Start();
        }

        public override void OnDisable()
        {
            this.Info("SCPDiscord disabled.");
        }
    }
}
using Smod2;
using Smod2.Attributes;

using System.Net.Sockets;
using System;

using System.Threading;

using System.IO;

using System.Reflection;


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
        SmodRevision = 9
        )]
    class SCPDiscordPlugin : Plugin
    {
        public TcpClient clientSocket = new TcpClient();
        public readonly string GENERICMESSAGECHANNEL = "000000000000000000";
        public bool hasConnectedOnce = false;
        public static string multilanguage_path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "sm_plugins" + Path.DirectorySeparatorChar + "SCPDiscord";


        public override void Register()
        {
            //Event handlers
            this.AddEventHandlers(new RoundEventHandler(this));
            this.AddEventHandlers(new PlayerEventHandler(this));
            this.AddEventHandlers(new AdminEventHandler(this));
            this.AddEventHandlers(new EnvironmentEventHandler(this));
            this.AddEventHandlers(new TeamEventHandler(this));

            //Connection settings
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_bot_ip", "127.0.0.1", Smod2.Config.SettingType.STRING, true, "IP of the discord bot."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_bot_port", 8888, Smod2.Config.SettingType.NUMERIC, true, "Port of the discord bot."));

            //Round events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onroundstart", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onconnect", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondisconnect", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_oncheckroundend", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onroundend", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onwaitingforplayers", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onroundrestart", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetservername", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));

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

            //Admin events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onadminquery", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onauthcheck", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onban", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));

            //Team events
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_ondecideteamrespawnqueue", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetrolemaxhp", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onteamrespawn", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_channel_onsetscpconfig", "off", Smod2.Config.SettingType.STRING, true, "Discord channel to post event messages in."));

            // Multi-Language Support Config
            this.AddConfig(new Smod2.Config.ConfigSetting("discord_bot_language", "eng", Smod2.Config.SettingType.STRING, true, "Discord bot language."));
        }

        // Multi-Language Support Method Start

        public string[] TranslationChoose()
        {
            string[] opened_file = new string[74];
            switch (this.GetConfigString("discord_bot_language"))
            {
                case "rus":
                    opened_file = File.ReadAllLines(multilanguage_path + Path.DirectorySeparatorChar + "Russian.txt");
                    break;
                default:
                    opened_file = File.ReadAllLines(multilanguage_path + Path.DirectorySeparatorChar + "English.txt");
                    break;
            }
            return opened_file;
        }

        public string MultiLanguage(int line)
        {

            line -= 1;

            string requested_line = TranslationChoose()[line];
            return requested_line;
        }
        // Multi-Language Support Method End

        public override void OnEnable()
        {
            // Multi-Language Support Check Start
            if (!Directory.Exists(multilanguage_path))
            {
                Directory.CreateDirectory(multilanguage_path);
            }
            if (!File.Exists(multilanguage_path + Path.DirectorySeparatorChar + "English.txt"))
            {
                File.WriteAllText(multilanguage_path + Path.DirectorySeparatorChar + "English.txt", Properties.Resources.English);
            }
            if (!File.Exists(multilanguage_path + Path.DirectorySeparatorChar + "Russian.txt"))
            {
                File.WriteAllText(multilanguage_path + Path.DirectorySeparatorChar + "Russian.txt", Properties.Resources.Russian);
            }
            // Multi-Language Support Check End

            this.Info("SCPDiscord enabled.");
            //Runs until the server has connected once
            Thread connectionThread = new Thread(new ThreadStart(() => new AsyncConnect(this)));
            connectionThread.Start();

            //Keeps running to auto-reconnect if needed
            Thread watchdogThread = new Thread(new ThreadStart(() => new AsyncConnectionWatchdog(this)));
            watchdogThread.Start();
        }

        public void SendMessageAsync(string channelID, string message)
        {
            if(channelID != "off")
            {
                Thread messageThread = new Thread(new ThreadStart(() => new AsyncMessage(this, channelID, message)));
                messageThread.Start();
            }
        }

        public override void OnDisable()
        {
            this.Info("SCPDiscord disabled.");
        }
    }
}
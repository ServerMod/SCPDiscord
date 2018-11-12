using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SCPDiscord
{
    internal static class Config
    {
        // I don't know how the best way of doing this is in C# but my code checks wanted me to make other classes unable to edit the variables here because something something static things not thread safe
        private static Bot _bot;
        internal static Bot bot { get => _bot; }

        private static Settings _settings;
        internal static Settings settings { get => _settings; }

        private static Dictionary<string, string> _aliases = new Dictionary<string, string>();
        internal static Dictionary<string, string> aliases { get => _aliases; }

        private static Channels _channels;
        internal static Channels channels { get => _channels; }

        internal static void Deserialise(SCPDiscordPlugin plugin, JObject configTree)
        {
            string currentNode = "";
            Thread.Sleep(1000);
            try
            {
                currentNode = "bot.ip";
                _bot.ip = configTree.SelectToken("bot.ip").Value<string>();
                currentNode = "bot.port";
                _bot.port = configTree.SelectToken("bot.port").Value<short>();
            }
            catch(Exception e)
            {
                plugin.Error("Failed to read bot config at node '" + currentNode + "'\n" + e);
            }

            try
            {
                currentNode = "settings.language";
                _settings.language = configTree.SelectToken("settings.language").Value<string>();
                currentNode = "settings.playercount";
                _settings.playercount = configTree.SelectToken("settings.playercount").Value<bool>();
                currentNode = "settings.timestamp";
                _settings.timestamp = configTree.SelectToken("settings.timestamp").Value<string>();
                currentNode = "settings.verbose";
                _settings.verbose = configTree.SelectToken("settings.verbose").Value<bool>();
                currentNode = "settings.metrics";
                _settings.metrics = true;
                currentNode = "settings.configvalidation";
                _settings.configvalidation = configTree.SelectToken("settings.configvalidation").Value<bool>();
            }
            catch(Exception e)
            {
                plugin.Error("Failed to read settings config at node '" + currentNode + "'\n" + e);
            }

            try
            {
                currentNode = "aliases";
                _aliases = configTree.SelectToken("aliases").Value<JArray>().ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());

            }
            catch (Exception e)
            {
                plugin.Error("Failed to read alias config at node '" + currentNode + "'\n" + e);
            }

            try
            {
                currentNode = "channels.statusmessages";
                _channels.statusmessages = configTree.SelectToken("channels.statusmessages").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.topic";
                _channels.topic = configTree.SelectToken("channels.topic").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.onroundstart";
                _channels.onroundstart = configTree.SelectToken("channels.onroundstart").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onconnect";
                _channels.onconnect = configTree.SelectToken("channels.onconnect").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.ondisconnect";
                _channels.ondisconnect = configTree.SelectToken("channels.ondisconnect").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.oncheckroundend";
                _channels.oncheckroundend = configTree.SelectToken("channels.oncheckroundend").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onroundend";
                _channels.onroundend = configTree.SelectToken("channels.onroundend").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onwaitingforplayers";
                _channels.onwaitingforplayers = configTree.SelectToken("channels.onwaitingforplayers").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onroundrestart";
                _channels.onroundrestart = configTree.SelectToken("channels.onroundrestart").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetservername";
                _channels.onsetservername = configTree.SelectToken("channels.onsetservername").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onscenechanged";
                _channels.onscenechanged = configTree.SelectToken("channels.onscenechanged").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.onscp914activate";
                _channels.onscp914activate = configTree.SelectToken("channels.onscp914activate").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onstartcountdown";
                _channels.onstartcountdown = configTree.SelectToken("channels.onstartcountdown").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onstopcountdown";
                _channels.onstopcountdown = configTree.SelectToken("channels.onstopcountdown").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.ondetonate";
                _channels.ondetonate = configTree.SelectToken("channels.ondetonate").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.ondecontaminate";
                _channels.ondecontaminate = configTree.SelectToken("channels.ondecontaminate").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.onplayerdie";
                _channels.onplayerdie = configTree.SelectToken("channels.onplayerdie").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerhurt";
                _channels.onplayerhurt = configTree.SelectToken("channels.onplayerhurt").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerpickupitem";
                _channels.onplayerpickupitem = configTree.SelectToken("channels.onplayerpickupitem").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerdropitem";
                _channels.onplayerdropitem = configTree.SelectToken("channels.onplayerdropitem").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerjoin";
                _channels.onplayerjoin = configTree.SelectToken("channels.onplayerjoin").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onnicknameset";
                _channels.onnicknameset = configTree.SelectToken("channels.onnicknameset").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onassignteam";
                _channels.onassignteam = configTree.SelectToken("channels.onassignteam").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetrole";
                _channels.onsetrole = configTree.SelectToken("channels.onsetrole").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.oncheckescape";
                _channels.oncheckescape = configTree.SelectToken("channels.oncheckescape").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onspawn";
                _channels.onspawn = configTree.SelectToken("channels.onspawn").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.ondooraccess";
                _channels.ondooraccess = configTree.SelectToken("channels.ondooraccess").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onintercom";
                _channels.onintercom = configTree.SelectToken("channels.onintercom").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onintercomcooldowncheck";
                _channels.onintercomcooldowncheck = configTree.SelectToken("channels.onintercomcooldowncheck").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onpocketdimensionexit";
                _channels.onpocketdimensionexit = configTree.SelectToken("channels.onpocketdimensionexit").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onpocketdimensionenter";
                _channels.onpocketdimensionenter = configTree.SelectToken("channels.onpocketdimensionenter").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onpocketdimensiondie";
                _channels.onpocketdimensiondie = configTree.SelectToken("channels.onpocketdimensiondie").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onthrowgrenade";
                _channels.onthrowgrenade = configTree.SelectToken("channels.onthrowgrenade").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerinfected";
                _channels.onplayerinfected = configTree.SelectToken("channels.onplayerinfected").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onspawnragdoll";
                _channels.onspawnragdoll = configTree.SelectToken("channels.onspawnragdoll").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onlure";
                _channels.onlure = configTree.SelectToken("channels.onlure").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.oncontain106";
                _channels.oncontain106 = configTree.SelectToken("channels.oncontain106").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onmedkituse";
                _channels.onmedkituse = configTree.SelectToken("channels.onmedkituse").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onshoot";
                _channels.onshoot = configTree.SelectToken("channels.onshoot").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.on106createportal";
                _channels.on106createportal = configTree.SelectToken("channels.on106createportal").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.on106teleport";
                _channels.on106teleport = configTree.SelectToken("channels.on106teleport").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onelevatoruse";
                _channels.onelevatoruse = configTree.SelectToken("channels.onelevatoruse").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onhandcuff";
                _channels.onhandcuff = configTree.SelectToken("channels.onhandcuff").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayertriggertesla";
                _channels.onplayertriggertesla = configTree.SelectToken("channels.onplayertriggertesla").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onscp914changeknob";
                _channels.onscp914changeknob = configTree.SelectToken("channels.onscp914changeknob").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.onadminquery";
                _channels.onadminquery = configTree.SelectToken("channels.onadminquery").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onauthcheck";
                _channels.onauthcheck = configTree.SelectToken("channels.onauthcheck").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onban";
                _channels.onban = configTree.SelectToken("channels.onban").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.ondecideteamrespawnqueue";
                _channels.ondecideteamrespawnqueue = configTree.SelectToken("channels.ondecideteamrespawnqueue").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetrolemaxhp";
                _channels.onsetrolemaxhp = configTree.SelectToken("channels.onsetrolemaxhp").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onteamrespawn";
                _channels.onteamrespawn = configTree.SelectToken("channels.onteamrespawn").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetscpconfig";
                _channels.onsetscpconfig = configTree.SelectToken("channels.onsetscpconfig").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetntfunitname";
                _channels.onsetntfunitname = configTree.SelectToken("channels.onsetntfunitname").Value<JArray>().Values<string>().ToArray();
            }
            catch (Exception e)
            {
                plugin.Error("Failed to read channel config at node '" + currentNode + "'\n" + e);
            }

            if(_settings.configvalidation)
            {
                ValidateConfig(plugin);
            }
        }

        public static void Serialise()
        {
            // To be implemented in case config saving is needed at any point
        }

        public static void ValidateConfig(SCPDiscordPlugin plugin)
        {
            plugin.Info("############################");
            plugin.Info("SCPDiscord CONFIG VALIDATION");
            plugin.Info("--------------------");
            plugin.Info("bot.ip: " + bot.ip);
            plugin.Info("bot.port: " + bot.port);
            plugin.Info("--------------------");
            plugin.Info("settings.language: " + settings.language);
            plugin.Info("settings.playercount: " + settings.playercount);
            plugin.Info("settings.timestamp: " + settings.timestamp);
            plugin.Info("settings.verbose: " + settings.verbose);
            plugin.Info("settings.metrics: " + settings.metrics);
            plugin.Info("--------------------");
            plugin.Info("aliases: ");
            foreach (KeyValuePair<string,string> alias in aliases)
            {
                plugin.Info("    " + alias.Key + ": " + alias.Value);
            }
            plugin.Info("--------------------");
            plugin.Info("channels.statusmessages: " + string.Join(",", channels.statusmessages));
            plugin.Info("channels.topic: " + string.Join(",", channels.topic));
            plugin.Info("--------------------");
            plugin.Info("channels.onroundstart: " + string.Join(",", channels.onroundstart));
            plugin.Info("channels.onconnect: " + string.Join(",", channels.onconnect));
            plugin.Info("channels.ondisconnect: " + string.Join(",", channels.ondisconnect));
            plugin.Info("channels.oncheckroundend: " + string.Join(",", channels.oncheckroundend));
            plugin.Info("channels.onroundend: " + string.Join(",", channels.onroundend));
            plugin.Info("channels.onwaitingforplayers: " + string.Join(",", channels.onwaitingforplayers));
            plugin.Info("channels.onroundrestart: " + string.Join(",", channels.onroundrestart));
            plugin.Info("channels.onsetservername: " + string.Join(",", channels.onsetservername));
            plugin.Info("channels.onscenechanged: " + string.Join(",", channels.onscenechanged));
            plugin.Info("--------------------");
            plugin.Info("channels.onscp914activate: " + string.Join(",", channels.onscp914activate));
            plugin.Info("channels.onstartcountdown: " + string.Join(",", channels.onstartcountdown));
            plugin.Info("channels.onstopcountdown: " + string.Join(",", channels.onstopcountdown));
            plugin.Info("channels.ondetonate: " + string.Join(",", channels.ondetonate));
            plugin.Info("channels.ondecontaminate: " + string.Join(",", channels.ondecontaminate));
            plugin.Info("--------------------");
            plugin.Info("channels.onplayerdie: " + string.Join(",", channels.onplayerdie));
            plugin.Info("channels.onplayerhurt: " + string.Join(",", channels.onplayerhurt));
            plugin.Info("channels.onplayerpickupitem: " + string.Join(",", channels.onplayerpickupitem));
            plugin.Info("channels.onplayerdropitem: " + string.Join(",", channels.onplayerdropitem));
            plugin.Info("channels.onplayerjoin: " + string.Join(",", channels.onplayerjoin));
            plugin.Info("channels.onnicknameset: " + string.Join(",", channels.onnicknameset));
            plugin.Info("channels.onassignteam: " + string.Join(",", channels.onassignteam));
            plugin.Info("channels.onsetrole: " + string.Join(",", channels.onsetrole));
            plugin.Info("channels.oncheckescape: " + string.Join(",", channels.oncheckescape));
            plugin.Info("channels.onspawn: " + string.Join(",", channels.onspawn));
            plugin.Info("channels.ondooraccess: " + string.Join(",", channels.ondooraccess));
            plugin.Info("channels.onintercom: " + string.Join(",", channels.onintercom));
            plugin.Info("channels.onintercomcooldowncheck: " + string.Join(",", channels.onintercomcooldowncheck));
            plugin.Info("channels.onpocketdimensionexit: " + string.Join(",", channels.onpocketdimensionexit));
            plugin.Info("channels.onpocketdimensionenter: " + string.Join(",", channels.onpocketdimensionenter));
            plugin.Info("channels.onpocketdimensiondie: " + string.Join(",", channels.onpocketdimensiondie));
            plugin.Info("channels.onthrowgrenade: " + string.Join(",", channels.onthrowgrenade));
            plugin.Info("channels.onplayerinfected: " + string.Join(",", channels.onplayerinfected));
            plugin.Info("channels.onspawnragdoll: " + string.Join(",", channels.onspawnragdoll));
            plugin.Info("channels.onlure: " + string.Join(",", channels.onlure));
            plugin.Info("channels.oncontain106: " + string.Join(",", channels.oncontain106));
            plugin.Info("channels.onmedkituse: " + string.Join(",", channels.onmedkituse));
            plugin.Info("channels.onshoot: " + string.Join(",", channels.onshoot));
            plugin.Info("channels.on106createportal: " + string.Join(",", channels.on106createportal));
            plugin.Info("channels.on106teleport: " + string.Join(",", channels.on106teleport));
            plugin.Info("channels.onelevatoruse: " + string.Join(",", channels.onelevatoruse));
            plugin.Info("channels.onhandcuff: " + string.Join(",", channels.onhandcuff));
            plugin.Info("channels.onplayertriggertesla: " + string.Join(",", channels.onplayertriggertesla));
            plugin.Info("channels.onscp914changeknob: " + string.Join(",", channels.onscp914changeknob));
            plugin.Info("--------------------");
            plugin.Info("channels.onadminquery: " + string.Join(",", channels.onadminquery));
            plugin.Info("channels.onauthcheck: " + string.Join(",", channels.onauthcheck));
            plugin.Info("channels.onban: " + string.Join(",", channels.onban));
            plugin.Info("--------------------");
            plugin.Info("channels.ondecideteamrespawnqueue: " + string.Join(",", channels.ondecideteamrespawnqueue));
            plugin.Info("channels.onsetrolemaxhp: " + string.Join(",", channels.onsetrolemaxhp));
            plugin.Info("channels.onteamrespawn: " + string.Join(",", channels.onteamrespawn));
            plugin.Info("channels.onsetscpconfig: " + string.Join(",", channels.onsetscpconfig));
            plugin.Info("channels.onsetntfunitname: " + string.Join(",", channels.onsetntfunitname));
            plugin.Info("############################");
        }

    }

    internal struct Bot
    {
        internal string ip;
        internal short port;
    }

    internal struct Settings
    {
        internal string language;
        internal bool playercount;
        internal string timestamp;
        internal bool verbose;
        internal bool metrics;
        internal bool configvalidation;
    }

    internal struct Channels
    {
        internal string[] statusmessages;
        internal string[] topic;

        internal string[] onroundstart;
        internal string[] onconnect;
        internal string[] ondisconnect;
        internal string[] oncheckroundend;
        internal string[] onroundend;
        internal string[] onwaitingforplayers;
        internal string[] onroundrestart;
        internal string[] onsetservername;
        internal string[] onscenechanged;

        internal string[] onscp914activate;
        internal string[] onstartcountdown;
        internal string[] onstopcountdown;
        internal string[] ondetonate;
        internal string[] ondecontaminate;

        internal string[] onplayerdie;
        internal string[] onplayerhurt;
        internal string[] onplayerpickupitem;
        internal string[] onplayerdropitem;
        internal string[] onplayerjoin;
        internal string[] onnicknameset;
        internal string[] onassignteam;
        internal string[] onsetrole;
        internal string[] oncheckescape;
        internal string[] onspawn;
        internal string[] ondooraccess;
        internal string[] onintercom;
        internal string[] onintercomcooldowncheck;
        internal string[] onpocketdimensionexit;
        internal string[] onpocketdimensionenter;
        internal string[] onpocketdimensiondie;
        internal string[] onthrowgrenade;
        internal string[] onplayerinfected;
        internal string[] onspawnragdoll;
        internal string[] onlure;
        internal string[] oncontain106;
        internal string[] onmedkituse;
        internal string[] onshoot;
        internal string[] on106createportal;
        internal string[] on106teleport;
        internal string[] onelevatoruse;
        internal string[] onhandcuff;
        internal string[] onplayertriggertesla;
        internal string[] onscp914changeknob;

        internal string[] onadminquery;
        internal string[] onauthcheck;
        internal string[] onban;

        internal string[] ondecideteamrespawnqueue;
        internal string[] onsetrolemaxhp;
        internal string[] onteamrespawn;
        internal string[] onsetscpconfig;
        internal string[] onsetntfunitname;
    }
}

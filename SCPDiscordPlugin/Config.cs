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
        internal static Bot bot;
        internal static Settings settings;
        internal static Dictionary<string, string> aliases = new Dictionary<string, string>();
        internal static Channels channels;

        internal static void Deserialise(SCPDiscordPlugin plugin, JObject configTree)
        {
            string currentNode = "";
            Thread.Sleep(1000);
            try
            {
                currentNode = "bot.ip";
                bot.ip = configTree.SelectToken("bot.ip").Value<string>();
                currentNode = "bot.port";
                bot.port = configTree.SelectToken("bot.port").Value<short>();
            }
            catch(Exception e)
            {
                plugin.Error("Failed to read bot config at node '" + currentNode + "'\n" + e);
            }

            try
            {
                currentNode = "settings.language";
                settings.language = configTree.SelectToken("settings.language").Value<string>();
                currentNode = "settings.playercount";
                settings.playercount = configTree.SelectToken("settings.playercount").Value<bool>();
                currentNode = "settings.timestamp";
                settings.timestamp = configTree.SelectToken("settings.timestamp").Value<string>();
                currentNode = "settings.verbose";
                settings.verbose = configTree.SelectToken("settings.verbose").Value<bool>();
                currentNode = "settings.metrics";
                settings.metrics = configTree.SelectToken("settings.metrics").Value<bool>();
            }
            catch(Exception e)
            {
                plugin.Error("Failed to read settings config at node '" + currentNode + "'\n" + e);
            }

            try
            {
                currentNode = "aliases";
                aliases = configTree.SelectToken("aliases").Value<JArray>().ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());

            }
            catch (Exception e)
            {
                plugin.Error("Failed to read alias config at node '" + currentNode + "'\n" + e);
            }

            try
            {
                currentNode = "channels.statusmessages";
                channels.statusmessages = configTree.SelectToken("channels.statusmessages").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.topic";
                channels.topic = configTree.SelectToken("channels.topic").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.onroundstart";
                channels.onroundstart = configTree.SelectToken("channels.onroundstart").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onconnect";
                channels.onconnect = configTree.SelectToken("channels.onconnect").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.ondisconnect";
                channels.ondisconnect = configTree.SelectToken("channels.ondisconnect").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.oncheckroundend";
                channels.oncheckroundend = configTree.SelectToken("channels.oncheckroundend").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onroundend";
                channels.onroundend = configTree.SelectToken("channels.onroundend").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onwaitingforplayers";
                channels.onwaitingforplayers = configTree.SelectToken("channels.onwaitingforplayers").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onroundrestart";
                channels.onroundrestart = configTree.SelectToken("channels.onroundrestart").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetservername";
                channels.onsetservername = configTree.SelectToken("channels.onsetservername").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onscenechanged";
                channels.onscenechanged = configTree.SelectToken("channels.onscenechanged").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.onscp914activate";
                channels.onscp914activate = configTree.SelectToken("channels.onscp914activate").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onstartcountdown";
                channels.onstartcountdown = configTree.SelectToken("channels.onstartcountdown").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onstopcountdown";
                channels.onstopcountdown = configTree.SelectToken("channels.onstopcountdown").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.ondetonate";
                channels.ondetonate = configTree.SelectToken("channels.ondetonate").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.ondecontaminate";
                channels.ondecontaminate = configTree.SelectToken("channels.ondecontaminate").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.onplayerdie";
                channels.onplayerdie = configTree.SelectToken("channels.onplayerdie").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerhurt";
                channels.onplayerhurt = configTree.SelectToken("channels.onplayerhurt").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerpickupitem";
                channels.onplayerpickupitem = configTree.SelectToken("channels.onplayerpickupitem").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerdropitem";
                channels.onplayerdropitem = configTree.SelectToken("channels.onplayerdropitem").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerjoin";
                channels.onplayerjoin = configTree.SelectToken("channels.onplayerjoin").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onnicknameset";
                channels.onnicknameset = configTree.SelectToken("channels.onnicknameset").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onassignteam";
                channels.onassignteam = configTree.SelectToken("channels.onassignteam").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetrole";
                channels.onsetrole = configTree.SelectToken("channels.onsetrole").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.oncheckescape";
                channels.oncheckescape = configTree.SelectToken("channels.oncheckescape").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onspawn";
                channels.onspawn = configTree.SelectToken("channels.onspawn").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.ondooraccess";
                channels.ondooraccess = configTree.SelectToken("channels.ondooraccess").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onintercom";
                channels.onintercom = configTree.SelectToken("channels.onintercom").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onintercomcooldowncheck";
                channels.onintercomcooldowncheck = configTree.SelectToken("channels.onintercomcooldowncheck").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onpocketdimensionexit";
                channels.onpocketdimensionexit = configTree.SelectToken("channels.onpocketdimensionexit").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onpocketdimensionenter";
                channels.onpocketdimensionenter = configTree.SelectToken("channels.onpocketdimensionenter").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onpocketdimensiondie";
                channels.onpocketdimensiondie = configTree.SelectToken("channels.onpocketdimensiondie").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onthrowgrenade";
                channels.onthrowgrenade = configTree.SelectToken("channels.onthrowgrenade").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayerinfected";
                channels.onplayerinfected = configTree.SelectToken("channels.onplayerinfected").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onspawnragdoll";
                channels.onspawnragdoll = configTree.SelectToken("channels.onspawnragdoll").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onlure";
                channels.onlure = configTree.SelectToken("channels.onlure").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.oncontain106";
                channels.oncontain106 = configTree.SelectToken("channels.oncontain106").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onmedkituse";
                channels.onmedkituse = configTree.SelectToken("channels.onmedkituse").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onshoot";
                channels.onshoot = configTree.SelectToken("channels.onshoot").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.on106createportal";
                channels.on106createportal = configTree.SelectToken("channels.on106createportal").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.on106teleport";
                channels.on106teleport = configTree.SelectToken("channels.on106teleport").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onelevatoruse";
                channels.onelevatoruse = configTree.SelectToken("channels.onelevatoruse").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onhandcuff";
                channels.onhandcuff = configTree.SelectToken("channels.onhandcuff").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onplayertriggertesla";
                channels.onplayertriggertesla = configTree.SelectToken("channels.onplayertriggertesla").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onscp914changeknob";
                channels.onscp914changeknob = configTree.SelectToken("channels.onscp914changeknob").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.onadminquery";
                channels.onadminquery = configTree.SelectToken("channels.onadminquery").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onauthcheck";
                channels.onauthcheck = configTree.SelectToken("channels.onauthcheck").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onban";
                channels.onban = configTree.SelectToken("channels.onban").Value<JArray>().Values<string>().ToArray();

                currentNode = "channels.ondecideteamrespawnqueue";
                channels.ondecideteamrespawnqueue = configTree.SelectToken("channels.ondecideteamrespawnqueue").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetrolemaxhp";
                channels.onsetrolemaxhp = configTree.SelectToken("channels.onsetrolemaxhp").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onteamrespawn";
                channels.onteamrespawn = configTree.SelectToken("channels.onteamrespawn").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetscpconfig";
                channels.onsetscpconfig = configTree.SelectToken("channels.onsetscpconfig").Value<JArray>().Values<string>().ToArray();
                currentNode = "channels.onsetntfunitname";
                channels.onsetntfunitname = configTree.SelectToken("channels.onsetntfunitname").Value<JArray>().Values<string>().ToArray();
            }
            catch (Exception e)
            {
                plugin.Error("Failed to read channel config at node '" + currentNode + "'\n" + e);
            }

            if(settings.verbose)
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

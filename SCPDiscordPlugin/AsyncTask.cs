using Newtonsoft.Json.Linq;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SCPDiscord
{
    class RefreshBotActivity
    {
        public RefreshBotActivity(SCPDiscord plugin)
        {
            //var message = (plugin.pluginManager.Server.NumPlayers - 1) + " / " + plugin.GetConfigString("max_players");

            //if (plugin.clientSocket == null || !plugin.clientSocket.Connected)
            //{
            //    if (plugin.hasConnectedOnce && Config.GetBool("settings.verbose"))
            //    {
            //        plugin.Warn("Error sending message '" + message + "' to bot: Not connected.");
            //    }
            //    return;
            //}

            //// Try to send the message to the bot
            //try
            //{
            //    NetworkStream serverStream = plugin.clientSocket.GetStream();
            //    byte[] outStream = System.Text.Encoding.UTF8.GetBytes("botactivity" + message + '\0');
            //    serverStream.Write(outStream, 0, outStream.Length);

            //    if (Config.GetBool("settings.verbose"))
            //    {
            //        plugin.Info("Sent activity '" + message + "' to bot.");
            //    }
            //}
            //catch (InvalidOperationException e)
            //{
            //    plugin.Error("Error sending activity '" + message + "' to bot.");
            //    plugin.Debug(e.ToString());
            //}
            //catch (ArgumentNullException e)
            //{
            //    plugin.Error("Error sending activity '" + message + "' to bot.");
            //    plugin.Debug(e.ToString());
            //}
        }
    }
    class RefreshChannelTopic
    {
        public RefreshChannelTopic(SCPDiscord plugin, string channelID, float tps)
        {
            //Dictionary<string, string> variables = new Dictionary<string, string>();
            //try
            //{
            //    Server server = plugin.pluginManager.Server;
            //    Dictionary<string, string> serverVariables;
            //    if (server != null)
            //    {
            //        serverVariables = new Dictionary<string, string>
            //        {
            //            { "players",           (server.NumPlayers - 1) + ""                                     },
            //            { "maxplayers",         plugin.GetConfigString("max_players")                           },
            //            { "ip",                 server.IpAddress                                                },
            //            { "port",               server.Port + ""                                                },
            //            { "isvisible",          server.Visible + ""                                             },
            //            { "isverified",         server.Verified + ""                                            },
            //            { "uptime",            (plugin.serverStartTime.ElapsedMilliseconds / 1000 / 60) + ""    },
            //            { "tps",                tps.ToString ("0.00")                                           }
            //        };
            //    }
            //    else
            //    {
            //        serverVariables = new Dictionary<string, string>
            //        {
            //            { "players",        "0"                 },
            //            { "maxplayers",     "0"                 },
            //            { "ip",             "---.---.---.---"   },
            //            { "port",           "----"              },
            //            { "isvisible",      "False"             },
            //            { "isverified",     "False"             },
            //            { "uptime",         "0"                 },
            //            { "tps",            tps.ToString("0.00")}
            //        };
            //    }

            //    Dictionary<string, string> mapVariables;
            //    if (server != null && server.Map != null)
            //    {
            //        mapVariables = new Dictionary<string, string>
            //        {
            //            { "warheaddetonated",   server.Map.WarheadDetonated + ""    },
            //            { "decontaminated",     server.Map.LCZDecontaminated + ""   }
            //        };
            //    }
            //    else
            //    {
            //        mapVariables = new Dictionary<string, string>
            //        {
            //            { "warheaddetonated",   "False" },
            //            { "decontaminated",     "False" }
            //        };
            //    }

            //    Dictionary<string, string> roundVariables;
            //    if (server != null && server.Round != null)
            //    {
            //        roundVariables = new Dictionary<string, string>
            //        {
            //            { "roundduration",     (server.Round.Duration / 60) + ""              },
            //            { "dclassalive",        server.Round.Stats.ClassDAlive + ""           },
            //            { "dclassdead",         server.Round.Stats.ClassDDead + ""            },
            //            { "dclassescaped",      server.Round.Stats.ClassDEscaped + ""         },
            //            { "dclassstart",        server.Round.Stats.ClassDStart + ""           },
            //            { "mtfalive",           server.Round.Stats.NTFAlive + ""              },
            //            { "scientistsalive",    server.Round.Stats.ScientistsAlive + ""       },
            //            { "scientistsdead",     server.Round.Stats.ScientistsDead + ""        },
            //            { "scientistsescaped",  server.Round.Stats.ScientistsEscaped + ""     },
            //            { "scientistsstart",    server.Round.Stats.ScientistsStart + ""       },
            //            { "scpalive",           server.Round.Stats.SCPAlive + ""              },
            //            { "scpdead",            server.Round.Stats.SCPDead + ""               },
            //            { "scpkills",           server.Round.Stats.SCPKills + ""              },
            //            { "scpstart",           server.Round.Stats.SCPStart + ""              },
            //            //{ "warheaddetonated",   server.Round.Stats.WarheadDetonated + ""      },
            //            { "zombies",            server.Round.Stats.Zombies + ""               }
            //        };
            //    }
            //    else
            //    {
            //        roundVariables = new Dictionary<string, string>
            //        {
            //            { "roundduration",      "0"     },
            //            { "dclassalive",        "0"     },
            //            { "dclassdead",         "0"     },
            //            { "dclassescaped",      "0"     },
            //            { "dclassstart",        "0"     },
            //            { "mtfalive",           "0"     },
            //            { "scientistsalive",    "0"     },
            //            { "scientistsdead",     "0"     },
            //            { "scientistsescaped",  "0"     },
            //            { "scientistsstart",    "0"     },
            //            { "scpalive",           "0"     },
            //            { "scpdead",            "0"     },
            //            { "scpkills",           "0"     },
            //            { "scpstart",           "0"     },
            //            //{ "warheaddetonated",   "0"     },
            //            { "zombies",            "0"     }
            //        };
            //    }

            //    foreach (var entry in serverVariables)
            //    {
            //        variables.Add(entry.Key, entry.Value);
            //    }

            //    foreach (var entry in mapVariables)
            //    {
            //        variables.Add(entry.Key, entry.Value);
            //    }

            //    foreach (var entry in roundVariables)
            //    {
            //        variables.Add(entry.Key, entry.Value);
            //    }


            //    var topic = Language.GetString("topic.message");

            //    topic = topic.Replace("\n", "");

            //    // Variable insertion
            //    foreach (KeyValuePair<string, string> variable in variables)
            //    {
            //        topic = topic.Replace("<var:" + variable.Key + ">", variable.Value);
            //    }

            //    // Regex replacements
            //    Dictionary<string, string> regex = Language.GetRegexDictionary("topic.regex");

            //    // Run the regex replacements
            //    foreach (KeyValuePair<string, string> entry in regex)
            //    {
            //        topic = topic.Replace(entry.Key, entry.Value);
            //    }

            //    // Try to send the message to the bot
            //    try
            //    {
            //        NetworkStream serverStream = plugin.clientSocket.GetStream();
            //        byte[] outStream = System.Text.Encoding.UTF8.GetBytes("channeltopic" + channelID + topic + '\0');
            //        serverStream.Write(outStream, 0, outStream.Length);

            //        if (Config.GetBool("settings.verbose"))
            //        {
            //            plugin.Info("Sent channel topic '" + topic + "' to bot.");
            //        }
            //    }
            //    catch (InvalidOperationException e)
            //    {
            //        plugin.Error("Error sending channel topic '" + topic + "' to bot.");
            //        plugin.Debug(e.ToString());
            //    }
            //    catch (ArgumentNullException e)
            //    {
            //        plugin.Error("Error sending channel topic '" + topic + "' to bot.");
            //        plugin.Debug(e.ToString());
            //    }
            //}
            //catch(Exception e)
            //{
            //    if(Config.GetBool("settings.verbose"))
            //    {
            //        plugin.Warn(e.ToString());
            //    }
            //}
        }
    }
}

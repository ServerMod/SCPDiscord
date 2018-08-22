using Newtonsoft.Json.Linq;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

//TODO: Clean up duplicated code in SendMessageToBot and RefreshChannelTopic

namespace SCPDiscord
{
    class SendMessageToBot
    {
        public SendMessageToBot(SCPDiscordPlugin plugin, string channelID, string messagePath, Dictionary<string, string> variables = null)
        {
            // Get unparsed message from config
            string message = "";
            try
            {
                message = plugin.language.GetString(messagePath + ".message");
            }
            catch (Exception e)
            {
                if(!(e is NullReferenceException))
                {
                    plugin.Error("Error reading base message" + e);
                }
                return;
            }

            // Abort on empty message
            if (message == null || message == "" || message == " " || message == ".")
            {
                plugin.Error("Tried to send empty message " + messagePath + " to discord. Verify your language file.");
                return;
            }

            // Abort if client is dead
            if (plugin.clientSocket == null || !plugin.clientSocket.Connected)
            {
                if(plugin.hasConnectedOnce && plugin.GetConfigBool("discord_verbose"))
                    plugin.Warn("Error sending message '" + message + "' to bot: Not connected.");
                return;
            }

            // Add time stamp
            if (plugin.GetConfigString("discord_formatting_date") != "off")
            {
                message = "[" + DateTime.Now.ToString(plugin.GetConfigString("discord_formatting_date")) + "]: " + message;
            }

            // Change the default keyword to the bot's representation of it
            if (channelID == "default")
            {
                channelID = "000000000000000000";
            }

            // Re-add newlines
            message = message.Replace("\\n", "\n");

            // Add variables //////////////////////////////
            if (variables != null)
            {
                // Variable insertion
                foreach (KeyValuePair<string, string> variable in variables)
                {
                    // Wait until after the regex replacements to add the player names
                    if(variable.Key == "servername" || variable.Key == "name" || variable.Key == "attackername" || variable.Key == "playername" || variable.Key == "adminname")
                    {
                        continue;
                    }
                    message = message.Replace("<var:" + variable.Key + ">", variable.Value);
                }
            }
            ///////////////////////////////////////////////

            // Global regex replacements //////////////////
            Dictionary<string, string> globalRegex = new Dictionary<string, string>();
            try
            {
                globalRegex = plugin.language.GetRegexDictionary("global_regex");
            }
            catch (Exception e)
            {
                plugin.Error("Error reading global regex" + e);
                return;
            }
            // Run the global regex replacements
            foreach (KeyValuePair<string, string> entry in globalRegex)
            {
                message = message.Replace(entry.Key, entry.Value);
            }
            ///////////////////////////////////////////////

            // Local regex replacements ///////////////////
            Dictionary<string, string> localRegex = new Dictionary<string, string>();
            try
            {
                localRegex = plugin.language.GetRegexDictionary(messagePath + ".regex");
            }
            catch (Exception e)
            {
                plugin.Error("Error reading local regex" + e);
                return;
            }
            // Run the local regex replacements
            foreach (KeyValuePair<string, string> entry in localRegex)
            {
                message = message.Replace(entry.Key, entry.Value);
            }
            ///////////////////////////////////////////////

            if (variables != null)
            {
                // Add names to the message ///////////////////
                foreach (KeyValuePair<string, string> variable in variables)
                {
                    if (variable.Key == "servername" || variable.Key == "name" || variable.Key == "attackername" || variable.Key == "playername" || variable.Key == "adminname")
                    {
                        message = message.Replace("<var:" + variable.Key + ">", variable.Value);
                    }
                }
                ///////////////////////////////////////////////

                // Final regex replacements ///////////////////
                Dictionary<string, string> finalRegex = new Dictionary<string, string>();
                try
                {
                    finalRegex = plugin.language.GetRegexDictionary("final_regex");
                }
                catch (Exception e)
                {
                    plugin.Error("Error reading final regex" + e);
                    return;
                }
                // Run the final regex replacements
                foreach (KeyValuePair<string, string> entry in finalRegex)
                {
                    message = message.Replace(entry.Key, entry.Value);
                }
                ///////////////////////////////////////////////
            }

            // Try to send the message to the bot
            try
            {
                NetworkStream serverStream = plugin.clientSocket.GetStream();
                byte[] outStream = System.Text.Encoding.UTF8.GetBytes(channelID + message + '\0');
                serverStream.Write(outStream, 0, outStream.Length);

                if (plugin.GetConfigBool("discord_verbose"))
                {
                    plugin.Info("Sent message '" + message + "' to bot.");
                }
            }
            catch (InvalidOperationException e)
            {
                plugin.Error("Error sending message '" + message + "' to bot.");
                plugin.Debug(e.ToString());
            }
            catch (ArgumentNullException e)
            {
                plugin.Error("Error sending message '" + message + "' to bot.");
                plugin.Debug(e.ToString());
            }
        }
    }
    class RefreshBotActivity
    {
        public RefreshBotActivity(SCPDiscordPlugin plugin)
        {
            var message = (plugin.pluginManager.Server.NumPlayers - 1) + " / " + plugin.GetConfigString("max_players");

            if (plugin.clientSocket == null || !plugin.clientSocket.Connected)
            {
                if (plugin.hasConnectedOnce && plugin.GetConfigBool("discord_verbose"))
                    plugin.Warn("Error sending message '" + message + "' to bot: Not connected.");
                return;
            }

            // Try to send the message to the bot
            try
            {
                NetworkStream serverStream = plugin.clientSocket.GetStream();
                byte[] outStream = System.Text.Encoding.UTF8.GetBytes("botactivity" + message + '\0');
                serverStream.Write(outStream, 0, outStream.Length);

                if (plugin.GetConfigBool("discord_verbose"))
                {
                    plugin.Info("Sent activity '" + message + "' to bot.");
                }
            }
            catch (InvalidOperationException e)
            {
                plugin.Error("Error sending activity '" + message + "' to bot.");
                plugin.Debug(e.ToString());
            }
            catch (ArgumentNullException e)
            {
                plugin.Error("Error sending activity '" + message + "' to bot.");
                plugin.Debug(e.ToString());
            }
        }
    }
    class RefreshChannelTopic
    {
        public RefreshChannelTopic(SCPDiscordPlugin plugin, string channelID)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>();
            try
            {
                Server server = plugin.pluginManager.Server;
                Dictionary<string, string> serverVariables;
                if (server != null)
                {
                    serverVariables = new Dictionary<string, string>
                    {
                        { "players",           (server.NumPlayers - 1) + ""                                     },
                        { "maxplayers",         plugin.GetConfigString("max_players")                           },
                        { "ip",                 server.IpAddress                                                },
                        { "port",               server.Port + ""                                                },
                        { "isvisible",          server.Visible + ""                                             },
                        { "isverified",         server.Verified + ""                                            },
                        { "uptime",            (plugin.serverStartTime.ElapsedMilliseconds / 1000 / 60) + ""    }
                    };
                }
                else
                {
                    serverVariables = new Dictionary<string, string>
                    {
                        { "players",        "0"                 },
                        { "maxplayers",     "0"                 },
                        { "ip",             "---.---.---.---"   },
                        { "port",           "----"              },
                        { "isvisible",      "False"             },
                        { "isverified",     "False"             },
                        { "uptime",         "0"                 }
                    };
                }

                Dictionary<string, string> mapVariables;
                if (server != null && server.Map != null)
                {
                    mapVariables = new Dictionary<string, string>
                    {
                        { "warheaddetonated",   server.Map.WarheadDetonated + ""    },
                        { "decontaminated",     server.Map.LCZDecontaminated + ""   }
                    };
                }
                else
                {
                    mapVariables = new Dictionary<string, string>
                    {
                        { "warheaddetonated",   "False" },
                        { "decontaminated",     "False" }
                    };
                }

                Dictionary<string, string> roundVariables;
                if (server != null && server.Round != null)
                {
                    roundVariables = new Dictionary<string, string>
                    {
                        { "roundduration",     (server.Round.Duration / 60) + ""              },
                        { "dclassalive",        server.Round.Stats.ClassDAlive + ""           },
                        { "dclassdead",         server.Round.Stats.ClassDDead + ""            },
                        { "dclassescaped",      server.Round.Stats.ClassDEscaped + ""         },
                        { "dclassstart",        server.Round.Stats.ClassDStart + ""           },
                        { "mtfalive",           server.Round.Stats.NTFAlive + ""              },
                        { "scientistsalive",    server.Round.Stats.ScientistsAlive + ""       },
                        { "scientistsdead",     server.Round.Stats.ScientistsDead + ""        },
                        { "scientistsescaped",  server.Round.Stats.ScientistsEscaped + ""     },
                        { "scientistsstart",    server.Round.Stats.ScientistsStart + ""       },
                        { "scpalive",           server.Round.Stats.SCPAlive + ""              },
                        { "scpdead",            server.Round.Stats.SCPDead + ""               },
                        { "scpkills",           server.Round.Stats.SCPKills + ""              },
                        { "scpstart",           server.Round.Stats.SCPStart + ""              },
                        //{ "warheaddetonated",   server.Round.Stats.WarheadDetonated + ""      },
                        { "zombies",            server.Round.Stats.Zombies + ""               }
                    };
                }
                else
                {
                    roundVariables = new Dictionary<string, string>
                    {
                        { "roundduration",      "0"     },
                        { "dclassalive",        "0"     },
                        { "dclassdead",         "0"     },
                        { "dclassescaped",      "0"     },
                        { "dclassstart",        "0"     },
                        { "mtfalive",           "0"     },
                        { "scientistsalive",    "0"     },
                        { "scientistsdead",     "0"     },
                        { "scientistsescaped",  "0"     },
                        { "scientistsstart",    "0"     },
                        { "scpalive",           "0"     },
                        { "scpdead",            "0"     },
                        { "scpkills",           "0"     },
                        { "scpstart",           "0"     },
                        //{ "warheaddetonated",   "0"     },
                        { "zombies",            "0"     }
                    };
                }

                foreach (var entry in serverVariables)
                    variables.Add(entry.Key, entry.Value);
                foreach (var entry in mapVariables)
                    variables.Add(entry.Key, entry.Value);
                foreach (var entry in roundVariables)
                    variables.Add(entry.Key, entry.Value);

                var topic = plugin.GetConfigString("discord_server_status");

                topic = topic.Replace("\n", "");

                // Variable insertion
                foreach (KeyValuePair<string, string> variable in variables)
                {
                    topic = topic.Replace("<var:" + variable.Key + ">", variable.Value);
                }

                // Regex replacements
                Dictionary<string, string> regex = plugin.GetConfigDict("discord_server_status_regex");

                // Run the regex replacements
                foreach (KeyValuePair<string, string> entry in regex)
                {
                    topic = topic.Replace(entry.Key, entry.Value);
                }

                // Change the default keyword to the bot's representation of it
                if (channelID == "default")
                {
                    channelID = "000000000000000000";
                }

                // Try to send the message to the bot
                try
                {
                    NetworkStream serverStream = plugin.clientSocket.GetStream();
                    byte[] outStream = System.Text.Encoding.UTF8.GetBytes("channeltopic" + channelID + topic + '\0');
                    serverStream.Write(outStream, 0, outStream.Length);

                    if (plugin.GetConfigBool("discord_verbose"))
                    {
                        plugin.Info("Sent channel topic '" + topic + "' to bot.");
                    }
                }
                catch (InvalidOperationException e)
                {
                    plugin.Error("Error sending channel topic '" + topic + "' to bot.");
                    plugin.Debug(e.ToString());
                }
                catch (ArgumentNullException e)
                {
                    plugin.Error("Error sending channel topic '" + topic + "' to bot.");
                    plugin.Debug(e.ToString());
                }
            }
            catch(Exception e)
            {
                if(plugin.GetConfigBool("discord_verbose"))
                    plugin.Warn(e.ToString());
            }


        }
    }
    class ConnectToBot
    {
        //This is ran once on the first time connecting to the bot
        public ConnectToBot(SCPDiscordPlugin plugin)
        {
            Thread.Sleep(2000);
            while (!plugin.clientSocket.Connected)
            {
                plugin.Info("Attempting Bot Connection...");
                try
                {
                    plugin.Info("Your Bot IP: " + plugin.GetConfigString("discord_bot_ip") + ". Your Bot Port: " + plugin.GetConfigInt("discord_bot_port") + ".");
                    plugin.clientSocket.Connect(plugin.GetConfigString("discord_bot_ip"), plugin.GetConfigInt("discord_bot_port"));
                }
                catch (SocketException e)
                {
                    plugin.Info("Error occured while connecting to discord bot server.");
                    plugin.Debug(e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ObjectDisposedException e)
                {
                    plugin.Info("TCP client was unexpectedly closed.");
                    plugin.Debug(e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    plugin.Info("Invalid port.");
                    plugin.Debug(e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ArgumentNullException e)
                {
                    plugin.Info("IP address is null.");
                    plugin.Debug(e.ToString());
                    Thread.Sleep(5000);
                }
            }
            plugin.Info("Connected to Discord bot.");
            plugin.SendMessageToBot("default", "botmessages.connectedtobot");
            plugin.hasConnectedOnce = true;
        }
    }
    class StartConnectionWatchdog
    {
        //This is a loop that keeps running and checks if the bot has been disconnected
        public StartConnectionWatchdog(SCPDiscordPlugin plugin)
        {
            while (true)
            {
                Thread.Sleep(200);
                if(!plugin.clientSocket.Connected && plugin.hasConnectedOnce)
                {
                    plugin.Info("Discord bot connection issue detected, attempting reconnect...");
                    try
                    {
                        plugin.Info("Your Bot IP: " + plugin.GetConfigString("discord_bot_ip") + ". Your Bot Port: " + plugin.GetConfigInt("discord_bot_port") + ".");
                        plugin.clientSocket = new TcpClient(plugin.GetConfigString("discord_bot_ip"), plugin.GetConfigInt("discord_bot_port"));
                        plugin.Info("Reconnected to Discord bot.");
                        plugin.SendMessageToBot("default", "botmessages.reconnectedtobot");
                    }
                    catch (SocketException e)
                    {
                        plugin.Info("Error occured while reconnecting to discord bot server.");
                        plugin.Debug(e.ToString());
                        Thread.Sleep(5000);
                    }
                    catch (ObjectDisposedException e)
                    {
                        plugin.Info("TCP client was unexpectedly closed.");
                        plugin.Debug(e.ToString());
                        Thread.Sleep(5000);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        plugin.Info("Invalid port.");
                        plugin.Debug(e.ToString());
                        Thread.Sleep(5000);
                    }
                    catch (ArgumentNullException e)
                    {
                        plugin.Info("IP address is null.");
                        plugin.Debug(e.ToString());
                        Thread.Sleep(5000);
                    }
                }

            }
        }
    }
}

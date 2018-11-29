using Smod2.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace SCPDiscord
{
    // Seperate class to run the thread
    public class StartNetworkSystem
    {
        public StartNetworkSystem(SCPDiscord plugin)
        {
            NetworkSystem.Run(plugin);
        }
    }

    public class ProcessMessageAsync
    {
        public ProcessMessageAsync(string channelID, string messagePath, Dictionary<string, string> variables)
        {
            NetworkSystem.ProcessMessage(channelID, messagePath, variables);
        }
    }

    public class QueueMessageAsync
    {
        public QueueMessageAsync(string channelID, string messagePath, Dictionary<string, string> variables)
        {
            NetworkSystem.ProcessMessage(channelID, messagePath, variables);
        }
    }

    public static class NetworkSystem
    {
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<string> messageQueue = new List<string>();
        private static SCPDiscord plugin;
        private static Stopwatch topicUpdateTimer;
        public static void Run(SCPDiscord plugin)
        {
            topicUpdateTimer = Stopwatch.StartNew();
            topicUpdateTimer.Start();
            NetworkSystem.plugin = plugin;
            while(!Config.ready || !Language.ready)
            {
                Thread.Sleep(1000);
            }

            Thread messageThread = new Thread(new ThreadStart(() => new BotListener(plugin)));
            messageThread.Start();

            while (!plugin.shutdown)
            {
                try
                {
                    if(IsConnected())
                    {
                        Update(plugin);
                    }
                    else
                    {
                        Connect(Config.GetString("bot.ip"), Config.GetInt("bot.port"));
                    }
                    Thread.Sleep(1000);
                }
                catch(Exception)
                {
                    plugin.Warn("Network error caught, if this happens a lot try using the 'scpd_rc' command.");
                }
            }
        }

        private static void Update(SCPDiscord plugin)
        {
            if (topicUpdateTimer.ElapsedMilliseconds >= 10000)
            {
                topicUpdateTimer.Reset();
                topicUpdateTimer.Start();
                float tps = TickCounter.Reset() / 10.0f;
                
                // Update player count
                if (Config.GetBool("settings.playercount"))
                {
                    string activity = "botactivity" + (plugin.pluginManager.Server.NumPlayers - 1) + " / " + plugin.GetConfigString("max_players");
                    QueueMessage(activity);
                }

                // Update channel topic
                foreach (string channel in Config.GetArray("channels.topic"))
                {
                    if (Config.GetDict("aliases").ContainsKey(channel))
                    {
                        RefreshChannelTopic(plugin, Config.GetDict("aliases")[channel], tps);
                    }
                }
            }

            // Send all messages
            for (int i = 0; i < messageQueue.Count; i++)
            {
                if(SendMessage(messageQueue[i]))
                {
                    messageQueue.RemoveAt(i);
                    i--;
                }
            }

            if(messageQueue.Count != 0 && Config.GetBool("settings.verbose"))
            {
                plugin.Warn("Warn could not send all messages.");
            }
        }

        /// Connection functions //////////////////////////
        public static bool IsConnected()
        {
            if(socket == null)
            {
                return false;
            }
            return !((socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
        }

        private static void Connect(string address, int port)
        {
            if (Config.GetBool("settings.verbose"))
            {
                plugin.Info("Attempting Bot Connection...");
            }
            try
            {
                if(socket != null && socket.IsBound)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                else
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Info("Your Bot IP: " + Config.GetString("bot.ip") + ". Your Bot Port: " + Config.GetInt("bot.port") + ".");
                }
                socket.Connect(Config.GetString("bot.ip"), Config.GetInt("bot.port"));
                plugin.Info("Connected to Discord bot.");
                plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botmessages.connectedtobot");
            }
            catch (SocketException e)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Error("Error occured while connecting to discord bot server.");
                    plugin.Error(e.ToString());
                }
                Thread.Sleep(5000);
            }
            catch (ObjectDisposedException e)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Error("TCP client was unexpectedly closed.");
                    plugin.Error(e.ToString());
                }
                Thread.Sleep(5000);
            }
            catch (ArgumentOutOfRangeException e)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Error("Invalid port.");
                    plugin.Error(e.ToString());
                }
                Thread.Sleep(5000);
            }
            catch (ArgumentNullException e)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Error("IP address is null.");
                    plugin.Error(e.ToString());
                }
                Thread.Sleep(5000);
            }
        }

        public static void Disconnect()
        {
            socket.Disconnect(false);
        }
        /// ///////////////////////////////////////////////

        /// Message functions /////////////////////////////
        private static bool SendMessage(string message)
        {
            // Abort if client is dead
            if (socket == null || !socket.Connected)
            {
                if(Config.GetBool("settings.verbose"))
                {
                    plugin.Warn("Error sending message '" + message + "' to bot: Not connected.");
                }
                return false;
            }

            // Try to send the message to the bot
            try
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message + '\0');
                socket.Send(data);

                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Info("Sent message '" + message + "' to bot.");
                }
                return true;
            }
            catch (Exception e)
            {
                plugin.Error("Error sending message '" + message + "' to bot.");
                plugin.Error(e.ToString());
                if (!(e is InvalidOperationException || e is ArgumentNullException || e is SocketException))
                {
                    throw e;
                }
            }
            return false;
        }

        public static bool ProcessMessage(string channelID, string messagePath, Dictionary<string,string> variables)
        {
            // Get unparsed message from config
            string message = "";
            try
            {
                message = Language.GetString(messagePath + ".message");
            }
            catch (Exception e)
            {
                plugin.Error("Error reading base message" + e);
                return false;
            }

            // An error mesage is already sent in the language function if this is null, so this just returns
            if(message == null)
            {
                return false;
            }

            // Abort on empty message
            if (message == "" || message == " " || message == ".")
            {
                if(Config.GetBool("settings.verbose"))
                {
                    plugin.Warn("Tried to send empty message " + messagePath + " to discord. Verify your language files.");
                }
                return false;
            }
            
            // Add time stamp
            if (Config.GetString("settings.timestamp") != "off")
            {
                message = "[" + DateTime.Now.ToString(Config.GetString("settings.timestamp")) + "]: " + message;
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
                    if (variable.Key == "servername" || variable.Key == "name" || variable.Key == "attackername" || variable.Key == "playername" || variable.Key == "adminname" || variable.Key == "feedback")
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
                globalRegex = Language.GetRegexDictionary("global_regex");
            }
            catch (Exception e)
            {
                plugin.Error("Error reading global regex" + e);
                return false;
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
                localRegex = Language.GetRegexDictionary(messagePath + ".regex");
            }
            catch (Exception e)
            {
                plugin.Error("Error reading local regex" + e);
                return false;
            }
            // Run the local regex replacements
            foreach (KeyValuePair<string, string> entry in localRegex)
            {
                message = message.Replace(entry.Key, entry.Value);
            }
            ///////////////////////////////////////////////

            if (variables != null)
            {
                // Add names/command feedback to the message //
                foreach (KeyValuePair<string, string> variable in variables)
                {
                    if (variable.Key == "servername" || variable.Key == "name" || variable.Key == "attackername" || variable.Key == "playername" || variable.Key == "adminname" || variable.Key == "feedback")
                    {
                        message = message.Replace("<var:" + variable.Key + ">", EscapeDiscordFormatting(variable.Value));
                    }
                }
                ///////////////////////////////////////////////

                // Final regex replacements ///////////////////
                Dictionary<string, string> finalRegex = new Dictionary<string, string>();
                try
                {
                    finalRegex = Language.GetRegexDictionary("final_regex");
                }
                catch (Exception e)
                {
                    plugin.Error("Error reading final regex" + e);
                    return false;
                }
                // Run the final regex replacements
                foreach (KeyValuePair<string, string> entry in finalRegex)
                {
                    message = message.Replace(entry.Key, entry.Value);
                }
                ///////////////////////////////////////////////
            }

            messageQueue.Add(channelID + message);
            return true;
        }

        public static void QueueMessage(string message)
        {
            messageQueue.Add(message);
        }

        private static string EscapeDiscordFormatting(string input)
        {
            input = input.Replace("`", "\\`");
            input = input.Replace("*", "\\*");
            input = input.Replace("_", "\\_");
            input = input.Replace("~", "\\~");
            return input;
        }
        
        public static int Receive(out byte[] data)
        {
            data = new byte[1000];
            return socket.Receive(data);
        }
        /// ///////////////////////////////////////////////

        /// Channel topic refreshing
        private static void RefreshChannelTopic(SCPDiscord plugin, string channelID, float tps)
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
                        { "uptime",            (plugin.serverStartTime.ElapsedMilliseconds / 1000 / 60) + ""    },
                        { "tps",                tps.ToString ("0.00")                                           }
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
                        { "uptime",         "0"                 },
                        { "tps",            tps.ToString("0.00")}
                    };
                }

                Dictionary<string, string> mapVariables;
                if (server != null && server.Map != null)
                {
                    mapVariables = new Dictionary<string, string>
                    {
                        //{ "warheaddetonated",   server.Map.WarheadDetonated + ""    },
                        { "decontaminated",     server.Map.LCZDecontaminated + ""   }
                    };
                }
                else
                {
                    mapVariables = new Dictionary<string, string>
                    {
                        //{ "warheaddetonated",   "False" },
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
                        { "warheaddetonated",   server.Round.Stats.WarheadDetonated + ""      },
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
                        { "warheaddetonated",   "0"     },
                        { "zombies",            "0"     }
                    };
                }

                foreach (var entry in serverVariables)
                {
                    variables.Add(entry.Key, entry.Value);
                }

                foreach (var entry in mapVariables)
                {
                    variables.Add(entry.Key, entry.Value);
                }

                foreach (var entry in roundVariables)
                {
                    variables.Add(entry.Key, entry.Value);
                }


                var topic = Language.GetString("topic.message");

                topic = topic.Replace("\n", "");

                // Variable insertion
                foreach (KeyValuePair<string, string> variable in variables)
                {
                    topic = topic.Replace("<var:" + variable.Key + ">", variable.Value);
                }

                // Regex replacements
                Dictionary<string, string> regex = Language.GetRegexDictionary("topic.regex");

                // Run the regex replacements
                foreach (KeyValuePair<string, string> entry in regex)
                {
                    topic = topic.Replace(entry.Key, entry.Value);
                }

                // Try to send the message to the bot
                try
                {
                    QueueMessage("channeltopic" + channelID + topic);

                    if (Config.GetBool("settings.verbose"))
                    {
                        plugin.Info("Sent channel topic '" + topic + "' to bot.");
                    }
                }
                catch (InvalidOperationException e)
                {
                    if (Config.GetBool("settings.verbose"))
                    {
                        plugin.Error("Error sending channel topic '" + topic + "' to bot.");
                        plugin.Error(e.ToString());
                    }
                }
                catch (ArgumentNullException e)
                {
                    if (Config.GetBool("settings.verbose"))
                    {
                        plugin.Error("Error sending channel topic '" + topic + "' to bot.");
                        plugin.Error(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Error(e.ToString());
                }
            }
        }
    }
}

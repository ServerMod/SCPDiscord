using Smod2.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using SCPDiscord.EventListeners;

namespace SCPDiscord
{
    // Separate class to run the thread
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

    public static class NetworkSystem
    {
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<string> messageQueue = new List<string>();
        private static SCPDiscord plugin;
        private static Stopwatch topicUpdateTimer;
        public static void Run(SCPDiscord pl)
        {
	        plugin = pl;
            topicUpdateTimer = Stopwatch.StartNew();
            topicUpdateTimer.Start();
            while(!Config.ready || !Language.ready)
            {
                Thread.Sleep(1000);
            }

            // ReSharper disable once ObjectCreationAsStatement
            Thread messageThread = new Thread(() => new BotListener(plugin));
            messageThread.Start();

            while (!plugin.shutdown)
            {
                try
                {
                    if(IsConnected())
                    {
                        Update();
                    }
                    else
                    {
                        Connect();
                    }
                    Thread.Sleep(1000);
                }
                catch(Exception e)
                {
                    plugin.Error("Network error caught, if this happens a lot try using the 'scpd_rc' command." + e);
                }
            }
        }

        private static void Update()
        {
            if (topicUpdateTimer.ElapsedMilliseconds >= 10000)
            {
                topicUpdateTimer.Reset();
                topicUpdateTimer.Start();
                float tps = TickCounter.Reset() / 10.0f;
                
                // Update player count
                if (Config.GetBool("settings.playercount"))
                {
                    string activity = "botactivity" + (plugin.PluginManager.Server.NumPlayers - 1) + " / " + plugin.maxPlayers;
                    QueueMessage(activity);
                }

                // Update channel topic
                foreach (string channel in Config.GetArray("channels.topic"))
                {
                    if (Config.GetDict("aliases").ContainsKey(channel))
                    {
                        RefreshChannelTopic(Config.GetDict("aliases")[channel], tps);
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

            if(messageQueue.Count != 0)
            {
                plugin.VerboseWarn("Could not send all messages.");
            }
        }

        /// Connection functions //////////////////////////
        public static bool IsConnected()
        {
            if(socket == null)
            {
                return false;
            }

            try
            {
                return !((socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
            }
            catch (ObjectDisposedException e)
            {
                plugin.VerboseError("TCP client was unexpectedly closed.");
                plugin.VerboseError(e.ToString());
                return false;
            }
        }

        private static void Connect()
        {
            plugin.Verbose("Attempting Bot Connection...");

            while(!IsConnected())
            {
                try
                {
                    if(socket != null && socket.IsBound)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    plugin.Verbose("Your Bot IP: " + Config.GetString("bot.ip") + ". Your Bot Port: " + Config.GetInt("bot.port") + ".");
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(Config.GetString("bot.ip"), Config.GetInt("bot.port"));
                    plugin.Info("Connected to Discord bot.");
                    plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botmessages.connectedtobot");
                }
                catch (SocketException e)
                {
                    plugin.VerboseError("Error occured while connecting to discord bot server: " + e.Message);
                    plugin.DebugError(e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ObjectDisposedException e)
                {
                    plugin.VerboseError("TCP client was unexpectedly closed.");
                    plugin.DebugError(e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    plugin.VerboseError("Invalid port.");
                    plugin.DebugError(e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ArgumentNullException e)
                {
                    plugin.VerboseError("IP address is null.");
                    plugin.DebugError(e.ToString());
                    Thread.Sleep(5000);
                }
            }
        }

        public static void Disconnect()
        {
            socket.Disconnect(false);
            //socket = null;
        }
        /// ///////////////////////////////////////////////

        /// Message functions /////////////////////////////
        private static bool SendMessage(string message)
        {
            // Abort if client is dead
            if (socket == null || !socket.Connected)
            {
                plugin.VerboseWarn("Error sending message '" + message + "' to bot: Not connected.");
                return false;
            }

            // Try to send the message to the bot
            try
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message + '\0');
                socket.Send(data);

                plugin.Debug("Sent message '" + message + "' to bot.");
                return true;
            }
            catch (Exception e)
            {
                plugin.Error("Error sending message '" + message + "' to bot.");
                plugin.Error(e.ToString());
                if (!(e is InvalidOperationException || e is ArgumentNullException || e is SocketException))
                {
                    throw;
                }
            }
            return false;
        }

        public static void ProcessMessage(string channelID, string messagePath, Dictionary<string, string> variables)
        {
            // Get unparsed message from config
            string message;
            try
            {
                message = Language.GetString(messagePath + ".message");
            }
            catch (Exception e)
            {
	            plugin.Error("Error reading base message" + e);
	            return;
            }

            switch (message)
            {
	            // An error message is already sent in the language function if this is null, so this just returns
	            case null:
		            return;
	            // Abort on empty message
	            case "":
	            case " ":
	            case ".":
		            plugin.VerboseWarn("Tried to send empty message " + messagePath + " to discord. Verify your language files.");
		            return;
            }

            // Add time stamp
            if (Config.GetString("settings.timestamp") != "off" && Config.GetString("settings.timestamp") != "")
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
                    if (variable.Key == "servername" || variable.Key == "name" || variable.Key == "attackername" || variable.Key == "playername" || variable.Key == "adminname" || variable.Key == "feedback" || variable.Key == "admintag")
                    {
                        continue;
                    }
                    message = message.Replace("<var:" + variable.Key + ">", variable.Value);
                }
            }
            ///////////////////////////////////////////////

            // Global regex replacements //////////////////
            Dictionary<string, string> globalRegex;
            try
            {
                globalRegex = Language.GetRegexDictionary("global_regex");
            }
            catch (Exception e)
            {
	            plugin.Error("Error reading global regex" + e);
	            return;
            }
            // Run the global regex replacements
            foreach (KeyValuePair<string, string> entry in globalRegex)
            {
                message = Regex.Replace(message, entry.Key, entry.Value);
            }
            ///////////////////////////////////////////////

            // Local regex replacements ///////////////////
            Dictionary<string, string> localRegex;
            try
            {
                localRegex = Language.GetRegexDictionary(messagePath + ".regex");
            }
            catch (Exception e)
            {
	            plugin.Error("Error reading local regex" + e);
	            return;
            }
            // Run the local regex replacements
            foreach (KeyValuePair<string, string> entry in localRegex)
            {
                message = Regex.Replace(message, entry.Key, entry.Value);
            }
            ///////////////////////////////////////////////

            if (variables != null)
            {
                // Add names/command feedback to the message //
                foreach (KeyValuePair<string, string> variable in variables)
                {
                    message = message.Replace("<var:" + variable.Key + ">", EscapeDiscordFormatting(variable.Value));
                }
                ///////////////////////////////////////////////

                // Final regex replacements ///////////////////
                Dictionary<string, string> finalRegex;
                try
                {
                    finalRegex = Language.GetRegexDictionary("final_regex");
                }
                catch (Exception e)
                {
	                plugin.Error("Error reading final regex" + e);
	                return;
                }
                // Run the final regex replacements
                foreach (KeyValuePair<string, string> entry in finalRegex)
                {
                    message = Regex.Replace(message, entry.Key, entry.Value);
                }
                ///////////////////////////////////////////////
            }

            messageQueue.Add(channelID + message);
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
        
        public static int Receive(byte[] data)
        {
            return socket.Receive(data);
        }
        /// ///////////////////////////////////////////////

        /// Channel topic refreshing //////////////////////
        private static void RefreshChannelTopic(string channelID, float tps)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>();
            try
            {
                Server server = plugin.PluginManager.Server;
                Dictionary<string, string> serverVariables;
                if (server != null)
                {
                    serverVariables = new Dictionary<string, string>
                    {
                        { "players",           (server.NumPlayers - 1) + ""                                     },
                        { "maxplayers",         plugin.maxPlayers + ""                                          },
                        { "ip",                 server.IpAddress                                                },
                        { "port",               server.Port + ""                                                },
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

                Dictionary<string, string> mapVariables = new Dictionary<string, string>();
                try
                {
                    mapVariables = new Dictionary<string, string>
                    {
                        { "decontaminated",     (server?.Map != null && server.Map.LCZDecontaminated) + ""   }
                    };
                }
                catch(Exception e)
                {
                    plugin.VerboseError("Server: " + (server != null) + " Server.Map: " + (server?.Map != null) + " Server.Map.LCZDecontaminated: " + (server?.Map?.LCZDecontaminated != null));
                    plugin.DebugError(e.ToString());
                }


                Dictionary<string, string> roundVariables;
                if (server?.Round != null)
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

                foreach (KeyValuePair<string, string> entry in serverVariables)
                {
                    variables.Add(entry.Key, entry.Value);
                }

                foreach (KeyValuePair<string, string> entry in mapVariables)
                {
                    variables.Add(entry.Key, entry.Value);
                }

                foreach (KeyValuePair<string, string> entry in roundVariables)
                {
                    variables.Add(entry.Key, entry.Value);
                }


                string topic = Language.GetString("topic.message");

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
                    topic = Regex.Replace(topic, entry.Key, entry.Value);
                }

                // Try to send the message to the bot
                try
                {
                    QueueMessage("channeltopic" + channelID + topic);

                    plugin.Debug("Sent channel topic '" + topic + "' to bot.");
                }
                catch (InvalidOperationException e)
                {
                    plugin.VerboseError("Error sending channel topic '" + topic + "' to bot.");
                    plugin.DebugError(e.ToString());
                }
                catch (ArgumentNullException e)
                {
                    plugin.VerboseError("Error sending channel topic '" + topic + "' to bot.");
                    plugin.DebugError(e.ToString());
                }
            }
            catch (Exception e)
            {
                plugin.DebugError(e.ToString());
            }
        }
    }
}

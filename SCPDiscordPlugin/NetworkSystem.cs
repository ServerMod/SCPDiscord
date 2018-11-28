using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

    public static class NetworkSystem
    {
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<string> messageQueue = new List<string>();
        private static SCPDiscord plugin;
        public static void Run(SCPDiscord plugin)
        {
            NetworkSystem.plugin = plugin;
            while(!Config.ready || !Language.ready)
            {
                Thread.Sleep(1000);
            }
            while(!plugin.shutdown)
            {
                if(IsConnected())
                {
                    Update(plugin);
                }
                else
                {
                    Connect(Config.GetString("bot.ip"), Config.GetInt("bot.port"));
                }
                Thread.Sleep(2000);
            }
        }

        private static void Update(SCPDiscord plugin)
        {
            for(int i = 0; i < messageQueue.Count; i++)
            {
                if(SendMessage(messageQueue[i]))
                {
                    messageQueue.RemoveAt(i);
                    i--;
                }
            }

            if(messageQueue.Count != 0 && Config.GetBool("verbose"))
            {
                plugin.Warn("Warn could not send all messages.");
            }
        }

        public static bool IsConnected()
        {
            return socket.Connected;
        }

        private static void Connect(string address, int port)
        {
            if (Config.GetBool("settings.verbose"))
            {
                plugin.Info("Attempting Bot Connection...");
            }
            try
            {
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
                    plugin.Info("Error occured while connecting to discord bot server.");
                    plugin.Debug(e.ToString());
                }
                Thread.Sleep(5000);
            }
            catch (ObjectDisposedException e)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Info("TCP client was unexpectedly closed.");
                    plugin.Debug(e.ToString());
                }
                Thread.Sleep(5000);
            }
            catch (ArgumentOutOfRangeException e)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Info("Invalid port.");
                    plugin.Debug(e.ToString());
                }
                Thread.Sleep(5000);
            }
            catch (ArgumentNullException e)
            {
                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Info("IP address is null.");
                    plugin.Debug(e.ToString());
                }
                Thread.Sleep(5000);
            }
        }

        public static void Disconnect()
        {
            socket.Disconnect(false);
        }

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
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException || e is ArgumentNullException)
                {
                    plugin.Error("Error sending message '" + message + "' to bot.");
                    plugin.Debug(e.ToString());
                }
                else
                {
                    plugin.Error("Error sending message '" + message + "' to bot. Exception: " + e);
                    plugin.Debug(e.ToString());
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

        private static string EscapeDiscordFormatting(string input)
        {
            input = input.Replace("`", "\\`");
            input = input.Replace("*", "\\*");
            input = input.Replace("_", "\\_");
            input = input.Replace("~", "\\~");
            return input;
        }
    }
}

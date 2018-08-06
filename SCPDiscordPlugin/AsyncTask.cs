using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace SCPDiscord
{
    class AsyncParsedMessage
    {
        public AsyncParsedMessage(SCPDiscordPlugin plugin, string channelID, string messagePath, Dictionary<string, string> variables = null)
        {
            // Check if node exists
            JToken eventNode = plugin.messageConfig.root.SelectToken(messagePath); 
            if (eventNode == null)
            {
                plugin.Error("Error reading message from language file: " + messagePath);
                return;
            }

            // Get unparsed message from config
            string message = eventNode.Value<string>("message");

            // Abort on empty message
            if (message == null || message == "" || message == " " || message == ".")
            {
                plugin.Error("Tried to send empty message to discord. Verify your language file.");
                return;
            }

            // Abort if client is dead
            if (plugin.clientSocket == null || !plugin.clientSocket.Connected)
            {
                if(plugin.hasConnectedOnce)
                    plugin.Warn("Error sending message '" + message + "' to discord: Not connected to bot.");
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

            if(variables != null)
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

            // Global regex replacements
            try
            {
                // Gets the regex array as a JArray and then converts it to a Dictionary of string pairs
                Dictionary<string, string> regex = plugin.messageConfig.root.Value<JArray>("global_regex").ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());

                // Run the regex replacements
                foreach (KeyValuePair<string, string> entry in regex)
                {
                    message = message.Replace(entry.Key, entry.Value);
                }
            }
            catch (Exception e)
            {
                plugin.Info("Regex error in " + messagePath);
                plugin.Error(e.ToString());
                throw;
            }

            // Local regex replacements
            try
            {
                // Gets the regex array as a JArray and then converts it to a Dictionary of string pairs
                Dictionary<string, string> regex = eventNode.Value<JArray>("regex").ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());

                // Run the regex replacements
                foreach (KeyValuePair<string, string> entry in regex)
                {
                    message = message.Replace(entry.Key, entry.Value);
                }
            }
            catch (Exception e)
            {
                plugin.Info("Regex error in " + messagePath);
                plugin.Error(e.ToString());
                throw;
            }

            // Add names to the message
            if (variables != null)
            {
                // Variable insertion
                foreach (KeyValuePair<string, string> variable in variables)
                {
                    if (variable.Key == "servername" || variable.Key == "name" || variable.Key == "attackername" || variable.Key == "playername" || variable.Key == "adminname")
                    {
                        message = message.Replace("<var:" + variable.Key + ">", variable.Value);
                    }
                }
                // Final regex replacements
                try
                {
                    // Gets the regex array as a JArray and then converts it to a Dictionary of string pairs
                    Dictionary<string, string> regex = plugin.messageConfig.root.Value<JArray>("final_regex").ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());

                    // Run the regex replacements
                    foreach (KeyValuePair<string, string> entry in regex)
                    {
                        message = message.Replace(entry.Key, entry.Value);
                    }
                }
                catch (Exception e)
                {
                    plugin.Info("Regex error in " + messagePath);
                    plugin.Error(e.ToString());
                    throw;
                }
            }

            // Try to send the message to the bot
            try
            {
                NetworkStream serverStream = plugin.clientSocket.GetStream();
                byte[] outStream = System.Text.Encoding.UTF8.GetBytes(channelID + message + '\0');
                serverStream.Write(outStream, 0, outStream.Length);

                if (plugin.GetConfigBool("discord_verbose"))
                {
                    plugin.Info("Sent message '" + message + "' to discord.");
                }
            }
            catch (InvalidOperationException e)
            {
                plugin.Error("Error sending message '" + message + "' to discord.");
                plugin.Debug(e.ToString());
            }
            catch (ArgumentNullException e)
            {
                plugin.Error("Error sending message '" + message + "' to discord.");
                plugin.Debug(e.ToString());
            }
        }
    }

    class AsyncConnect
    {
        //This is ran once on the first time connecting to the bot
        public AsyncConnect(SCPDiscordPlugin plugin)
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
            plugin.SendParsedMessageAsync("default", "botmessages.connectedtobot");
            plugin.hasConnectedOnce = true;
        }
    }
    class AsyncConnectionWatchdog
    {
        //This is a loop that keeps running and checks if the bot has been disconnected
        public AsyncConnectionWatchdog(SCPDiscordPlugin plugin)
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
                        plugin.SendParsedMessageAsync("default", "botmessages.reconnectedtobot");
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

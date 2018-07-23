using Smod2;
using System;
using System.Net.Sockets;
using System.Threading;

namespace SCPDiscord
{
    class AsyncMessage
    {
        public AsyncMessage(SCPDiscordPlugin plugin, string channelID, string message)
        {
            //Abort if client is dead
            if(plugin.clientSocket == null || !plugin.clientSocket.Connected)
            {
                plugin.Warn("Error sending message '" + message + "' to discord: Not connected to bot.");
                return;
            }

            //Abort on empty message
            if (message == null || message == "" || message == " " || message == ".")
            {
                plugin.Warn("Tried to send empty message to discord.");
                return;
            }

            //Change the default keyword to the bot's representation of it
            if(channelID == "default")
            {
                channelID = "000000000000000000";
            }

            //Try to send the message to the bot
            try
            {
                NetworkStream serverStream = plugin.clientSocket.GetStream();
                byte[] outStream = System.Text.Encoding.UTF8.GetBytes(channelID + message + '\0');
                serverStream.Write(outStream, 0, outStream.Length);

                plugin.Info("Sent message '" + message + "' to discord.");
            }
            catch(InvalidOperationException e)
            {
                plugin.Error("Error sending message '" + message + "' to discord: " + e.ToString());
            }
            catch (ArgumentNullException e)
            {
                plugin.Error("Error sending message '" + message + "' to discord: " + e.ToString());
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
                    plugin.Info("Error occured while connecting to discord bot server.\n" + e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ObjectDisposedException e)
                {
                    plugin.Info("TCP client was unexpectedly closed.\n" + e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    plugin.Info("Invalid port.\n" + e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ArgumentNullException e)
                {
                    plugin.Info("IP address is null.\n" + e.ToString());
                    Thread.Sleep(5000);
                }
            }
            plugin.Info("Connected to Discord bot.");
            plugin.SendMessageAsync("default", "Plugin Connected.");
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
                        plugin.clientSocket.Connect(plugin.GetConfigString("discord_bot_ip"), plugin.GetConfigInt("discord_bot_port"));
                        plugin.Info("Reconnected to Discord bot.");
                        plugin.SendMessageAsync("default", "Plugin Reconnected.");
                    }
                    catch (SocketException e)
                    {
                        plugin.Info("Error occured while reconnecting to discord bot server.\n" + e.ToString());
                        Thread.Sleep(5000);
                    }
                    catch (ObjectDisposedException e)
                    {
                        plugin.Info("TCP client was unexpectedly closed.\n" + e.ToString());
                        Thread.Sleep(5000);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        plugin.Info("Invalid port.\n" + e.ToString());
                        Thread.Sleep(5000);
                    }
                    catch (ArgumentNullException e)
                    {
                        plugin.Info("IP address is null.\n" + e.ToString());
                        Thread.Sleep(5000);
                    }
                }

            }
        }
    }
}

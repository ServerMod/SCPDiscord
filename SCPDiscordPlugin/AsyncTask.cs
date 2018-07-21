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
            if(plugin.clientSocket == null || !plugin.clientSocket.Connected)
            {
                plugin.Warn(plugin.MultiLanguage(44) + " '" + message + "' " + plugin.MultiLanguage(45));
                return;
            }
            if (message == null || message == "" || message == " " || message == ".")
            {
                plugin.Warn(plugin.MultiLanguage(46));
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
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(channelID + message + '\0');
                serverStream.Write(outStream, 0, outStream.Length);

                plugin.Info(plugin.MultiLanguage(47) + " '" + message + "' " + plugin.MultiLanguage(48));
            }
            catch(InvalidOperationException e)
            {
                plugin.Error(plugin.MultiLanguage(49) + " '" + message + "' " + plugin.MultiLanguage(50) + ": " + e.ToString());
            }
            catch (ArgumentNullException e)
            {
                plugin.Error(plugin.MultiLanguage(49) + " '" + message + "' " + plugin.MultiLanguage(50) + ": " + e.ToString());
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
                plugin.Info(plugin.MultiLanguage(51));
                try
                {
                    plugin.Info(plugin.MultiLanguage(52) + ": " + plugin.GetConfigString("discord_bot_ip") + ". " + plugin.MultiLanguage(53) + ": " + plugin.GetConfigInt("discord_bot_port") + ".");
                    plugin.clientSocket.Connect(plugin.GetConfigString("discord_bot_ip"), plugin.GetConfigInt("discord_bot_port"));
                }
                catch (SocketException e)
                {
                    plugin.Info(plugin.MultiLanguage(54) + "\n" + e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ObjectDisposedException e)
                {
                    plugin.Info(plugin.MultiLanguage(55) + "\n" + e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    plugin.Info(plugin.MultiLanguage(56) + "\n" + e.ToString());
                    Thread.Sleep(5000);
                }
                catch (ArgumentNullException e)
                {
                    plugin.Info(plugin.MultiLanguage(57) + "\n" + e.ToString());
                    Thread.Sleep(5000);
                }
            }
            plugin.Info(plugin.MultiLanguage(58));
            plugin.SendMessageAsync("default", plugin.MultiLanguage(59));
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
                    plugin.Info(plugin.MultiLanguage(60));
                    try
                    {
                        plugin.Info(plugin.MultiLanguage(52) + ": " + plugin.GetConfigString("discord_bot_ip") + ". " + plugin.MultiLanguage(53) + ": " + plugin.GetConfigInt("discord_bot_port") + ".");
                        plugin.clientSocket.Connect(plugin.GetConfigString("discord_bot_ip"), plugin.GetConfigInt("discord_bot_port"));
                        plugin.Info(plugin.MultiLanguage(61));
                        plugin.SendMessageAsync("default", plugin.MultiLanguage(62));
                    }
                    catch (SocketException e)
                    {
                        plugin.Info(plugin.MultiLanguage(63) + "\n" + e.ToString());
                        Thread.Sleep(5000);
                    }
                    catch (ObjectDisposedException e)
                    {
                        plugin.Info(plugin.MultiLanguage(55) + "\n" + e.ToString());
                        Thread.Sleep(5000);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        plugin.Info(plugin.MultiLanguage(56) + ".\n" + e.ToString());
                        Thread.Sleep(5000);
                    }
                    catch (ArgumentNullException e)
                    {
                        plugin.Info(plugin.MultiLanguage(57) + "\n" + e.ToString());
                        Thread.Sleep(5000);
                    }
                }

            }
        }
    }
}

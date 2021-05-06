using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SCPDiscord.Interface;
using DSharpPlus.Entities;

namespace SCPDiscord
{
    // Separate class to run the thread
    public class StartNetworkSystem
    {
        public StartNetworkSystem()
        {
            NetworkSystem.Init();
        }
    }

    public static class NetworkSystem // TODO: Make singleton, improve shutdown logic
	{
        private static Socket clientSocket = null;
        private static Socket listenerSocket = null;
        private static NetworkStream networkStream = null;

        private static bool shutdown = false;

        public static void Init()
		{
            shutdown = false;

            if (listenerSocket != null)
            {
                listenerSocket.Shutdown(SocketShutdown.Both);
                listenerSocket.Close();
            }

            if (clientSocket != null)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }

            while (!ConfigParser.loaded)
			{
				Thread.Sleep(1000);
			}

            IPEndPoint listenerEndpoint;
            IPAddress ipAddress;

			if (ConfigParser.config.plugin.address == "0.0.0.0")
			{
				ipAddress = IPAddress.Any;
				listenerEndpoint = new IPEndPoint(ipAddress, ConfigParser.config.plugin.port);

			}
			else if (ConfigParser.config.plugin.address == "::0")
			{
				ipAddress = IPAddress.IPv6Any;
				listenerEndpoint = new IPEndPoint(ipAddress, ConfigParser.config.plugin.port);
			}
			else
			{
				IPHostEntry ipHostInfo = Dns.GetHostEntry(ConfigParser.config.plugin.address);
				ipAddress = ipHostInfo.AddressList[0];
				listenerEndpoint = new IPEndPoint(ipAddress, ConfigParser.config.plugin.port);
			}


			listenerSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listenerSocket.Bind(listenerEndpoint);
			listenerSocket.Listen(10);

			while (!shutdown)
			{
				try
				{
					if (IsConnected())
					{
						Update();
					}
					else
					{
                        DiscordAPI.SetDisconnectedActivity();
						Logger.Log("Listening on " + ConfigParser.config.plugin.address + ":" + ConfigParser.config.plugin.port, LogID.Network);
						clientSocket = listenerSocket.Accept();
						networkStream = new NetworkStream(clientSocket, true);
						Logger.Log("Plugin connected.", LogID.Network);
					}
					Thread.Sleep(1000);
				}
				catch (Exception e)
				{
					Logger.Error("Network error caught, if this happens a lot try using the 'scpd_rc' command." + e, LogID.Network);
				}
			}
		}

        private static async void Update()
		{
            MessageWrapper wrapper;
            try
			{
                wrapper = MessageWrapper.Parser.ParseDelimitedFrom(networkStream);
			}
            catch(Exception e)
			{
                Logger.Error("Couldnt parse incoming package!\n" + e.ToString() , LogID.Network);
                return;
			}

			switch (wrapper.MessageCase)
			{
                case MessageWrapper.MessageOneofCase.BotActivity:
				{
                    DiscordAPI.SetActivity(wrapper.BotActivity.ActivityText, (ActivityType)wrapper.BotActivity.ActivityType, (UserStatus)wrapper.BotActivity.StatusType);
                    Logger.Debug("Parsed: " + wrapper.ToString(), LogID.Network);
                    break;
				}
                case MessageWrapper.MessageOneofCase.ChannelTopic:
				{
                    DiscordAPI.SetChannelTopic(wrapper.ChannelTopic.ChannelID, wrapper.ChannelTopic.TopicText);
                    Logger.Debug("Parsed: " + wrapper.ToString(), LogID.Network);
                    break;
				}
                case MessageWrapper.MessageOneofCase.ChatMessage:
                {
                    await DiscordAPI.SendMessage(wrapper.ChatMessage.ChannelID, wrapper.ChatMessage.Content);
                    Logger.Debug("Parsed: " + wrapper.ToString(), LogID.Network);
                    break;
                }
                default:
				{
                    Logger.Warn("Unknown package received: " + wrapper.ToString(), LogID.Network);
                    break;
				}
			}
        }

        public static void ShutDown()
		{
            shutdown = true;
		}

        public static bool IsConnected()
        {
            if (clientSocket == null)
            {
                return false;
            }

            try
            {
                return !((clientSocket.Poll(1000, SelectMode.SelectRead) && (clientSocket.Available == 0)) || !clientSocket.Connected);
            }
            catch (ObjectDisposedException e)
            {
                Logger.Error("TCP client was unexpectedly closed.", LogID.Network);
                Logger.Debug(e.ToString(), LogID.Network);
                return false;
            }
        }
    }
}

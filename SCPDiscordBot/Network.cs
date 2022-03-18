using DSharpPlus.Entities;
using Google.Protobuf;
using SCPDiscord.Interface;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

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
			}
			else if (ConfigParser.config.plugin.address == "::0")
			{
				ipAddress = IPAddress.IPv6Any;
			}
			else if (IPAddress.TryParse(ConfigParser.config.plugin.address, out IPAddress parsedIP))
			{
				ipAddress = parsedIP;
			}
			else
			{
				IPHostEntry ipHostInfo = Dns.GetHostEntry(ConfigParser.config.plugin.address);

				// Use an IPv4 address if available
				if(ipHostInfo.AddressList.Any(ip => ip.AddressFamily == AddressFamily.InterNetwork))
				{
					ipAddress = ipHostInfo.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
				}
				else
				{
					ipAddress = ipHostInfo.AddressList[0];
				}
			}

			listenerEndpoint = new IPEndPoint(ipAddress, ConfigParser.config.plugin.port);
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
						Logger.Log("Listening on " + ipAddress.ToString() + ":" + ConfigParser.config.plugin.port, LogID.Network);
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
			catch (Exception)
			{
				Logger.Error("Couldn't parse incoming packet!", LogID.Network);
				return;
			}

			Logger.Debug("Incoming packet: " + JsonFormatter.Default.Format(wrapper), LogID.Network);

			switch (wrapper.MessageCase)
			{
				case MessageWrapper.MessageOneofCase.BotActivity:
					DiscordAPI.SetActivity(wrapper.BotActivity.ActivityText, (ActivityType)wrapper.BotActivity.ActivityType, (UserStatus)wrapper.BotActivity.StatusType);
					break;
				case MessageWrapper.MessageOneofCase.ChatMessage:
					await DiscordAPI.SendMessage(wrapper.ChatMessage.ChannelID, wrapper.ChatMessage.Content);
					break;
				case MessageWrapper.MessageOneofCase.RoleQuery:
					DiscordAPI.GetPlayerRoles(wrapper.RoleQuery.DiscordID, wrapper.RoleQuery.SteamID);
					break;
				case MessageWrapper.MessageOneofCase.SyncRoleCommand:
				case MessageWrapper.MessageOneofCase.UnsyncRoleCommand:
				case MessageWrapper.MessageOneofCase.ConsoleCommand:
				case MessageWrapper.MessageOneofCase.RoleResponse:
					Logger.Warn("Recieved packet meant for plugin: " + JsonFormatter.Default.Format(wrapper), LogID.Network);
					break;
				default:
					Logger.Warn("Unknown packet received: " + JsonFormatter.Default.Format(wrapper), LogID.Network);
					break;
			}
		}

		public static void SendMessage(MessageWrapper message)
		{
			message.WriteDelimitedTo(networkStream);
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

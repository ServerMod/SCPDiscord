using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace SCPDiscord
{
	public enum LogID
	{
		General,
		Config,
		Network,
		Command,
		Discord
	};

	public static class Logger
	{
		private static Dictionary<LogID, EventId> eventIDs = new Dictionary<LogID, EventId>
		{
			{ LogID.General, new EventId(500, "General") },
			{ LogID.Config,  new EventId(501, "Config")  },
			{ LogID.Network, new EventId(502, "Network") },
			{ LogID.Command, new EventId(503, "Command") },
			{ LogID.Discord, new EventId(504, "Discord") },
		};

		public static void Debug(string Message, LogID logID)
		{
			try
			{
				DiscordAPI.GetClient().Logger.Log(LogLevel.Debug, eventIDs[logID], Message);
			}
			catch(NullReferenceException)
			{
				Console.WriteLine("[DEBUG] " + Message);
			}
		}

		public static void Log(string Message, LogID logID)
		{
			try
			{
				DiscordAPI.GetClient().Logger.Log(LogLevel.Information, eventIDs[logID], Message);
			}
			catch (NullReferenceException)
			{
				Console.WriteLine("[INFO] " + Message);
			}
		}

		public static void Warn(string Message, LogID logID)
		{
			try
			{
				DiscordAPI.GetClient().Logger.Log(LogLevel.Warning, eventIDs[logID], Message);
			}
			catch (NullReferenceException)
			{
				Console.WriteLine("[WARNING] " + Message);
			}
		}

		public static void Error(string Message, LogID logID)
		{
			try
			{
				DiscordAPI.GetClient().Logger.Log(LogLevel.Error, eventIDs[logID], Message);
			}
			catch (NullReferenceException)
			{
				Console.WriteLine("[ERROR] " + Message);
			}
		}

		public static void Fatal(string Message, LogID logID)
		{
			try
			{
				DiscordAPI.GetClient().Logger.Log(LogLevel.Critical, eventIDs[logID], Message);
			}
			catch (NullReferenceException)
			{
				Console.WriteLine("[CRITICAL] " + Message);
			}
		}
	}
}

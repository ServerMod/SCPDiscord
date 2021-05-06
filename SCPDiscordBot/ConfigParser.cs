using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using SCPDiscordBot.Properties;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SCPDiscord
{
	public class Config
	{
		public class Bot
		{
			public string token = "";
			public ulong serverId = 0;
			public string prefix = "";
			public List<ulong> statusChannels = new List<ulong>();
			public List<ulong> commandChannels = new List<ulong>();
			public string logLevel = "Information";
			public string presenceType = "Watching";
			public string presenceText = "for server startup...";
			public int messageCooldown = 1000;
			public int messageDelay = 0;
		}
		public Bot bot;

		public class Plugin
		{
			public string address = "127.0.0.1";
			public int port = 8888;
		}
		public Plugin plugin;

		public Dictionary<ulong, string[]> permissions;
	}

	public static class ConfigParser
	{
		public static bool loaded = false;

		public static Config config = null;

		public static void LoadConfig()
		{
			// Writes default config to file if it does not already exist
			if (!File.Exists("./config.yml"))
			{
				File.WriteAllText("./config.yml", Encoding.UTF8.GetString(Resources.default_config));
			}

			// Reads config contents into FileStream
			FileStream stream = File.OpenRead("./config.yml");

			// Converts the FileStream into a YAML object
			IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build();
			config = deserializer.Deserialize<Config>(new StreamReader(stream));

			loaded = true;
		}

		public static void PrintConfig()
		{
			Logger.Debug("#### Config settings ####", LogID.Config);
			// Token skipped
			Logger.Debug("bot.server-id: " + config.bot.serverId, LogID.Config);
			Logger.Debug("bot.prefix: " + config.bot.prefix, LogID.Config);
			Logger.Debug("bot.status-channels: " + string.Join(", ", config.bot.statusChannels), LogID.Config);
			Logger.Debug("bot.command-channels: " + string.Join(", ", config.bot.commandChannels), LogID.Config);
			Logger.Debug("bot.log-level: " + config.bot.logLevel, LogID.Config);
			Logger.Debug("bot.presence-type: " + config.bot.presenceType, LogID.Config);
			Logger.Debug("bot.presence-text: " + config.bot.presenceText, LogID.Config);
			Logger.Debug("bot.message-cooldown: " + config.bot.messageCooldown, LogID.Config);
			Logger.Debug("bot.message-delay: " + config.bot.messageDelay, LogID.Config);

			Logger.Debug("plugin.address: " + config.plugin.address, LogID.Config);
			Logger.Debug("plugin.port: " + config.plugin.port, LogID.Config);
		}

		/// <summary>
		/// Checks whether a user has a specific permission.
		/// </summary>
		/// <param name="member">The Discord user to check.</param>
		/// <param name="permission">The permission name to check.</param>
		/// <returns></returns>
		public static bool HasPermission(DiscordMember member, string permission)
		{
			return false; //member.Roles.Any(role => permissions.ContainsKey(role.Id) && permissions[role.Id].Any(id => id.StartsWith(permission)));
		}
	}
}

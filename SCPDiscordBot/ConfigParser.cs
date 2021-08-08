using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SCPDiscordBot.Properties;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

			Logger.Debug("plugin.address: " + config.plugin.address, LogID.Config);
			Logger.Debug("plugin.port: " + config.plugin.port, LogID.Config);

			Logger.Debug("permissions:", LogID.Config);
			foreach (KeyValuePair<ulong, string[]> role in config.permissions)
			{
				Logger.Debug("  " + role.Key + ":", LogID.Config);
				foreach (string permission in role.Value)
				{
					Logger.Debug("    " + permission, LogID.Config);
				}
			}
		}

		public static bool ValidatePermission(CommandContext command)
		{
			if (HasPermission(command.Member, command.Message.Content.Substring(config.bot.prefix.Length))) return true;

			DiscordEmbed deniedMessage = new DiscordEmbedBuilder
			{
				Color = DiscordColor.Red,
				Description = "You do not have permission to do that!"
			};
			Task.Run(async () => await command.RespondAsync(deniedMessage));
			Logger.Log(command.Member.Username + "#" + command.Member.Discriminator + " tried to use '" + command.Message.Content + "' but did not have permission.", LogID.Command);
			return false;
		}

		public static bool HasPermission(DiscordMember member, string permission)
		{
			// If a specific role is allowed to use the command
			if (member.Roles.Any(role => config.permissions.ContainsKey(role.Id) && config.permissions[role.Id].Any(node => Regex.IsMatch(permission, "^" + node, RegexOptions.IgnoreCase | RegexOptions.Singleline))))
			{
				return true;
			}

			// If everyone is allowed to use the command
			if (config.permissions.ContainsKey(0) && config.permissions[0].Any(node => Regex.IsMatch(permission, node, RegexOptions.IgnoreCase | RegexOptions.Singleline)))
			{
				return true;
			}

			return false;
		}

		public static bool IsCommandChannel(ulong channelID)
		{
			return config.bot.commandChannels.Any(id => id == channelID);
		}

		public static bool IsStatusChannel(ulong channelID)
		{
			return config.bot.statusChannels.Any(id => id == channelID);
		}
	}
}

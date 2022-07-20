using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCPDiscord
{
	public class DiscordAPI
	{
		public static DiscordAPI instance = null;
		public bool connected = false;
		public static DiscordClient client = new DiscordClient(new DiscordConfiguration { Token = "DUMMY_TOKEN", TokenType = TokenType.Bot, MinimumLogLevel = LogLevel.Debug });
		private CommandsNextExtension commands = null;

		public static async Task Reset()
		{
			try
			{
				Logger.Log("Setting up Discord client...", LogID.DISCORD);

				instance = new DiscordAPI();

				// Check if token is unset
				if (ConfigParser.config.bot.token == "add-your-token-here" || ConfigParser.config.bot.token == "")
				{
					Logger.Fatal("You need to set your bot token in the config and start the bot again.", LogID.CONFIG);
					throw new ArgumentException("Invalid Discord bot token");
				}

				// Checking log level
				if (!Enum.TryParse(ConfigParser.config.bot.logLevel, true, out LogLevel logLevel))
				{
					Logger.Warn("Log level '" + ConfigParser.config.bot.logLevel + "' invalid, using 'Information' instead.", LogID.CONFIG);
					logLevel = LogLevel.Information;
				}

				// Setting up client configuration
				DiscordConfiguration cfg = new DiscordConfiguration
				{
					Token = ConfigParser.config.bot.token,
					TokenType = TokenType.Bot,
					MinimumLogLevel = logLevel,
					AutoReconnect = true,
					Intents = DiscordIntents.AllUnprivileged,
					LogTimestampFormat = "yyyy-MM-dd HH:mm:ss"
				};

				client = new DiscordClient(cfg);

				Logger.Log("Registering commands...", LogID.DISCORD);
				instance.commands = client.UseCommandsNext(new CommandsNextConfiguration
				{
					StringPrefixes = new[] { ConfigParser.config.bot.prefix }
				});

				instance.commands.RegisterCommands<Commands.SyncSteamIDCommand>();
				instance.commands.RegisterCommands<Commands.SyncIPCommand>();
				instance.commands.RegisterCommands<Commands.UnsyncRoleCommand>();
				instance.commands.RegisterCommands<Commands.ServerCommand>();
				instance.commands.RegisterCommands<Commands.ListCommand>();
				instance.commands.RegisterCommands<Commands.KickAllCommand>();
				instance.commands.RegisterCommands<Commands.KickCommand>();
				instance.commands.RegisterCommands<Commands.BanCommand>();
				instance.commands.RegisterCommands<Commands.UnbanCommand>();

				Logger.Log("Hooking events...", LogID.DISCORD);
				client.Ready += instance.OnReady;
				client.GuildAvailable += instance.OnGuildAvailable;
				client.ClientErrored += instance.OnClientError;
				client.SocketErrored += instance.OnSocketError;

				Logger.Log("Hooking command events...", LogID.DISCORD);
				instance.commands.CommandErrored += instance.OnCommandError;

				ConfigParser.PrintConfig();

				Logger.Log("Connecting to Discord...", LogID.DISCORD);
				await client.ConnectAsync();
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString(), LogID.DISCORD);
			}
		}

		public static void SetDisconnectedActivity()
		{
			// Checking activity type
			if (!Enum.TryParse(ConfigParser.config.bot.presenceType, true, out ActivityType activityType))
			{
				Logger.Warn("Presence type '" + ConfigParser.config.bot.presenceType + "' invalid, using 'Playing' instead.", LogID.DISCORD);
				activityType = ActivityType.Playing;
			}

			SetActivity(ConfigParser.config.bot.presenceText, activityType, UserStatus.DoNotDisturb);
		}

		public static void SetActivity(string activityText, ActivityType activityType, UserStatus status)
		{
			if (instance.connected)
				client.UpdateStatusAsync(new DiscordActivity(activityText, activityType), status);
		}

		public static async Task SendMessage(ulong channelID, string message)
		{
			if (!instance.connected) return;

			try
			{
				DiscordChannel channel = await client.GetChannelAsync(channelID);
				try
				{
					foreach (string content in SplitString(message, 2000))
					{
						await channel.SendMessageAsync(content);
					}
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to send message in '" + channel.Name + "'", LogID.DISCORD);
				}
			}
			catch (Exception)
			{
				Logger.Error("Could not send message in text channel with the ID '" + channelID + "'", LogID.DISCORD);
			}
		}

		public static async Task SendMessage(ulong channelID, DiscordEmbed message)
		{
			if (!instance.connected) return;

			try
			{
				DiscordChannel channel = await client.GetChannelAsync(channelID);
				try
				{
					await channel.SendMessageAsync(message);
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to send message in '" + channel.Name + "'", LogID.DISCORD);
				}
			}
			catch (Exception)
			{
				Logger.Error("Could not send message in text channel with the ID '" + channelID + "'", LogID.DISCORD);
			}
		}

		private static IEnumerable<string> SplitString(string str, int size)
		{
			for (int i = 0; i < str.Length; i += size)
			{
				yield return str.Substring(i, Math.Min(size, str.Length - i));
			}
		}

		public static async void GetPlayerRoles(ulong userID, string steamID)
		{
			if (!instance.connected) return;

			if (ConfigParser.config.bot.serverId == 0)
			{
				Logger.Warn("Plugin attempted to use role sync, but no server ID was set in the config. Ignoring request...", LogID.DISCORD);
				return;
			}

			try
			{
				DiscordGuild guild = await client.GetGuildAsync(ConfigParser.config.bot.serverId);
				DiscordMember member = await guild.GetMemberAsync(userID);

				Interface.MessageWrapper message = new Interface.MessageWrapper
				{
					RoleResponse = new Interface.RoleResponse
					{
						DiscordID = userID,
						SteamIDOrIP = steamID
					}
				};
				message.RoleResponse.RoleIDs.AddRange(member.Roles.Select(role => role.Id));
				NetworkSystem.SendMessage(message);
			}
			catch (Exception)
			{
				Logger.Warn("Couldn't find discord server or server member for role syncing requested by plugin. Discord ID: " + userID + " SteamID/IP: " + steamID, LogID.DISCORD);
			}
		}

		public async Task OnReady(DiscordClient client, ReadyEventArgs e)
		{
			instance.connected = true;
			Logger.Log("Connected to Discord.", LogID.DISCORD);
			SetDisconnectedActivity();

			foreach (ulong channelID in ConfigParser.config.bot.statusChannels)
			{
				DiscordEmbed message = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Green,
					Description = "**Bot online**"
				};
				await SendMessage(channelID, message);
			}
		}

		public Task OnSocketError(DiscordClient client, SocketErrorEventArgs e)
		{
			Logger.Debug("Discord socket error: " + e.Exception, LogID.DISCORD);
			return Task.CompletedTask;
		}

		public Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs e)
		{
			Logger.Log("Found Discord server: " + e.Guild.Name, LogID.DISCORD);

			IReadOnlyDictionary<ulong, DiscordRole> roles = e.Guild.Roles;

			foreach ((ulong roleID, DiscordRole role) in roles)
			{
				Logger.Debug(role.Name.PadRight(40, '.') + roleID, LogID.DISCORD);
			}
			return Task.CompletedTask;
		}

		public Task OnClientError(DiscordClient client, ClientErrorEventArgs e)
		{
			Logger.Error($"Exception occured: {e.Exception.GetType()}: {e.Exception}", LogID.DISCORD);

			return Task.CompletedTask;
		}

		public Task OnCommandError(CommandsNextExtension commandSystem, CommandErrorEventArgs e)
		{
			switch (e.Exception)
			{
				case CommandNotFoundException:
					return Task.CompletedTask;

				case ArgumentException:
				{
					if (!ConfigParser.IsCommandChannel(e.Context.Channel.Id)) return Task.CompletedTask;
					DiscordEmbed error = new DiscordEmbedBuilder
					{
						Color = DiscordColor.Red,
						Description = "Invalid arguments."
					};
					e.Context?.Channel?.SendMessageAsync(error);
					return Task.CompletedTask;
				}

				case ChecksFailedException ex:
				{
					if (!ConfigParser.IsCommandChannel(e.Context.Channel.Id)) return Task.CompletedTask;
					foreach (CheckBaseAttribute attr in ex.FailedChecks)
					{
						DiscordEmbed error = new DiscordEmbedBuilder
						{
							Color = DiscordColor.Red,
							Description = this.ParseFailedCheck(attr)
						};
						e.Context?.Channel?.SendMessageAsync(error);
					}
					return Task.CompletedTask;
				}

				default:
				{
					Logger.Error("Exception occured: " + e.Exception, LogID.DISCORD);
					if (!ConfigParser.IsCommandChannel(e.Context.Channel.Id)) return Task.CompletedTask;
					DiscordEmbed error = new DiscordEmbedBuilder
					{
						Color = DiscordColor.Red,
						Description = "Internal error occured, please report this to the developer."
					};
					e.Context?.Channel?.SendMessageAsync(error);
					return Task.CompletedTask;
				}
			}
		}

		private string ParseFailedCheck(CheckBaseAttribute attr)
		{
			return attr switch
			{
				CooldownAttribute _ => "You cannot use do that so often!",
				RequireOwnerAttribute _ => "Only the server owner can use that command!",
				RequirePermissionsAttribute _ => "You don't have permission to do that!",
				RequireRolesAttribute _ => "You do not have a required role!",
				RequireUserPermissionsAttribute _ => "You don't have permission to do that!",
				RequireNsfwAttribute _ => "This command can only be used in an NSFW channel!",
				_ => "Unknown Discord API error occured, please try again later.",
			};
		}
	}
}

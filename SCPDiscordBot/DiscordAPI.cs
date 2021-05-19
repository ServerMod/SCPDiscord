using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using DSharpPlus.Exceptions;

namespace SCPDiscord
{
	public class DiscordAPI
	{
		public static DiscordAPI instance = null;
		public bool connected = false;
		public DiscordClient discordClient = null;
		private CommandsNextExtension commands = null;

		public async static Task Reset()
		{
			try
			{
				Logger.Log("Setting up Discord client...", LogID.Discord);

				instance = new DiscordAPI();

				// Check if token is unset
				if (ConfigParser.config.bot.token == "<add-token-here>" || ConfigParser.config.bot.token == "")
				{
					Logger.Fatal("You need to set your bot token in the config and start the bot again.", LogID.Config);
					throw new ArgumentException("Invalid Discord bot token");
				}

				// Checking log level
				if (!Enum.TryParse(ConfigParser.config.bot.logLevel, true, out LogLevel logLevel))
				{
					Logger.Warn("Log level '" + ConfigParser.config.bot.logLevel + "' invalid, using 'Information' instead.", LogID.Config);
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

				instance.discordClient = new DiscordClient(cfg);

				Logger.Log("Registering commands...", LogID.Discord);
				instance.commands = instance.discordClient.UseCommandsNext(new CommandsNextConfiguration
				{
					StringPrefixes = new[] { ConfigParser.config.bot.prefix }
				});

				// Handle hidetag/showtag differently but prepending the command with "console_"
				instance.commands.RegisterCommands<Commands.SyncRoleCommand>();
				instance.commands.RegisterCommands<Commands.UnsyncRoleCommand>();
				instance.commands.RegisterCommands<Commands.ServerCommand>();
				instance.commands.RegisterCommands<Commands.ListCommand>();

				Logger.Log("Hooking events...", LogID.Discord);
				instance.discordClient.Ready += instance.OnReady;
				instance.discordClient.GuildAvailable += instance.OnGuildAvailable;
				instance.discordClient.ClientErrored += instance.OnClientError;
				instance.discordClient.SocketClosed += instance.OnSocketClosed;
				instance.discordClient.SocketErrored += instance.OnSocketError;

				Logger.Log("Hooking command events...", LogID.Discord);
				instance.commands.CommandErrored += instance.OnCommandError;

				ConfigParser.PrintConfig();

				Logger.Log("Connecting to Discord...", LogID.Discord);
				await instance.discordClient.ConnectAsync();
			}
			catch(Exception e)
			{
				Logger.Error(e.ToString(), LogID.Discord);
			}
		}

		public static DiscordClient GetClient()
		{
			return instance.discordClient;
		}

		public static void SetDisconnectedActivity()
		{
			// Checking activity type
			if (!Enum.TryParse(ConfigParser.config.bot.presenceType, true, out ActivityType activityType))
			{
				Logger.Warn("Presence type '" + ConfigParser.config.bot.presenceType + "' invalid, using 'Playing' instead.", LogID.Discord);
				activityType = ActivityType.Playing;
			}

			SetActivity(ConfigParser.config.bot.presenceText, activityType, UserStatus.DoNotDisturb);
		}

		public static void SetActivity(string activityText, ActivityType activityType, UserStatus status)
		{
			if(instance.connected)
				GetClient()?.UpdateStatusAsync(new DiscordActivity(activityText, activityType), status);
		}

		public static async void SetChannelTopic(ulong channelID, string channelTopic)
		{
			if (!instance.connected) return;

			try
			{
				DiscordChannel channel = await GetClient().GetChannelAsync(channelID);
				try
				{
					await channel.ModifyAsync(modification => modification.Topic = channelTopic);
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to set channel topic on '" + channel.Name + "'", LogID.Discord);
				}
			}
			catch (Exception)
			{
				Logger.Error("Could not find text channel with the ID '" + channelID + "'", LogID.Discord);
			}
		}

		public static async Task SendMessage(ulong channelID, string message)
		{
			if (!instance.connected) return;

			try
			{
				DiscordChannel channel = await GetClient().GetChannelAsync(channelID);
				try
				{
					foreach(string content in SplitString(message, 2000))
					{
						await channel.SendMessageAsync(content);
					}
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to send message in '" + channel.Name + "'", LogID.Discord);
				}
			}
			catch (Exception)
			{
				Logger.Error("Could not send message in text channel with the ID '" + channelID + "'", LogID.Discord);
			}
		}

		public static async Task SendMessage(ulong channelID, DiscordEmbed message)
		{
			if (!instance.connected) return;

			try
			{
				DiscordChannel channel = await GetClient().GetChannelAsync(channelID);
				try
				{
					await channel.SendMessageAsync(message);
				}
				catch (UnauthorizedException)
				{
					Logger.Error("No permissions to send message in '" + channel.Name + "'", LogID.Discord);
				}
			}
			catch (Exception)
			{
				Logger.Error("Could not send message in text channel with the ID '" + channelID + "'", LogID.Discord);
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
			if(ConfigParser.config.bot.serverId == 0)
			{
				Logger.Warn("Plugin attempted to use role sync, but no server ID was set in the config. Ignoring request...", LogID.Discord);
				return;
			}

			try
			{
				DiscordGuild guild = await GetClient().GetGuildAsync(ConfigParser.config.bot.serverId);
				DiscordMember member = await guild.GetMemberAsync(userID);

				Interface.MessageWrapper message = new Interface.MessageWrapper
				{
					RoleResponse = new Interface.RoleResponse
					{
						DiscordID = userID,
						SteamID = steamID
					}
				};
				message.RoleResponse.RoleIDs.AddRange(member.Roles.Select(role => role.Id));
				NetworkSystem.SendMessage(message);
			}
			catch(Exception)
			{
				Logger.Warn("Couldn't find discord server or server member for role syncing requested by plugin. Discord ID: " + userID.ToString() + " SteamID: " + steamID, LogID.Discord);
			}
		}

		public async Task OnReady(DiscordClient client, ReadyEventArgs e)
		{
			instance.connected = true;
			Logger.Log("Connected to Discord.", LogID.Discord);
			SetDisconnectedActivity();

			foreach(ulong channelID in ConfigParser.config.bot.statusChannels)
			{
				DiscordEmbed message = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Green,
					Description = "**Bot online**"
				};
				await SendMessage(channelID, message);
			}
		}

		public Task OnSocketClosed(DiscordClient client, SocketCloseEventArgs e)
		{
			Reset();
			return Task.CompletedTask;
		}

		public Task OnSocketError(DiscordClient client, SocketErrorEventArgs e)
		{
			Logger.Error("Discord client error: " + e.Exception.ToString(), LogID.Discord);
			return Task.CompletedTask;
		}

		public Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs e)
		{
			Logger.Log("Found Discord server: " + e.Guild.Name, LogID.Discord);

			IReadOnlyDictionary<ulong, DiscordRole> roles = e.Guild.Roles;

			foreach ((ulong roleID, DiscordRole role) in roles)
			{
				Logger.Debug(role.Name.PadRight(40, '.') + roleID, LogID.Discord);
			}
			return Task.CompletedTask;
		}

		public Task OnClientError(DiscordClient client, ClientErrorEventArgs e)
		{
			Logger.Error($"Exception occured: {e.Exception.GetType()}: {e.Exception}", LogID.Discord);

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
					DiscordEmbed error = new DiscordEmbedBuilder
					{
						Color = DiscordColor.Red,
						Description = "Invalid arguments."
					};
					e.Context?.Channel?.SendMessageAsync(error);
					return Task.CompletedTask;
				}

				case ChecksFailedException:
				{
					foreach (CheckBaseAttribute attr in ((ChecksFailedException)e.Exception).FailedChecks)
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
					Logger.Error($"Exception occured: {e.Exception.GetType()}: {e.Exception}", LogID.Discord);
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
				CooldownAttribute => "You cannot use do that so often!",
				RequireOwnerAttribute => "Only the server owner can use that command!",
				RequirePermissionsAttribute => "You don't have permission to do that!",
				RequireRolesAttribute => "You do not have a required role!",
				RequireUserPermissionsAttribute => "You don't have permission to do that!",
				RequireNsfwAttribute => "This command can only be used in an NSFW channel!",
				_ => "Unknown Discord API error occured, please try again later.",
			};
		}
	}
}

using SCPDiscord.Interface;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SCPDiscord
{
	class BotListener
	{
		private readonly SCPDiscord plugin;
		public BotListener(SCPDiscord plugin)
		{
			this.plugin = plugin;
			while (true)
			{
				try
				{
					//Listen for connections
					if (NetworkSystem.IsConnected())
					{
						MessageWrapper data;
						try
						{
							data = MessageWrapper.Parser.ParseDelimitedFrom(NetworkSystem.networkStream);
						}
						catch (Exception e)
						{
							if (e is IOException)
								plugin.Error("Connection to bot lost.");

							else
								plugin.Error("Couldn't parse incoming packet!\n" + e.ToString());

							return;
						}

						plugin.Debug("Incoming packet: " + Google.Protobuf.JsonFormatter.Default.Format(data));

						switch (data.MessageCase)
						{
							case MessageWrapper.MessageOneofCase.SyncRoleCommand:
								plugin.SendStringByID(data.SyncRoleCommand.ChannelID, plugin.roleSync.AddPlayer(data.SyncRoleCommand.SteamID.ToString(), data.SyncRoleCommand.DiscordID));
								break;

							case MessageWrapper.MessageOneofCase.UnsyncRoleCommand:
								plugin.SendStringByID(data.UnsyncRoleCommand.ChannelID, plugin.roleSync.RemovePlayer(data.UnsyncRoleCommand.DiscordID));
								break;

							case MessageWrapper.MessageOneofCase.ConsoleCommand:
								plugin.sync.ScheduleDiscordCommand(data.ConsoleCommand);
								break;

							case MessageWrapper.MessageOneofCase.RoleResponse:
								plugin.roleSync.ReceiveQueryResponse(data.RoleResponse.SteamID, data.RoleResponse.RoleIDs.ToList());
								break;

							case MessageWrapper.MessageOneofCase.BanCommand:
								BanCommand(data.BanCommand.ChannelID, data.BanCommand.SteamID.ToString(), data.BanCommand.Duration, data.BanCommand.Reason, data.BanCommand.AdminTag);
								break;

							case MessageWrapper.MessageOneofCase.UnbanCommand:
								UnbanCommand(data.UnbanCommand.ChannelID, data.UnbanCommand.SteamIDOrIP);
								break;

							case MessageWrapper.MessageOneofCase.KickCommand:
								KickCommand(data.KickCommand.ChannelID, data.KickCommand.SteamID.ToString(), data.KickCommand.Reason, data.KickCommand.AdminTag);
								break;

							case MessageWrapper.MessageOneofCase.KickallCommand:
								KickallCommand(data.KickallCommand.ChannelID, data.KickallCommand.Reason, data.KickallCommand.AdminTag);
								break;

							case MessageWrapper.MessageOneofCase.ListCommand:
								var reply = "```md\n# Players online (" + (plugin.Server.NumPlayers - 1) + "):\n";
								foreach (Player player in plugin.Server.GetPlayers())
								{
									reply += player.Name.PadRight(35) + "<" + player.UserId + ">" + "\n";
								}
								reply += "```";
								plugin.SendStringByID(data.ListCommand.ChannelID, reply);
								break;

							case MessageWrapper.MessageOneofCase.BotActivity:
							case MessageWrapper.MessageOneofCase.ChatMessage:
							case MessageWrapper.MessageOneofCase.RoleQuery:
								plugin.Warn("Recieved packet meant for bot: " + Google.Protobuf.JsonFormatter.Default.Format(data));
								break;

							default:
								plugin.Warn("Unknown packet received: " + Google.Protobuf.JsonFormatter.Default.Format(data));
								break;
						}
					}
					Thread.Sleep(1000);
				}
				catch (Exception ex)
				{
					plugin.Error("BotListener Error: " + ex);
				}
			}
		}

		/// <summary>
		/// Handles a ban command from Discord.
		/// </summary>
		/// <param name="channelID">ChannelID of the channel the command was used in.</param>
		/// <param name="steamID">SteamID of player to be banned.</param>
		/// <param name="duration">Duration of ban expressed as xu where x is a number and u is a character representing a unit of time.</param>
		/// <param name="reason">Optional reason for the ban.</param>
		private void BanCommand(ulong channelID, string steamID, string duration, string reason, string adminTag)
		{
			// Perform very basic SteamID validation.
			if (!IsPossibleSteamID(steamID))
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "steamid", steamID }
				};
				plugin.SendMessageByID(channelID, "botresponses.invalidsteamid", variables);
				return;
			}

			// Create duration timestamp.
			string humanReadableDuration = "";
			DateTime endTime;
			try
			{
				endTime = ParseBanDuration(duration, ref humanReadableDuration);
			}
			catch (IndexOutOfRangeException)
			{
				endTime = DateTime.MinValue;
			}

			if (endTime == DateTime.MinValue)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "duration", duration }
				};
				plugin.SendMessageByID(channelID, "botresponses.invalidduration", variables);
				return;
			}

			string name = "";
			if (!plugin.GetPlayerName(steamID, ref name))
			{
				name = "Offline player";
			}

			//Semicolons are separators in the ban file so cannot be part of strings
			name = name.Replace(";", "");
			reason = reason.Replace(";", "");

			if (reason == "")
			{
				reason = "No reason provided.";
			}

			// Add the player to the SteamIDBans file.
			StreamWriter streamWriter = new StreamWriter(FileManager.GetAppFolder(true, true) + "UserIdBans.txt", true);
			streamWriter.WriteLine(name + ';' + steamID + ';' + endTime.Ticks + ';' + reason + ";" + adminTag + ";" + DateTime.UtcNow.Ticks);
			streamWriter.Dispose();

			// Kicks the player if they are online.
			plugin.KickPlayer(steamID, "Banned for the following reason: '" + reason + "'");

			Dictionary<string, string> banVars = new Dictionary<string, string>
			{
				{ "name",       name                    },
				{ "steamid",    steamID                 },
				{ "reason",     reason                  },
				{ "duration",   humanReadableDuration   },
				{ "admintag",   adminTag                }
			};
			plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.playerbanned", banVars);
		}

		/// <summary>
		/// Handles an unban command from Discord.
		/// </summary>
		/// <param name="channelID">ChannelID of discord channel command was used in.</param>
		/// <param name="steamIDOrIP">SteamID of player to be unbanned.</param>
		private void UnbanCommand(ulong channelID, string steamIDOrIP)
		{
			// Perform very basic SteamID and ip validation.
			if (!IsPossibleSteamID(steamIDOrIP) && !IPAddress.TryParse(steamIDOrIP, out IPAddress _))
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "steamidorip", steamIDOrIP }
				};
				plugin.SendMessageByID(channelID, "botresponses.invalidsteamidorip", variables);
				return;
			}

			// Get all ban entries in the files.
			List<string> ipBans = File.ReadAllLines(FileManager.GetAppFolder(true, true) + "IpBans.txt").ToList();
			List<string> steamIDBans = File.ReadAllLines(FileManager.GetAppFolder(true, true) + "UserIdBans.txt").ToList();

			// Get all ban entries to be removed.
			List<string> matchingIPBans = ipBans.FindAll(s => s.Contains(steamIDOrIP));
			List<string> matchingSteamIDBans = steamIDBans.FindAll(s => s.Contains(steamIDOrIP));

			// Delete the entries from the original containers now that there is a backup of them
			ipBans.RemoveAll(s => s.Contains(steamIDOrIP));
			steamIDBans.RemoveAll(s => s.Contains(steamIDOrIP));

			// Check if either ban file has a ban with a time stamp matching the one removed and remove it too as most servers create both a steamid-ban entry and an ip-ban entry.
			foreach (var row in matchingIPBans)
			{
				steamIDBans.RemoveAll(s => s.Contains(row.Split(';').Last()));
			}

			foreach (var row in matchingSteamIDBans)
			{
				ipBans.RemoveAll(s => s.Contains(row.Split(';').Last()));
			}

			// Save the edited ban files
			File.WriteAllLines(FileManager.GetAppFolder(true, true) + "IpBans.txt", ipBans);
			File.WriteAllLines(FileManager.GetAppFolder(true, true) + "UserIdBans.txt", steamIDBans);

			// Send response message to Discord
			Dictionary<string, string> unbanVars = new Dictionary<string, string>
			{
				{ "steamidorip", steamIDOrIP }
			};
			plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.playerunbanned", unbanVars);
		}

		/// <summary>
		/// Handles the kick command.
		/// </summary>
		/// <param name="channelID">Channel ID for response message.</param>
		/// <param name="steamID">SteamID of player to be kicked.</param>
		/// <param name="reason">The kick reason.</param>
		private void KickCommand(ulong channelID, string steamID, string reason, string adminTag)
		{
			//Perform very basic SteamID validation
			if (!IsPossibleSteamID(steamID))
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "steamid", steamID }
				};
				plugin.SendMessageByID(channelID, "botresponses.invalidsteamid", variables);
				return;
			}

			//Get player name for feedback message
			string playerName = "";
			plugin.GetPlayerName(steamID, ref playerName);

			//Kicks the player
			if (plugin.KickPlayer(steamID, reason))
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "name", playerName },
					{ "steamid", steamID },
					{ "admintag", adminTag }
				};
				plugin.SendMessageByID(channelID, "botresponses.playerkicked", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "steamid", steamID }
				};
				plugin.SendMessageByID(channelID, "botresponses.playernotfound", variables);
			}
		}

		/// <summary>
		/// Kicks all players from the server
		/// </summary>
		/// <param name="channelID">The channel to send the message in</param>
		/// <param name="reason">Reason displayed to kicked players</param>
		private void KickallCommand(ulong channelID, string reason, string adminTag)
		{
			if (reason == "")
			{
				reason = "All players kicked by Admin";
			}
			foreach (Player player in plugin.PluginManager.Server.GetPlayers())
			{
				player.Ban(0, reason);
			}
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "reason", reason },
				{ "admintag", adminTag}
			};
			plugin.SendMessageByID(channelID, "botresponses.kickall", variables);
		}

		/// <summary>
		/// Returns a timestamp of the duration's end, and outputs a human readable duration for command feedback.
		/// </summary>
		/// <param name="duration">Duration of ban in format 'xu' where x is a number and u is a character representing a unit of time.</param>
		/// <param name="humanReadableDuration">String to be filled by the function with the duration in human readable form.</param>
		/// <returns>Returns a timestamp of the duration's end.</returns>
		private static DateTime ParseBanDuration(string duration, ref string humanReadableDuration)
		{
			//Check if the amount is a number
			if (!int.TryParse(new string(duration.Where(char.IsDigit).ToArray()), out int amount))
			{
				return DateTime.MinValue;
			}

			char unit = duration.Where(char.IsLetter).ToArray()[0];
			TimeSpan timeSpanDuration = new TimeSpan();

			// Parse time into a TimeSpan duration and string
			if (unit == 's')
			{
				humanReadableDuration = amount + " second";
				timeSpanDuration = new TimeSpan(0, 0, 0, amount);
			}
			else if (unit == 'm')
			{
				humanReadableDuration = amount + " minute";
				timeSpanDuration = new TimeSpan(0, 0, amount, 0);
			}
			else if (unit == 'h')
			{
				humanReadableDuration = amount + " hour";
				timeSpanDuration = new TimeSpan(0, amount, 0, 0);
			}
			else if (unit == 'd')
			{
				humanReadableDuration = amount + " day";
				timeSpanDuration = new TimeSpan(amount, 0, 0, 0);
			}
			else if (unit == 'w')
			{
				humanReadableDuration = amount + " week";
				timeSpanDuration = new TimeSpan(7 * amount, 0, 0, 0);
			}
			else if (unit == 'M')
			{
				humanReadableDuration = amount + " month";
				timeSpanDuration = new TimeSpan(30 * amount, 0, 0, 0);
			}
			else if (unit == 'y')
			{
				humanReadableDuration = amount + " year";
				timeSpanDuration = new TimeSpan(365 * amount, 0, 0, 0);
			}

			// Pluralize string if needed
			if (amount != 1)
			{
				humanReadableDuration += 's';
			}

			return DateTime.UtcNow.Add(timeSpanDuration);
		}

		/// <summary>
		/// Does very basic validation of a SteamID.
		/// </summary>
		/// <param name="steamID">A SteamID.</param>
		/// <returns>True if a possible SteamID, false if not.</returns>
		private static bool IsPossibleSteamID(string steamID)
		{
			return steamID.Length == 17 && ulong.TryParse(steamID, out ulong _);
		}
	}
}

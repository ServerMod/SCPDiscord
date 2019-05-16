using Smod2.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace SCPDiscord
{
    class BotListener
    {
        private readonly SCPDiscord plugin;
        public BotListener(SCPDiscord plugin)
        {
            this.plugin = plugin;
            while (!plugin.shutdown)
            {
                //Listen for connections
                if (NetworkSystem.IsConnected())
                {
                    try
                    {
                        //Discord messages can be up to 2000 chars long, UTF8 chars can be up to 4 bytes long.
                        byte[] data = new byte[1000];
                        int dataLength = NetworkSystem.Receive(data);

                        string incomingData = Encoding.UTF8.GetString(data, 0, dataLength);

                        List<string> messages = new List<string>(incomingData.Split('\n'));

                        //If several messages come in at the same time, process all of them
                        while (messages.Count > 0)
                        {
                            if (messages[0].Length == 0)
                            {
                                messages.RemoveAt(0);
                                continue;
                            }

                            plugin.Debug("Incoming command from discord: " + messages[0]);

                            string[] words = messages[0].Split(' ');
                            if (words[0] == "command")
                            {
                                string channel = words[1];
                                string command = words[2];
                                string[] arguments = new string[0];
                                if(words.Length >= 4)
                                {
                                    arguments = words.Skip(3).ToArray();
                                }

                                string response;
                                Dictionary<string, string> variables;

                                switch (command)
                                {
                                    case "ban":
                                        //Check if the command has enough arguments
                                        if (arguments.Length >= 2)
                                        {
                                            BanCommand(channel, arguments[0], arguments[1], MergeString(arguments.Skip(2).ToArray()));
                                        }
                                        else
                                        {
                                            variables = new Dictionary<string, string>
                                            {
                                                { "command", messages[0] }
                                            };
                                            plugin.SendMessage(channel, "botresponses.missingarguments", variables);
                                        }
                                        break;

                                    case "kick":
                                        //Check if the command has enough arguments
                                        if (arguments.Length >= 1)
                                        {
                                            KickCommand(channel, arguments[0], MergeString(arguments.Skip(1).ToArray()));
                                        }
                                        else
                                        {
                                            variables = new Dictionary<string, string>
                                            {
                                                { "command", messages[0] }
                                            };
                                            plugin.SendMessage(channel, "botresponses.missingarguments", variables);
                                        }
                                        break;

                                    case "kickall":
                                        KickallCommand(channel, MergeString(arguments));
                                        break;

                                    case "unban":
                                       //Check if the command has enough arguments
                                        if (arguments.Length >= 1)
                                        {
                                            UnbanCommand(channel, arguments[0]);
                                        }
                                        else
                                        {
                                            variables = new Dictionary<string, string>
                                            {
                                                { "command", messages[0] }
                                            };
                                            plugin.SendMessage(channel, "botresponses.missingarguments", variables);
                                        }
                                        break;

                                    case "list":
                                        var message = "```md\n# Players online (" + (plugin.Server.NumPlayers - 1) + "):\n";
                                        foreach (Player player in plugin.Server.GetPlayers())
                                        {
                                            message += player.Name.PadRight(35) + "<" + player.SteamId + ">" + "\n";
                                        }
                                        message += "```";
                                        NetworkSystem.QueueMessage(channel + message);
                                        break;

                                    case "exit":
                                        plugin.SendMessage(channel, "botresponses.exit");
                                        break;

                                    case "help":
                                        plugin.SendMessage(channel, "botresponses.help");
                                        break;

                                    case "hidetag":
                                    case "showtag":
                                        if (plugin.PluginManager.GetEnabledPlugin("karlofduty.toggletag") != null)
                                        {
                                            if (arguments.Length > 0)
                                            {
                                                command = "console_" + command;
                                                response = plugin.ConsoleCommand(plugin.PluginManager.Server, command, arguments);

                                                variables = new Dictionary<string, string>
                                                {
                                                    { "feedback", response }
                                                };
                                                plugin.SendMessage(channel, "botresponses.consolecommandfeedback", variables);
                                            }
                                            else
                                            {
                                                variables = new Dictionary<string, string>
                                                {
                                                    { "command", command }
                                                };
                                                plugin.SendMessage(channel, "botresponses.missingarguments", variables);
                                            }
                                        }
                                        else
                                        {
                                            plugin.SendMessage(channel, "botresponses.toggletag.notinstalled");
                                        }
                                        break;

                                    case "vs_enable":
                                    case "vs_disable":
                                    case "vs_whitelist":
                                    case "vs_reload":
                                        if (plugin.PluginManager.GetEnabledPlugin("karlofduty.vpnshield") != null)
                                        {
                                            response = plugin.ConsoleCommand(plugin.PluginManager.Server, command, arguments);

                                            variables = new Dictionary<string, string>
                                            {
                                                { "feedback", response }
                                            };
                                            plugin.SendMessage(channel, "botresponses.consolecommandfeedback", variables);
                                        }
                                        else
                                        {
                                            plugin.SendMessage(channel, "botresponses.vpnshield.notinstalled");
                                        }
                                        break;

									case "scperms_reload":
									case "scperms_giverank":
									case "scperms_removerank":
									case "scperms_verbose":
									case "scperms_debug":
									case "scpermissions_reload":
									case "scpermissions_giverank":
									case "scpermissions_removerank":
									case "scpermissions_verbose":
									case "scpermissions_debug":
										if (plugin.PluginManager.GetEnabledPlugin("karlofduty.scpermissions") != null)
										{
											response = plugin.ConsoleCommand(plugin.PluginManager.Server, command, arguments);

											variables = new Dictionary<string, string>
											{
												{ "feedback", response }
											};
											plugin.SendMessage(channel, "botresponses.consolecommandfeedback", variables);
										}
										else
										{
											plugin.SendMessage(channel, "botresponses.scpermissions.notinstalled");
										}
										break;
                                    case "syncrole":
                                        NetworkSystem.QueueMessage(channel + plugin.roleSync.AddPlayer(arguments[0], arguments[1]));
                                        break;

                                    case "unsyncrole":
                                        NetworkSystem.QueueMessage(channel + plugin.roleSync.RemovePlayer(arguments[0]));
                                        break;
                                    default:
                                        response = plugin.ConsoleCommand(plugin.PluginManager.Server, command, arguments);
                                        variables = new Dictionary<string, string>
                                        {
                                            { "feedback", response }
                                        };
                                        plugin.SendMessage(channel, "botresponses.consolecommandfeedback", variables);
                                        break;
                                }
                            }
                            else if (words[0] == "roleresponse")
                            {
                                plugin.roleSync.ReceiveQueryResponse(words[1], MergeString(words.Skip(2).ToArray()));
                            }
                            plugin.Verbose("From discord: " + messages[0]);

                            messages.RemoveAt(0);
                        }
                    }
                    catch (Exception ex)
                    {
                        if(ex is IOException)
                        {
                            plugin.Error("BotListener Error: " + ex);
                        }
                        plugin.Error("BotListener Error: " + ex);
                    }
                }
                Thread.Sleep(1000);
            }
        }

		/// <summary>
		/// Handles a ban command from Discord.
		/// </summary>
		/// <param name="channelID">ChannelID of the channel the command was used in.</param>
		/// <param name="steamID">SteamID of player to be banned.</param>
		/// <param name="duration">Duration of ban expressed as xu where x is a number and u is a character representing a unit of time.</param>
		/// <param name="reason">Optional reason for the ban.</param>
		private void BanCommand(string channelID, string steamID, string duration, string reason = "")
        {
            // Perform very basic SteamID validation.
            if (!IsPossibleSteamID(steamID))
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "steamid", steamID }
                };
                plugin.SendMessage(channelID, "botresponses.invalidsteamid", variables);
                return;
            }

            // Create duration timestamp.
            string humanReadableDuration = "";
            DateTime endTime;
            try
            {
                endTime = ParseBanDuration(duration, ref humanReadableDuration);
            }
            catch(IndexOutOfRangeException)
            {
                endTime = DateTime.MinValue;
            }

            if (endTime == DateTime.MinValue)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "duration", duration }
                };
                plugin.SendMessage(channelID, "botresponses.invalidduration", variables);
                return;
            }

            string name = "";
            if(!plugin.GetPlayerName(steamID, ref name))
            {
                name = "Offline player";
            }

            //Semicolons are separators in the ban file so cannot be part of strings
            name = name.Replace(";","");
            reason = reason.Replace(";","");

            if(reason == "")
            {
                reason = "No reason provided.";
            }

            // Add the player to the SteamIDBans file.
            StreamWriter streamWriter = new StreamWriter(FileManager.GetAppFolder(true) + "/SteamIdBans.txt", true);
            streamWriter.WriteLine(name + ';' + steamID + ';' + endTime.Ticks + ';' + reason + ";DISCORD;" + DateTime.UtcNow.Ticks);
            streamWriter.Dispose();

            // Kicks the player if they are online.
            plugin.KickPlayer(steamID, "Banned for the following reason: '" + reason + "'");

            Dictionary<string, string> banVars = new Dictionary<string, string>
                {
                    { "name",       name                    },
                    { "steamid",    steamID                 },
                    { "reason",     reason                  },
                    { "duration",   humanReadableDuration   }
                };
            plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.playerbanned", banVars);
        }

		/// <summary>
		/// Handles an unban command from Discord.
		/// </summary>
		/// <param name="channelID">ChannelID of discord channel command was used in.</param>
		/// <param name="steamIDOrIP">SteamID of player to be unbanned.</param>
		private void UnbanCommand(string channelID, string steamIDOrIP)
        {
            // Perform very basic SteamID and ip validation.
            if (!IsPossibleSteamID(steamIDOrIP) && !IPAddress.TryParse(steamIDOrIP, out IPAddress _))
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "steamidorip", steamIDOrIP }
                };
                plugin.SendMessage(channelID, "botresponses.invalidsteamidorip", variables);
                return;
            }

			// Get all ban entries in the files.
            List<string> ipBans = File.ReadAllLines(FileManager.GetAppFolder(true) + "/IpBans.txt").ToList();
			List<string> steamIDBans = File.ReadAllLines(FileManager.GetAppFolder(true) + "/SteamIdBans.txt").ToList();

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
			File.WriteAllLines(FileManager.GetAppFolder(true) + "/IpBans.txt", ipBans);
            File.WriteAllLines(FileManager.GetAppFolder(true) + "/SteamIdBans.txt", steamIDBans);

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
		private void KickCommand(string channelID, string steamID, string reason)
        {
            //Perform very basic SteamID validation
            if (!IsPossibleSteamID(steamID))
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "steamid", steamID }
                };
                plugin.SendMessage(channelID, "botresponses.invalidsteamid", variables);
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
                    { "steamid", steamID }
                };
                plugin.SendMessage(channelID, "botresponses.playerkicked", variables);
            }
            else
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "steamid", steamID }
                };
                plugin.SendMessage(channelID, "botresponses.playernotfound", variables);
            }
        }

		/// <summary>
		/// Kicks all players from the server
		/// </summary>
		/// <param name="channelID">The channel to send the message in</param>
		/// <param name="reason">Reason displayed to kicked players</param>
		private void KickallCommand(string channelID, string reason)
        {
            if(reason == "")
            {
                reason = "All players kicked by Admin";
            }
            foreach (Player player in plugin.PluginManager.Server.GetPlayers())
            {
                player.Ban(0, reason);
            }
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "reason", reason }
            };
            plugin.SendMessage(channelID, "botresponses.kickall", variables);
        }

		/// <summary>
		/// Merges an array of strings into a sentence.
		/// </summary>
		/// <param name="input">The string array to merge.</param>
		/// <returns>The output sentence.</returns>
		private static string MergeString(string[] input)
        {
            StringBuilder output = new StringBuilder();
            foreach(string word in input)
            {
                output.Append(word);
                output.Append(' ');
            }
            
            return output.ToString().Trim();
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
            if (!int.TryParse(new string(duration.Where(Char.IsDigit).ToArray()), out int amount))
            {
                return DateTime.MinValue;
            }

            char unit = duration.Where(Char.IsLetter).ToArray()[0];
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
                timeSpanDuration = new TimeSpan(0,0,amount,0);
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
            return (steamID.Length == 17 && long.TryParse(steamID, out long _));
        }
    }
}

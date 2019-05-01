using Smod2.API;
using Smod2.Commands;
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
                                        KickallCommand(MergeString(arguments));
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
                                                response = ConsoleCommand(plugin.PluginManager.Server, command, arguments);

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
                                            response = ConsoleCommand(plugin.PluginManager.Server, command, arguments);

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
											response = ConsoleCommand(plugin.PluginManager.Server, command, arguments);

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
                                        response = ConsoleCommand(plugin.PluginManager.Server, command, arguments);
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

        private string ConsoleCommand(ICommandSender user, string command, string[] arguments)
        {
            string[] feedback = plugin.PluginManager.CommandManager.CallCommand(user, command, arguments);

            StringBuilder builder = new StringBuilder();
            foreach (string line in feedback)
            {
                builder.Append(line + "\n");
            }
            return builder.ToString();
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
		/// <param name="steamID">SteamID of player to be unbanned.</param>
		private void UnbanCommand(string channelID, string steamID)
        {
            // Perform very basic SteamID validation. (Also secretly maybe works on ip addresses now)
            if (!IsPossibleSteamID(steamID) && !IPAddress.TryParse(steamID, out IPAddress _))
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "steamidorip", steamID }
                };
                plugin.SendMessage(channelID, "botresponses.invalidsteamidorip", variables);
                return;
            }

            // Read and save all lines to file except for the one to be unbanned
            File.WriteAllLines(FileManager.GetAppFolder(true) + "/SteamIdBans.txt", File.ReadAllLines(FileManager.GetAppFolder(true) + "/SteamIdBans.txt").Where(w => !w.Contains(steamID)).ToArray());

            // Read and save all lines to file except for the one to be unbanned
            File.WriteAllLines(FileManager.GetAppFolder(true) + "/IpBans.txt", File.ReadAllLines(FileManager.GetAppFolder(true) + "/IpBans.txt").Where(w => !w.Contains(steamID)).ToArray());

            Dictionary<string, string> unbanVars = new Dictionary<string, string>
            {
                { "steamidorip", steamID }
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
                plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.playerkicked", variables);
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
        /// <param name="reason">Reason displayed to kicked players</param>
        private void KickallCommand(string reason)
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
            plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.kickall", variables);
        }

        private static string MergeString(string[] reason)
        {
            StringBuilder output = new StringBuilder();
            foreach(string word in reason)
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

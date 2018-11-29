using Smod2.API;
using Smod2.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
                        int dataLength = NetworkSystem.Receive(out byte[] data);

                        string incomingData = "";
                        incomingData = Encoding.UTF8.GetString(data, 0, dataLength);

                        List<string> messages = new List<string>(incomingData.Split('\n'));

                        //If several messages come in at the same time, process all of them
                        while (messages.Count > 0)
                        {
                            if(Config.GetBool("settings.debug"))
                            {
                                plugin.Info("COMMAND: " + messages[0]);
                            }

                            if(messages[0].Length == 0)
                            {
                                messages.RemoveAt(0);
                                continue;
                            }
                            string[] words = messages[0].Split(' ');

                            bool isCommand = words[0] == "command";
                            string channel = words[1];
                            string command = words[2];
                            string[] arguments = new string[0];
                            if(words.Length >= 4)
                            {
                                arguments = words.Skip(3).ToArray();
                            }

                            //A verification that message is a command and not some left over string in the socket
                            if (isCommand)
                            {
                                if (command == "ban")
                                {
                                    //Check if the command has enough arguments
                                    if (arguments.Length >= 2)
                                    {
                                        BanCommand(arguments[0], arguments[1], MergeReason(arguments.Skip(2).ToArray()));
                                    }
                                    else
                                    {
                                        Dictionary<string, string> variables = new Dictionary<string, string>
                                        {
                                            { "command", messages[0] }
                                        };
                                        plugin.SendMessage(channel, "botresponses.missingarguments", variables);
                                    }
                                }
                                else if (command == "kick")
                                {
                                    //Check if the command has enough arguments
                                    if (arguments.Length >= 1)
                                    {
                                        KickCommand(arguments[0], MergeReason(arguments.Skip(1).ToArray()));
                                    }
                                    else
                                    {
                                        Dictionary<string, string> variables = new Dictionary<string, string>
                                        {
                                            { "command", messages[0] }
                                        };
                                        plugin.SendMessage(channel, "botresponses.missingarguments", variables);
                                    }
                                }
                                else if (command == "kickall")
                                {
                                    KickallCommand(MergeReason(arguments));
                                }
                                else if (command == "unban")
                                {
                                    //Check if the command has enough arguments
                                    if (arguments.Length >= 1)
                                    {
                                        UnbanCommand(arguments[0]);
                                    }
                                    else
                                    {
                                        Dictionary<string, string> variables = new Dictionary<string, string>
                                        {
                                            { "command", messages[0] }
                                        };
                                        plugin.SendMessage(channel, "botresponses.missingarguments", variables);
                                    }
                                }
                                else if (command == "list")
                                {

                                    var message = "```md\n# Players online:\n";
                                    foreach (Player player in plugin.Server.GetPlayers())
                                    {
                                        string line = player.Name.PadRight(32);
                                        line += player.SteamId;
                                        line += "\n";
                                    }
                                    message += "```";

                                    // Try to send the message to the bot
                                    try
                                    {
                                        NetworkSystem.QueueMessage(channel + message);

                                        if (Config.GetBool("settings.verbose"))
                                        {
                                            plugin.Info("Sent activity '" + message + "' to bot.");
                                        }
                                    }
                                    catch (InvalidOperationException e)
                                    {
                                        plugin.Error("Error sending activity '" + message + "' to bot.");
                                        plugin.Debug(e.ToString());
                                    }
                                    catch (ArgumentNullException e)
                                    {
                                        plugin.Error("Error sending activity '" + message + "' to bot.");
                                        plugin.Debug(e.ToString());
                                    }
                                }
                                else if (command == "exit")
                                {
                                    plugin.SendMessage(channel, "botresponses.exit");
                                }
                                else if (command == "help")
                                {
                                    plugin.SendMessage(channel, "botresponses.help");
                                }
                                else if (command == "hidetag" || command == "showtag")
                                {
                                    if (plugin.pluginManager.GetEnabledPlugin("karlofduty.toggletag") != null)
                                    {
                                        if (arguments.Length > 0)
                                        {
                                            command = "console_" + command;
                                            string response = ConsoleCommand(plugin.pluginManager.Server, command, arguments);

                                            Dictionary<string, string> variables = new Dictionary<string, string>
                                            {
                                                { "feedback", response }
                                            };
                                            plugin.SendMessage(channel, "botresponses.consolecommandfeedback", variables);
                                        }
                                        else
                                        {
                                            Dictionary<string, string> variables = new Dictionary<string, string>
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
                                }
                                else if (command == "vs_enable" || command == "vs_disable" || command == "vs_whitelist" || command == "vs_reload")
                                {
                                    if (plugin.pluginManager.GetEnabledPlugin("karlofduty.vpnshield") != null)
                                    {
                                        string response = ConsoleCommand(plugin.pluginManager.Server, command, arguments);

                                        Dictionary<string, string> variables = new Dictionary<string, string>
                                        {
                                            { "feedback", response }
                                        };
                                        plugin.SendMessage(channel, "botresponses.consolecommandfeedback", variables);
                                    }
                                    else
                                    {
                                        plugin.SendMessage(channel, "botresponses.vpnshield.notinstalled");
                                    }
                                }
                                else if(command == "syncrole")
                                {
                                    plugin.roleSync.AddPlayer(arguments[0], arguments[1]);
                                }
                                else
                                {
                                    string response = ConsoleCommand(plugin.pluginManager.Server, command, arguments);

                                    Dictionary<string, string> variables = new Dictionary<string, string>
                                    {
                                        { "feedback", response }
                                    };
                                    plugin.SendMessage(channel, "botresponses.consolecommandfeedback", variables);
                                }
                            }
                            if(Config.GetBool("settings.verbose"))
                            {
                                plugin.Info("From discord: " + messages[0]);
                            }

                            messages.RemoveAt(0);
                        }
                    }
                    catch (Exception ex)
                    {
                        if(ex is IOException)
                        {
                            plugin.Error("BotListener Error: " + ex.ToString());
                        }
                        plugin.Error("BotListener Error: " + ex.ToString());
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private string ConsoleCommand(ICommandSender user, string command, string[] arguments)
        {
            string[] feedback = plugin.pluginManager.CommandManager.CallCommand(user, command, arguments);

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
        /// <param name="steamID">SteamID of player to be banned.</param>
        /// <param name="duration">Duration of ban expressed as xu where x is a number and u is a character representing a unit of time.</param>
        /// <param name="reason">Optional reason for the ban.</param>
        private void BanCommand(string steamID, string duration, string reason = "")
        {
            // Perform very basic SteamID validation.
            if (!IsPossibleSteamID(steamID))
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "steamid", steamID }
                };
                plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.invalidsteamid", variables);
                return;
            }

            // Create duration timestamp.
            string humanReadableDuration = "";
            DateTime endTime = ParseBanDuration(duration, ref humanReadableDuration);
            if (endTime == DateTime.MinValue)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "duration", duration }
                };
                plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.invalidduration", variables);
                return;
            }

            string name = "";
            if(!plugin.GetPlayerName(steamID, ref name))
            {
                name = "Offline player";
            }

            //Semicolons are seperators in the ban file so cannot be part of strings
            name = name.Replace(";","");
            reason = reason.Replace(";","");

            if(reason == "")
            {
                reason = "No reason provided.";
            }

            // Add the player to the SteamIDBans file.
            StreamWriter streamWriter = new StreamWriter(FileManager.GetAppFolder() + "/SteamIdBans.txt", true);
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
        /// <param name="steamID">SteamID of player to be unbanned.</param>
        private void UnbanCommand(string steamID)
        {
            // Perform very basic SteamID validation. (Also secretly maybe works on ip addresses now)
            if (!IsPossibleSteamID(steamID) && !IPAddress.TryParse(steamID, out IPAddress address))
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "steamidorip", steamID }
                };
                plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.invalidsteamidorip", variables);
                return;
            }

            // Read and save all lines to file except for the one to be unbanned
            File.WriteAllLines(FileManager.GetAppFolder() + "/SteamIdBans.txt", File.ReadAllLines(FileManager.GetAppFolder() + "/SteamIdBans.txt").Where(w => !w.Contains(steamID)).ToArray());

            // Read and save all lines to file except for the one to be unbanned
            File.WriteAllLines(FileManager.GetAppFolder() + "/IpBans.txt", File.ReadAllLines(FileManager.GetAppFolder() + "/IpBans.txt").Where(w => !w.Contains(steamID)).ToArray());

            Dictionary<string, string> unbanVars = new Dictionary<string, string>
            {
                { "steamidorip", steamID }
            };
            plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.playerunbanned", unbanVars);
        }

        /// <summary>
        /// Handles the kick command.
        /// </summary>
        /// <param name="steamID">SteamID of player to be kicked.</param>
        private void KickCommand(string steamID, string reason)
        {
            //Perform very basic SteamID validation
            if (!IsPossibleSteamID(steamID))
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "steamid", steamID }
                };
                plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.invalidsteamid", variables);
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
                plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.playernotfound", variables);
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
            foreach (Smod2.API.Player player in plugin.pluginManager.Server.GetPlayers())
            {
                player.Ban(0, reason);
            }
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "reason", reason }
            };
            plugin.SendMessage(Config.GetArray("channels.statusmessages"), "botresponses.kickall", variables);
        }

        /// <summary>
        /// Merges the words of the ban reason to one string.
        /// </summary>
        /// <param name="args">The reason split into words.</param>
        /// <returns>The resulting string, empty string if no reason was given.</returns>
        private static string MergeReason(string[] reason)
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
            return (steamID.Length == 17 && long.TryParse(steamID, out long n));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SCPDiscord
{
    class BotListener
    {
        public BotListener(SCPDiscordPlugin plugin)
        {
            while (true)
            {
                //Listen for connections
                if (plugin.clientSocket.Connected)
                {
                    try
                    {
                        //Discord messages can be up to 2000 chars long, UTF8 chars can be up to 4 bytes long.
                        byte[] data = new byte[8000];

                        NetworkStream stream = plugin.clientSocket.GetStream();

                        int lengthOfData = stream.Read(data, 0, data.Length);

                        string discordMessage = System.Text.Encoding.UTF8.GetString(data, 0, lengthOfData);

                        discordMessage = discordMessage.Remove(discordMessage.Length - 2);

                        string[] args = discordMessage.Split(' ');

                        //A verification that will differenciate a message from a command in the future
                        if (args[0] == "command")
                        {
                            if (args[1] == "ban")
                            {
                                //Check if the command has enough arguments
                                if (args.Length < 4)
                                {
                                    Thread messageThread = new Thread(new ThreadStart(() => new AsyncMessage(plugin, "default", "Missing arguments.")));
                                    messageThread.Start();
                                    continue;
                                }

                                //Perform very basic SteamID validation
                                string steamID = args[2];
                                if(steamID.Length != 17 || !long.TryParse(steamID, out long n))
                                {
                                    Thread messageThread = new Thread(new ThreadStart(() => new AsyncMessage(plugin, "default", "Invalid SteamID.")));
                                    messageThread.Start();
                                    continue;
                                }

                                //Create timestamps
                                DateTime currentTime = DateTime.UtcNow;
                                DateTime endTime = ParseDuration(args[3]);
                                if(endTime == DateTime.MinValue)
                                {
                                    Thread messageThread = new Thread(new ThreadStart(() => new AsyncMessage(plugin, "default", "Invalid Duration.")));
                                    messageThread.Start();
                                    continue;
                                }

                                //Add the player to the SteamIDBans file
                                StreamWriter streamWriter = new StreamWriter(FileManager.AppFolder + "/SteamIdBans.txt", true);
                                streamWriter.WriteLine(GetName(steamID, plugin) + ';' + steamID + ';' + endTime.Ticks + ';' + MergeReason(args) + ";DISCORD;" + currentTime.Ticks);
                                streamWriter.Dispose();
                                KickPlayer(steamID, plugin);
                            }
                            else if (args[1] == "kick")
                            {
                                //Check if the command has enough arguments
                                if (args.Length < 3)
                                {
                                    Thread messageThread = new Thread(new ThreadStart(() => new AsyncMessage(plugin, "default", "Missing arguments.")));
                                    messageThread.Start();
                                    continue;
                                }

                                //Perform very basic SteamID validation
                                string steamID = args[2];
                                plugin.Info("SteamID: '" + steamID + "' SteamID Length: " + steamID.Length + ". SteamID numeric: " + long.TryParse(steamID, out long test));
                                if (steamID.Length != 17 || !long.TryParse(steamID, out long n))
                                {
                                    Thread messageThread = new Thread(new ThreadStart(() => new AsyncMessage(plugin, "default", "Invalid SteamID.")));
                                    messageThread.Start();
                                    continue;
                                }


                                KickPlayer(steamID, plugin);
                            }
                        }
                        plugin.Info("From discord: " + discordMessage);
                    }
                    catch (Exception ex)
                    {
                        plugin.Info(ex.ToString());
                    }
                }
                else
                {
                    Thread.Sleep(200);
                }
            }
        }

        // Gets the name of a player by steamid
        private string GetName(string steamID, SCPDiscordPlugin plugin)
        {
            foreach(Smod2.API.Player player in plugin.pluginManager.Server.GetPlayers())
            {
                if(player.SteamId == steamID)
                {
                    return player.Name;
                }
            }
            return "Offline player banned through Discord.";
        }

        // Kicks a player by steamid
        private bool KickPlayer(string steamID, SCPDiscordPlugin plugin)
        {
            foreach (Smod2.API.Player player in plugin.pluginManager.Server.GetPlayers())
            {
                if (player.SteamId == steamID)
                {
                    player.Ban(0);
                    return true;
                }
            }
            return false;
        }
        
        // Merges the words of the reason
        private string MergeReason(string[] args)
        {
            string output = "";
            for(int i = 4; i < args.Length; i++)
            {
                output += args[i];
                output += ' ';
            }
            return output;
        }

        // Returns a timestamp of the duration's end
        private DateTime ParseDuration(string duration)
        {
            if (!int.TryParse(new string(duration.Where(Char.IsDigit).ToArray()), out int amount))
            {
                return DateTime.MinValue;
            }
            char unit = duration.Where(Char.IsLetter).ToArray()[0];
            TimeSpan timeSpanDuration = new TimeSpan();
            
            if (unit == 's')
            {
                timeSpanDuration = new TimeSpan(0, 0, 0, amount);
            }
            else if (unit == 'm')
            {
                timeSpanDuration = new TimeSpan(0,0,amount,0);
            }
            else if (unit == 'h')
            {
                timeSpanDuration = new TimeSpan(0, amount, 0, 0);
            }
            else if (unit == 'd')
            {
                timeSpanDuration = new TimeSpan(amount, 0, 0, 0);
            }
            else if (unit == 'w')
            {
                timeSpanDuration = new TimeSpan(7 * amount, 0, 0, 0);
            }
            else if (unit == 'M')
            {
                timeSpanDuration = new TimeSpan(30 * amount, 0, 0, 0);
            }
            else if (unit == 'y')
            {
                timeSpanDuration = new TimeSpan(365 * amount, 0, 0, 0);
            }
            return DateTime.UtcNow.Add(timeSpanDuration);
        }
    }
}

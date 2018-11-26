using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SCPDiscord
{
    public class RoleSync
    {
        public Dictionary<string, string> syncedPlayers = new Dictionary<string, string>();

        SCPDiscord plugin;
        public RoleSync(SCPDiscord plugin)
        {
            this.plugin = plugin;
        }

        public void CheckPlayer(string steamID)
        {
            if(!syncedPlayers.ContainsKey(steamID))
            {
                return;
            }

            try
            {
                NetworkStream serverStream = plugin.clientSocket.GetStream();
                byte[] outStream = Encoding.UTF8.GetBytes("rolequery " + syncedPlayers[steamID] + '\0');
                serverStream.Write(outStream, 0, outStream.Length);

                if (Config.GetBool("settings.verbose"))
                {
                    plugin.Info("Requested role verification from bot.");
                }
            }
            catch (Exception e)
            {
                plugin.Error("Error requesting role verification.");
                plugin.Debug(e.ToString());
                if (!(e is InvalidOperationException || e is ArgumentNullException))
                { 
                    throw e;
                }
            }
        }

        public string AddPlayer(string steamID, string discordID)
        {
            if (syncedPlayers.ContainsKey(steamID))
            {
                return "SteamID is already linked to a Discord account";
            }

            if (syncedPlayers.ContainsValue(discordID))
            {
                return "Discord user ID is already linked to a Steam account";
            }

            syncedPlayers.Add(steamID, discordID);
            return "Successfully linked accounts.";
        }

        public string RemovePlayer(string discordID)
        {
            try
            {
                KeyValuePair<string, string> player = syncedPlayers.First(kvp => kvp.Value == discordID);
                syncedPlayers.Remove(player.Key);
                return "Discord user ID link has been removed.";
            }
            catch(InvalidOperationException)
            {
                return "Discord user ID is not linked to a Steam account";
            }
            
        }
    }
}

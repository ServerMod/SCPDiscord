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
            if (!syncedPlayers.ContainsKey(steamID))
            {
                return;
            }
            NetworkSystem.QueueMessage("rolequery" + syncedPlayers[steamID]);
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

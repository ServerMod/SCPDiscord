using Smod2.EventHandlers;
using Smod2.EventSystem.Events;
using System.Collections.Generic;

namespace SCPDiscord
{
    class TeamEventListener : IEventHandlerTeamRespawn, IEventHandlerSetNTFUnitName
    {
        private readonly SCPDiscord plugin;

        public TeamEventListener(SCPDiscord plugin)
        {
            this.plugin = plugin;
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            /// <summary>  
            /// Called before MTF or CI respawn.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "players",    ev.PlayerList.ToString()    },
                { "spawnchaos", ev.SpawnChaos.ToString()    }
            };

            if(ev.SpawnChaos)
            {
                plugin.SendMessage(Config.GetArray("channels.onteamrespawn.ci"), "team.onteamrespawn.ci", variables);
            }
            else
            {
                plugin.SendMessage(Config.GetArray("channels.onteamrespawn.mtf"), "team.onteamrespawn.mtf", variables);
            }
        }

        public void OnSetNTFUnitName(SetNTFUnitNameEvent ev)
        {
            /// <summary>  
            /// Called when the name of NTF unit is about to be set. This happens when NTF units respawn.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "name", ev.Unit }
            };
            plugin.SendMessage(Config.GetArray("channels.onsetntfunitname"), "team.onsetntfunitname", variables);
        }
    }
}

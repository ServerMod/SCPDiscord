using System.Collections.Generic;
using Smod2.EventHandlers;
using Smod2.EventSystem.Events;

namespace SCPDiscord.EventListeners
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
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "players",    ev.PlayerList.ToString()    },
                { "spawnchaos", ev.SpawnChaos.ToString()    }
            };

            if(ev.SpawnChaos)
            {
                this.plugin.SendMessage(Config.GetArray("channels.onteamrespawn.ci"), "team.onteamrespawn.ci", variables);
            }
            else
            {
                this.plugin.SendMessage(Config.GetArray("channels.onteamrespawn.mtf"), "team.onteamrespawn.mtf", variables);
            }
        }

        public void OnSetNTFUnitName(SetNTFUnitNameEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "name", ev.Unit }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onsetntfunitname"), "team.onsetntfunitname", variables);
        }
    }
}

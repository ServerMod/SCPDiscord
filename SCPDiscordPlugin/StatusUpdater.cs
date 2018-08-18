using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCPDiscord
{
    class StatusUpdater : IEventHandlerUpdate
    {
        DateTime nextUpdate = DateTime.Now;
        SCPDiscordPlugin plugin;

        public StatusUpdater(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnUpdate(UpdateEvent ev)
        {
            if(DateTime.Now > nextUpdate && plugin.hasConnectedOnce)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "players",       (plugin.pluginManager.Server.NumPlayers - 1).ToString()  },
                    { "maxplayers",     plugin.pluginManager.Server.MaxPlayers.ToString()       }
                };
                plugin.SendToBot("playercount", "botstatus.activity", variables);
                nextUpdate = DateTime.Now.AddSeconds(5);
            }
        }
    }
}

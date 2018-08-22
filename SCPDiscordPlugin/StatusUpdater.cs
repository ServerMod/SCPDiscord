using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;

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
            //if(DateTime.Now > nextUpdate && plugin.hasConnectedOnce)
            //{
            //    // Update player count
            //    if(plugin.GetConfigString("discord_activity_playercount") == "on")
            //    {
            //        plugin.RefreshBotActivity();
            //    }

            //    // Update channel topic
            //    plugin.RefreshChannelTopic();

            //    nextUpdate = DateTime.Now.AddSeconds(5);
            //}
        }
    }
}

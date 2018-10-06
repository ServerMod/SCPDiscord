using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SCPDiscord
{
    class StatusUpdater : IEventHandlerUpdate
    {
        private readonly SCPDiscordPlugin plugin;

        public StatusUpdater(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        short ticks = 0;
        Stopwatch stopWatch = Stopwatch.StartNew();
        public void OnUpdate(UpdateEvent ev)
        {
            if(!stopWatch.IsRunning)
            {
                stopWatch.Start();
            }
            ticks++;
            if (stopWatch.ElapsedMilliseconds >= 10000 && plugin.hasConnectedOnce)
            {
                stopWatch.Reset();
                float tps = ticks / 10.0f;
                ticks = 0;

                // Update player count
                if (plugin.GetConfigString("discord_activity_playercount") == "on")
                {
                    plugin.RefreshBotActivity();
                }

                // Update channel topic
                plugin.RefreshChannelTopic(tps);
            }
        }
    }
}

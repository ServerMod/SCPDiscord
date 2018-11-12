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

        private short ticks = 0;
        private readonly Stopwatch stopWatch = Stopwatch.StartNew();
        public void OnUpdate(UpdateEvent ev)
        {
            if(!stopWatch.IsRunning)
            {
                stopWatch.Start();
            }
            ticks++;
            if (stopWatch.ElapsedMilliseconds >= 10000 && plugin.hasConnectedOnce && plugin.clientSocket.Connected)
            {
                stopWatch.Reset();
                float tps = ticks / 10.0f;
                ticks = 0;

                // Update player count
                if (Config.settings.playercount)
                {
                    plugin.RefreshBotActivity();
                }

                // Update channel topic
                plugin.RefreshChannelTopic(tps);
            }
        }
    }
}

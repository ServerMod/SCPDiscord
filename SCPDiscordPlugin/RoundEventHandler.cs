using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class RoundEventHandler : IEventHandlerRoundStart, IEventHandlerRoundEnd
    {
        private SCPDiscordPlugin plugin;

        public RoundEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            plugin.SendMessageAsync("Round ended after " + (ev.Round.Duration/60) + " minutes.");
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            plugin.SendMessageAsync("Round started.");
        }
    }
}

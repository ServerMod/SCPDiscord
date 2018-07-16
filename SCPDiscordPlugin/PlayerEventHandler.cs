using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class PlayerEventHandler : IEventHandlerPlayerJoin
    {
        private SCPDiscordPlugin plugin;

        public PlayerEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            
        }
    }
}

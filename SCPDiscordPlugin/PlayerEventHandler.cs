using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class PlayerEventHandler : IEventHandlerPlayerJoin, IEventHandlerPlayerDie, IEventHandlerSpawn
    {
        private SCPDiscordPlugin plugin;

        public PlayerEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " died. Killed by " + ev.Killer.Name + ".");
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " joined the game.");
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " spawned as " + ev.Player.TeamRole.Name + ".");
        }
    }
}

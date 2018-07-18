using Smod2.EventHandlers;
using Smod2.EventSystem.Events;

namespace SCPDiscord
{
    class TeamEventHandler : IEventHandlerTeamRespawn, IEventHandlerSetRoleMaxHP, IEventHandlerDecideTeamRespawnQueue
    {
        private SCPDiscordPlugin plugin;

        public TeamEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnDecideTeamRespawnQueue(DecideRespawnQueueEvent ev)
        {
            plugin.SendMessageAsync("Respawn queue decided.");
        }

        public void OnSetRoleMaxHP(SetRoleMaxHPEvent ev)
        {
            plugin.SendMessageAsync("Max HP for roles set.");
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            if(!ev.SpawnChaos)
            {
                plugin.SendMessageAsync("MTF Epsilon-11 - \"Nine-Tailed Fox\" have arrived at the facility.");
            }
            else
            {
                plugin.SendMessageAsync("Hostile incursion detected, intruders identified as members of hostile GOI \"Chaos Insurgency\".");
            }
        }
    }
}

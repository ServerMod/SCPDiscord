using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class PlayerEventHandler : IEventHandlerPlayerJoin, IEventHandlerPlayerDie, IEventHandlerSpawn, IEventHandlerPlayerHurt, IEventHandlerPlayerPickupItem, IEventHandlerPlayerDropItem, IEventHandlerNicknameSet, IEventHandlerInitialAssignTeam, IEventHandlerSetRole, IEventHandlerCheckEscape, IEventHandlerDoorAccess, IEventHandlerIntercom
    {
        private SCPDiscordPlugin plugin;

        public PlayerEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnAssignTeam(PlayerInitialAssignTeamEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") has been assugned to team " + ev.Team + ".");
        }

        public void OnCheckEscape(PlayerCheckEscapeEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") has escaped as " + ev.Player.TeamRole + ".");
        }

        public void OnDoorAccess(PlayerDoorAccessEvent ev)
        {
            if(ev.Allow)
            {
                plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") opened a locked door " + ev.Player.TeamRole + ".");
            }
            else
            {
                plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") tried to access a locked door " + ev.Player.TeamRole + ".");
            }
        }

        public void OnIntercom(PlayerIntercomEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") started using the intercom.");
        }

        public void OnNicknameSet(PlayerNicknameSetEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") set their nickname to " + ev.Nickname + ".");
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") died. Killed by " + ev.Killer.Name + " (" + ev.Killer.SteamId + ").");
        }

        public void OnPlayerDropItem(PlayerDropItemEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") dropped item " + ev.Item + ".");
        }

        public void OnPlayerHurt(PlayerHurtEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") was hurt by " + ev.Attacker.Name + " (" + ev.Attacker.SteamId + ") using " + ev.DamageType + ".");
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") joined the game.");
        }

        public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") picked up item " + ev.Item + ".");
        }

        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") got the role " + ev.TeamRole.Name + ".");
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            plugin.SendMessageAsync(ev.Player.Name + " (" + ev.Player.SteamId + ") spawned as " + ev.Player.TeamRole.Name + ".");
        }
    }
}

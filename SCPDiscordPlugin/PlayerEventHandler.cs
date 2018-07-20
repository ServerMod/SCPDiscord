using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class PlayerEventHandler : IEventHandlerPlayerJoin, IEventHandlerPlayerDie, IEventHandlerSpawn, IEventHandlerPlayerHurt, IEventHandlerPlayerPickupItem, 
        IEventHandlerPlayerDropItem, IEventHandlerNicknameSet, IEventHandlerInitialAssignTeam, IEventHandlerSetRole, IEventHandlerCheckEscape, IEventHandlerDoorAccess,
        IEventHandlerIntercom, IEventHandlerIntercomCooldownCheck, IEventHandlerPocketDimensionExit, IEventHandlerPocketDimensionEnter, IEventHandlerPocketDimensionDie, 
        IEventHandlerThrowGrenade
    {
        private SCPDiscordPlugin plugin;

        public PlayerEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnPlayerHurt(PlayerHurtEvent ev)
        {
            /// <summary>  
            /// This is called before the player is going to take damage.
            /// In case the attacker can't be passed, attacker will be null (fall damage etc)
            /// This may be broken into two events in the future
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onplayerhurt"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(24) + ev.Attacker.Name + " (" + ev.Attacker.SteamId + plugin.MultiLanguage(25) + ev.DamageType + ".");
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            /// <summary>  
            /// This is called before the player is about to die. Be sure to check if player is SCP106 (classID 3) and if so, set spawnRagdoll to false.
            /// In case the killer can't be passed, attacker will be null, so check for that before doing something.
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onplayerdie"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(26) + ev.Killer.Name + " (" + ev.Killer.SteamId + ").");
        }

        public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
        {
            /// <summary>  
            /// This is called when a player picks up an item.
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onplayerpickupitem"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(27) + ev.Item + ".");
        }

        public void OnPlayerDropItem(PlayerDropItemEvent ev)
        {
            /// <summary>  
            /// This is called when a player drops up an item.
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onplayerdropitem"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(28) + ev.Item + ".");
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            /// <summary>  
            /// This is called when a player joins and is initialised.
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onplayerjoin"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(29));
        }

        public void OnNicknameSet(PlayerNicknameSetEvent ev)
        {
            /// <summary>  
            /// This is called when a player attempts to set their nickname after joining. This will only be called once per game join.
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onnicknameset"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(30) + ev.Nickname + ".");
        }

        public void OnAssignTeam(PlayerInitialAssignTeamEvent ev)
        {
            /// <summary>  
            /// Called when a team is picked for a player. Nothing is assigned to the player, but you can change what team the player will spawn as.
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onassignteam"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(31) + ev.Team + ".");
        }

        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            /// <summary>  
            /// Called after the player is set a class, at any point in the game. 
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onsetrole"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(32) + ev.TeamRole.Name + ".");
        }

        public void OnCheckEscape(PlayerCheckEscapeEvent ev)
        {
            /// <summary>  
            /// Called when a player is checking if they should escape (this is regardless of class)
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_oncheckescape"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(33) + ev.Player.TeamRole + ".");
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            /// <summary>  
            /// Called when a player spawns into the world
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onspawn"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(34) + ev.Player.TeamRole.Name + ".");
        }

        public void OnDoorAccess(PlayerDoorAccessEvent ev)
        {
            /// <summary>  
            /// Called when a player attempts to access a door that requires perms
            /// <summary> 
            if (ev.Allow)
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondooraccess"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(35));
            }
            else
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondooraccess"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(36));
            }
        }

        public void OnIntercom(PlayerIntercomEvent ev)
        {
            /// <summary>  
            /// Called when a player attempts to use intercom.
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onintercom"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(37));
        }

        public void OnIntercomCooldownCheck(PlayerIntercomCooldownCheckEvent ev)
        {
            /// <summary>  
            /// Called when a player attempts to use intercom. This happens before the cooldown check.
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onintercomcooldowncheck"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(38));
        }

        public void OnPocketDimensionExit(PlayerPocketDimensionExitEvent ev)
        {
            /// <summary>  
            /// Called when a player escapes from Pocket Demension
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onpocketdimensionexit"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(39));
        }

        public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
        {
            /// <summary>  
            /// Called when a player enters Pocket Demension
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onpocketdimensionenter"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(40));
        }

        public void OnPocketDimensionDie(PlayerPocketDimensionDieEvent ev)
        {
            /// <summary>  
            /// Called when a player enters the wrong way of Pocket Demension. This happens before the player is killed.
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onpocketdimensiondie"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(41));
        }

        public void OnThrowGrenade(PlayerThrowGrenadeEvent ev)
        {
            /// <summary>  
            /// Called after a player throws a grenade
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onthrowgrenade"), ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(42));
        }
    }
}

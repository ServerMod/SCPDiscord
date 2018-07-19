using Smod2.EventHandlers;
using Smod2.EventSystem.Events;

namespace SCPDiscord
{
    class TeamEventHandler : IEventHandlerTeamRespawn, IEventHandlerSetRoleMaxHP, IEventHandlerDecideTeamRespawnQueue, IEventHandlerSetSCPConfig
    {
        private SCPDiscordPlugin plugin;

        public TeamEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnDecideTeamRespawnQueue(DecideRespawnQueueEvent ev)
        {
            /// <summary>  
            /// Called at the start, when the team respawn queue is being read. This happens BEFORE it fills it to full with filler_team_id.
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondecideteamrespawnqueue"), "Respawn queue decided.");
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            /// <summary>  
            /// Called before MTF or CI respawn.
            /// <summary>  
            if (!ev.SpawnChaos)
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onteamrespawn"), "MTF Epsilon-11 - \"Nine-Tailed Fox\" have arrived at the facility.");
            }
            else
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onteamrespawn"), "Hostile incursion detected, intruders identified as members of hostile GOI \"Chaos Insurgency\".");
            }
        }

        public void OnSetRoleMaxHP(SetRoleMaxHPEvent ev)
        {
            /// <summary>  
            /// Called when the max HP of each role is being set. This happens every round.
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onsetrolemaxhp"), "Max HP for " + ev.Role + " set to " + ev.MaxHP + ".");
        }

        public void OnSetSCPConfig(SetSCPConfigEvent ev)
        {
            /// <summary>  
            /// Called when the configs of SCPs are being set. This happens every round.
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onsetscpconfig"), "SCP settings set.");
        }

    }
}

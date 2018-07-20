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
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondecideteamrespawnqueue"), plugin.MultiLanguage(5));
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            /// <summary>  
            /// Called before MTF or CI respawn.
            /// <summary>  
            if (!ev.SpawnChaos)
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onteamrespawn"), plugin.MultiLanguage(6));
            }
            else
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onteamrespawn"), plugin.MultiLanguage(7));
            }
        }

        public void OnSetRoleMaxHP(SetRoleMaxHPEvent ev)
        {
            /// <summary>  
            /// Called when the max HP of each role is being set. This happens every round.
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onsetrolemaxhp"), plugin.MultiLanguage(8) + ev.Role + plugin.MultiLanguage(9) + ev.MaxHP + ".");
        }

        public void OnSetSCPConfig(SetSCPConfigEvent ev)
        {
            /// <summary>  
            /// Called when the configs of SCPs are being set. This happens every round.
            /// <summary>  
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onsetscpconfig"), plugin.MultiLanguage(10));
        }

    }
}

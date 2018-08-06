using Smod2.EventHandlers;
using Smod2.EventSystem.Events;
using System.Collections.Generic;

namespace SCPDiscord
{
    class TeamEventListener : IEventHandlerTeamRespawn, IEventHandlerSetRoleMaxHP, IEventHandlerDecideTeamRespawnQueue, IEventHandlerSetSCPConfig
    {
        private SCPDiscordPlugin plugin;

        public TeamEventListener(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnDecideTeamRespawnQueue(DecideRespawnQueueEvent ev)
        {
            /// <summary>  
            /// Called at the start, when the team respawn queue is being read. This happens BEFORE it fills it to full with filler_team_id.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "teams", ev.Teams.ToString() },
            };
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_ondecideteamrespawnqueue"), "team.ondecideteamrespawnqueue", variables);
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            /// <summary>  
            /// Called before MTF or CI respawn.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "players",    ev.PlayerList.ToString()    },
                { "spawnchaos", ev.SpawnChaos.ToString()    }
            };

            if(ev.SpawnChaos)
            {
                plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onteamrespawn"), "team.onteamrespawn.cispawn", variables);
            }
            else
            {
                plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onteamrespawn"), "team.onteamrespawn", variables);
            }

        }

        public void OnSetRoleMaxHP(SetRoleMaxHPEvent ev)
        {
            /// <summary>  
            /// Called when the max HP of each role is being set. This happens every round.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "maxhp",  ev.MaxHP.ToString() },
                { "role",   ev.Role.ToString()  }
            };
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onsetrolemaxhp"), "Max HP for " + ev.Role + " set to " + ev.MaxHP + ".");
        }

        public void OnSetSCPConfig(SetSCPConfigEvent ev)
        {
            /// <summary>  
            /// Called when the configs of SCPs are being set. This happens every round.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "banned049",      ev.Ban049.ToString()            },
                { "banned079",      ev.Ban079.ToString()            },
                { "banned096",      ev.Ban096.ToString()            },
                { "banned106",      ev.Ban106.ToString()            },
                { "banned173",      ev.Ban173.ToString()            },
                { "banned939_53",   ev.Ban939_53.ToString()         },
                { "banned939_89",   ev.Ban939_89.ToString()         },
                { "049amount",      ev.SCP049amount.ToString()      },
                { "079amount",      ev.SCP079amount.ToString()      },
                { "096amount",      ev.SCP096amount.ToString()      },
                { "106amount",      ev.SCP106amount.ToString()      },
                { "173amount",      ev.SCP173amount.ToString()      },
                { "939_53amount",   ev.SCP939_53amount.ToString()   },
                { "939_89amount",   ev.SCP939_89amount.ToString()   }
            };
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onsetscpconfig"), "SCP settings set.");
        }

    }
}

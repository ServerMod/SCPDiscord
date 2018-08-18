using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace SCPDiscord
{
    class EnvironmentEventListener : IEventHandlerSCP914Activate, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown, IEventHandlerWarheadDetonate, IEventHandlerLCZDecontaminate
    {
        private SCPDiscordPlugin plugin;

        public EnvironmentEventListener(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnSCP914Activate(SCP914ActivateEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for when a SCP914 is activated
            /// </summary> 
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "inputs",         ev.Inputs.ToString()        },
                { "knobsetting",    ev.KnobSetting.ToString()   }
            };
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onscp914activate"), "environment.onscp914activate", variables);
        }

        public void OnStartCountdown(WarheadStartEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for when the warhead starts counting down, isResumed is false if its the initial count down. Note: activator can be null
            /// </summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "isresumed",      ev.IsResumed.ToString()                 },
                { "timeleft",       ev.TimeLeft.ToString()                  },
                { "ipaddress",      ev.Activator.IpAddress                  },
                { "name",           ev.Activator.Name                       },
                { "playerid",       ev.Activator.PlayerId.ToString()        },
                { "steamid",        ev.Activator.SteamId                    },
                { "class",          ev.Activator.TeamRole.Role.ToString()   },
                { "team",           ev.Activator.TeamRole.Team.ToString()   }
            };

            if(ev.IsResumed)
            {
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onstartcountdown"), "environment.onstartcountdown.countdownresumed", variables);
            }
            else
            {
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onstartcountdown"), "environment.onstartcountdown", variables);
            }
        }

        public void OnStopCountdown(WarheadStopEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for when the warhead stops counting down.
            /// </summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "timeleft",       ev.TimeLeft.ToString()                  },
                { "ipaddress",      ev.Activator.IpAddress                  },
                { "name",           ev.Activator.Name                       },
                { "playerid",       ev.Activator.PlayerId.ToString()        },
                { "steamid",        ev.Activator.SteamId                    },
                { "class",          ev.Activator.TeamRole.Role.ToString()   },
                { "team",           ev.Activator.TeamRole.Team.ToString()   }
            };
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onstopcountdown"), "environment.onstopcountdown", variables);
        }

        public void OnDetonate()
        {
            /// <summary>  
            ///  This is the event handler for when the warhead is about to detonate (so before it actually triggers)
            /// </summary> 
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_ondetonate"), "environment.ondetonate");
        }

        public void OnDecontaminate()
        {
            /// <summary>  
            ///  This is the event handler for when the LCZ is decontaminated
            /// </summary> 
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_ondecontaminate"), "environment.ondecontaminate");
        }
    }
}

using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace SCPDiscord
{
    internal class EnvironmentEventListener : IEventHandlerSCP914Activate, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown,
        IEventHandlerWarheadDetonate, IEventHandlerLCZDecontaminate, IEventHandlerSummonVehicle
    {
        private readonly SCPDiscord plugin;

        public EnvironmentEventListener(SCPDiscord plugin)
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
                { "knobsetting",    ev.KnobSetting.ToString()   }
            };
            plugin.SendMessage(Config.GetArray("channels.onscp914activate"), "environment.onscp914activate", variables);
        }

        public void OnStartCountdown(WarheadStartEvent ev)
        {
            /// <summary>
            ///  This is the event handler for when the warhead starts counting down, isResumed is false if its the initial count down. Note: activator can be null
            /// </summary>

            if (ev.Activator == null)
            {
                Dictionary<string, string> vars = new Dictionary<string, string>
                {
                    { "isresumed",      ev.IsResumed.ToString()                 },
                    { "timeleft",       ev.TimeLeft.ToString()                  },
                    { "opendoorsafter", ev.OpenDoorsAfter.ToString()            }
                };
                plugin.SendMessage(Config.GetArray("channels.onstartcountdown.noplayer"), "environment.onstartcountdown.noplayer", vars);
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "isresumed",      ev.IsResumed.ToString()                 },
                { "timeleft",       ev.TimeLeft.ToString()                  },
                { "opendoorsafter", ev.OpenDoorsAfter.ToString()            },
                { "ipaddress",      ev.Activator.IpAddress                  },
                { "name",           ev.Activator.Name                       },
                { "playerid",       ev.Activator.PlayerId.ToString()        },
                { "steamid",        ev.Activator.SteamId                    },
                { "class",          ev.Activator.TeamRole.Role.ToString()   },
                { "team",           ev.Activator.TeamRole.Team.ToString()   }
            };

            if (ev.IsResumed)
            {
                plugin.SendMessage(Config.GetArray("channels.onstartcountdown.resumed"), "environment.onstartcountdown.resumed", variables);
            }
            else
            {
                plugin.SendMessage(Config.GetArray("channels.onstartcountdown.initiated"), "environment.onstartcountdown.initiated", variables);
            }
        }

        public void OnStopCountdown(WarheadStopEvent ev)
        {
            /// <summary>
            ///  This is the event handler for when the warhead stops counting down.
            /// </summary>

            if (ev.Activator == null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "timeleft",       ev.TimeLeft.ToString()                  }
                };
                plugin.SendMessage(Config.GetArray("channels.onstopcountdown.noplayer"), "environment.onstopcountdown.noplayer", variables);
            }
            else
            {
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
                plugin.SendMessage(Config.GetArray("channels.onstopcountdown.default"), "environment.onstopcountdown.default", variables);
            }
        }

        public void OnDetonate()
        {
            /// <summary>
            ///  This is the event handler for when the warhead is about to detonate (so before it actually triggers)
            /// </summary>
            plugin.SendMessage(Config.GetArray("channels.ondetonate"), "environment.ondetonate");
        }

        public void OnDecontaminate()
        {
            /// <summary>
            ///  This is the event handler for when the LCZ is decontaminated
            /// </summary>
            plugin.SendMessage(Config.GetArray("channels.ondecontaminate"), "environment.ondecontaminate");
        }

        public void OnSummonVehicle(SummonVehicleEvent ev)
        {
            /// <summary>
            /// Called when a van/chopper is summoned.
            /// </summary>

            if (ev.IsCI)
            {
                plugin.SendMessage(Config.GetArray("channels.onsummonvehicle.chaos"), "environment.onsummonvehicle.chaos");
            }
            else
            {
                plugin.SendMessage(Config.GetArray("channels.onsummonvehicle.mtf"), "environment.onsummonvehicle.mtf");
            }
        }
    }
}
using System.Collections.Generic;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord.EventListeners
{
    internal class EnvironmentEventListener : IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown,
        IEventHandlerWarheadDetonate, IEventHandlerLCZDecontaminate, IEventHandlerSummonVehicle
    {
        private readonly SCPDiscord plugin;

        public EnvironmentEventListener(SCPDiscord plugin)
        {
            this.plugin = plugin;
        }

        public void OnStartCountdown(WarheadStartEvent ev)
        {
            if (ev.Activator == null)
            {
                Dictionary<string, string> vars = new Dictionary<string, string>
                {
                    { "isresumed",      ev.IsResumed.ToString()                 },
                    { "timeleft",       ev.TimeLeft.ToString()                  },
                    { "opendoorsafter", ev.OpenDoorsAfter.ToString()            }
                };
                this.plugin.SendMessage(Config.GetArray("channels.onstartcountdown.noplayer"), "environment.onstartcountdown.noplayer", vars);
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
                this.plugin.SendMessage(Config.GetArray("channels.onstartcountdown.resumed"), "environment.onstartcountdown.resumed", variables);
            }
            else
            {
                this.plugin.SendMessage(Config.GetArray("channels.onstartcountdown.initiated"), "environment.onstartcountdown.initiated", variables);
            }
        }

        /// <summary>
        ///  This is the event handler for when the warhead stops counting down.
        /// </summary>
        public void OnStopCountdown(WarheadStopEvent ev)
        {
            if (ev.Activator == null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "timeleft",       ev.TimeLeft.ToString()                  }
                };
                this.plugin.SendMessage(Config.GetArray("channels.onstopcountdown.noplayer"), "environment.onstopcountdown.noplayer", variables);
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
                this.plugin.SendMessage(Config.GetArray("channels.onstopcountdown.default"), "environment.onstopcountdown.default", variables);
            }
        }

        /// <summary>
        ///  This is the event handler for when the warhead is about to detonate (so before it actually triggers)
        /// </summary>
        public void OnDetonate()
        {
            this.plugin.SendMessage(Config.GetArray("channels.ondetonate"), "environment.ondetonate");
        }

        /// <summary>
        ///  This is the event handler for when the LCZ is decontaminated
        /// </summary>
        public void OnDecontaminate()
        {
            this.plugin.SendMessage(Config.GetArray("channels.ondecontaminate"), "environment.ondecontaminate");
        }

        /// <summary>
        /// Called when a van/chopper is summoned.
        /// </summary>
        public void OnSummonVehicle(SummonVehicleEvent ev)
        {
            if (ev.IsCI)
            {
                this.plugin.SendMessage(Config.GetArray("channels.onsummonvehicle.chaos"), "environment.onsummonvehicle.chaos");
            }
            else
            {
                this.plugin.SendMessage(Config.GetArray("channels.onsummonvehicle.mtf"), "environment.onsummonvehicle.mtf");
            }
        }
    }
}
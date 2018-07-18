using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class EnvironmentEventHandler : IEventHandlerSCP914Activate, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown, IEventHandlerWarheadDetonate, IEventHandlerLCZDecontaminate
    {
        private SCPDiscordPlugin plugin;

        public EnvironmentEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnDecontaminate()
        {
            plugin.SendMessageAsync("Light Containment Zone de-contamination has been initiated, all biological samples will be destroyed.");
        }

        public void OnDetonate()
        {
            plugin.SendMessageAsync("Detonation of the on-site nuclear warhead has been detected. Dispatching Foundation operatives to investigate.");
        }

        public void OnSCP914Activate(SCP914ActivateEvent ev)
        {
            plugin.SendMessageAsync("SCP-914 has been activated on setting " + ev.KnobSetting + ".");
        }

        public void OnStartCountdown(WarheadStartEvent ev)
        {
            plugin.SendMessageAsync("The on-site nuclear warhead has been activated with authorization of either the Site Director or a member of the O5-council");
        }

        public void OnStopCountdown(WarheadStopEvent ev)
        {
            plugin.SendMessageAsync("The on-site nuclear warhead has been deactivated with authorization of either the Site Director or a member of the O5-council");
        }
    }
}

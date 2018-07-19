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

        public void OnSCP914Activate(SCP914ActivateEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for when a SCP914 is activated
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onscp914activate"), "SCP-914 has been activated on setting " + ev.KnobSetting + ".");
        }

        public void OnStartCountdown(WarheadStartEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for when the warhead starts counting down, isResumed is false if its the initial count down. Note: activator can be null
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onstartcountdown"), "The on-site nuclear warhead has been activated with authorization of either the Site Director or a member of the O5-council");
        }

        public void OnStopCountdown(WarheadStopEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for when the warhead stops counting down.
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onstopcountdown"), "The on-site nuclear warhead has been deactivated.");
        }

        public void OnDetonate()
        {
            /// <summary>  
            ///  This is the event handler for when the warhead is about to detonate (so before it actually triggers)
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondetonate"), "Detonation of the on-site nuclear warhead has been detected. Dispatching Foundation operatives to investigate.");
        }

        public void OnDecontaminate()
        {
            /// <summary>  
            ///  This is the event handler for when the LCZ is decontaminated
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondecontaminate"), "Light Containment Zone de-contamination has been initiated, all biological samples will be destroyed.");
        }
    }
}

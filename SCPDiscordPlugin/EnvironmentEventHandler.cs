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
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onscp914activate"), plugin.MultiLanguage(43) + ev.KnobSetting + ".");
        }

        public void OnStartCountdown(WarheadStartEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for when the warhead starts counting down, isResumed is false if its the initial count down. Note: activator can be null
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onstartcountdown"), plugin.MultiLanguage(44));
        }

        public void OnStopCountdown(WarheadStopEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for when the warhead stops counting down.
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onstopcountdown"), plugin.MultiLanguage(45));
        }

        public void OnDetonate()
        {
            /// <summary>  
            ///  This is the event handler for when the warhead is about to detonate (so before it actually triggers)
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondetonate"), plugin.MultiLanguage(46));
        }

        public void OnDecontaminate()
        {
            /// <summary>  
            ///  This is the event handler for when the LCZ is decontaminated
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondecontaminate"), plugin.MultiLanguage(47));
        }
    }
}

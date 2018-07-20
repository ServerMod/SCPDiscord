using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class RoundEventHandler : IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerConnect, IEventHandlerDisconnect, IEventHandlerWaitingForPlayers, IEventHandlerCheckRoundEnd, IEventHandlerRoundRestart, IEventHandlerSetServerName
    {
        private SCPDiscordPlugin plugin;

        public RoundEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for Round start events (before people are spawned in)
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onroundstart"), plugin.MultiLanguage(11));
        }

        public void OnConnect(ConnectEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for connection events, before players have been created, so names and what not are available. See PlayerJoin if you need that information
            /// </summary>
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onconnect"), plugin.MultiLanguage(12));
        }

        public void OnDisconnect(DisconnectEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for disconnection events.
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondisconnect"), plugin.MultiLanguage(13));
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            /// <summary>  
            ///  This event handler will call everytime the game checks for a round end
            /// </summary> 

            //Protip, don't turn this on.
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_oncheckroundend"), plugin.MultiLanguage(14));
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for Round end events (when the stats appear on screen)
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onroundend"), plugin.MultiLanguage(15) + (ev.Round.Duration/60) + plugin.MultiLanguage(16) +
                "```\n" +
                plugin.MultiLanguage(17) + ev.Round.Stats.ClassDEscaped + "/" + ev.Round.Stats.ClassDStart + "\n" +
                plugin.MultiLanguage(18) + ev.Round.Stats.ScientistsEscaped + "/" + ev.Round.Stats.ScientistsStart + "\n" +
                plugin.MultiLanguage(19) + ev.Round.Stats.SCPDead + "/" + ev.Round.Stats.SCPStart + "\n" +
                plugin.MultiLanguage(20) + ev.Round.Stats.SCPKills + "\n" +
                "```");
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server is waiting for players
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onwaitingforplayers"), plugin.MultiLanguage(21));
        }

        public void OnRoundRestart(RoundRestartEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server is about to restart
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onroundrestart"), plugin.MultiLanguage(22));
        }

        public void OnSetServerName(SetServerNameEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server name is set
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onsetservername"), plugin.MultiLanguage(23) + ev.ServerName + ".");
        }


    }
}

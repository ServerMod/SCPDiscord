using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class RoundEventHandler : IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerConnect, IEventHandlerDisconnect, IEventHandlerWaitingForPlayers, IEventHandlerCheckRoundEnd, IEventHandlerRoundRestart, IEventHandlerSetServerName
    {
        private SCPDiscordPlugin plugin;
        bool roundHasStarted = false;

        public RoundEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for Round start events (before people are spawned in)
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onroundstart"), "**Round started.**");
            roundHasStarted = true;
        }

        public void OnConnect(ConnectEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for connection events, before players have been created, so names and what not are available. See PlayerJoin if you need that information
            /// </summary>
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onconnect"), "Player attempting connection...");
        }

        public void OnDisconnect(DisconnectEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for disconnection events.
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_ondisconnect"), "A player has disconnected.");
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            /// <summary>  
            ///  This event handler will call everytime the game checks for a round end
            /// </summary> 

            //Protip, don't turn this on.
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_oncheckroundend"),"Checking if round has ended...");
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for Round end events (when the stats appear on screen)
            /// </summary>
            if(roundHasStarted && ev.Round.Duration > 60)
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onroundend"), "**Round ended after " + (ev.Round.Duration/60) + " minutes.** \n" +
                    "```\n" +
                    "Escaped D-class:    " + ev.Round.Stats.ClassDEscaped + "/" + ev.Round.Stats.ClassDStart + "\n" +
                    "Rescued Scientists: " + ev.Round.Stats.ScientistsEscaped + "/" + ev.Round.Stats.ScientistsStart + "\n" +
                    "Contained SCPs:     " + ev.Round.Stats.SCPDead + "/" + ev.Round.Stats.SCPStart + "\n" +
                    "Killed by SCP:      " + ev.Round.Stats.SCPKills + "\n" +
                    "```");
                roundHasStarted = false;
            }
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server is waiting for players
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onwaitingforplayers"), "**Server is ready and waiting for players.**");
        }

        public void OnRoundRestart(RoundRestartEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server is about to restart
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onroundrestart"), "Round is restarting...");
        }

        public void OnSetServerName(SetServerNameEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server name is set
            /// </summary> 
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onsetservername"), "Server name set to " + ev.ServerName + ".");
        }


    }
}

using SCPDiscord.Properties;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class RoundEventListener : IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerConnect, IEventHandlerDisconnect, IEventHandlerWaitingForPlayers, IEventHandlerCheckRoundEnd, IEventHandlerRoundRestart, IEventHandlerSetServerName
    {
        private SCPDiscordPlugin plugin;
        bool roundHasStarted = false;

        public RoundEventListener(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for Round start events (before people are spawned in)
            /// </summary> 
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onroundstart"), "round.onroundstart");
            roundHasStarted = true;
        }

        public void OnConnect(ConnectEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for connection events, before players have been created, so names and what not are available. See PlayerJoin if you need that information
            /// </summary>
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onconnect"), "round.onconnect", ev.Connection.IpAddress.ToString());
        }

        public void OnDisconnect(DisconnectEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for disconnection events.
            /// </summary> 
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_ondisconnect"), "round.ondisconnect");
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            /// <summary>  
            ///  This event handler will call everytime the game checks for a round end
            /// </summary> 

            //Protip, don't turn this on.
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_oncheckroundend"), "round.oncheckroundend");
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for Round end events (when the stats appear on screen)
            /// </summary>
            if(roundHasStarted && ev.Round.Duration > 60)
            {
                plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onroundend"), "round.onroundend", 
                    ev.Round.Duration.ToString(), 
                    ev.Round.Stats.ClassDAlive.ToString(), 
                    ev.Round.Stats.ClassDDead.ToString(), 
                    ev.Round.Stats.ClassDEscaped.ToString(), 
                    ev.Round.Stats.ClassDStart.ToString(), 
                    ev.Round.Stats.NTFAlive.ToString(), 
                    ev.Round.Stats.ScientistsAlive.ToString(), 
                    ev.Round.Stats.ScientistsDead.ToString(), 
                    ev.Round.Stats.ScientistsEscaped.ToString(),
                    ev.Round.Stats.ScientistsStart.ToString(), 
                    ev.Round.Stats.SCPAlive.ToString(), 
                    ev.Round.Stats.SCPDead.ToString(), 
                    ev.Round.Stats.SCPKills.ToString(), 
                    ev.Round.Stats.SCPStart.ToString(),
                    ev.Round.Stats.WarheadDetonated.ToString(), 
                    ev.Round.Stats.Zombies.ToString());
                roundHasStarted = false;
            }
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server is waiting for players
            /// </summary> 
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onwaitingforplayers"), "round.onwaitingforplayers");
        }

        public void OnRoundRestart(RoundRestartEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server is about to restart
            /// </summary> 
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onroundrestart"), "round.onroundrestart");
        }

        public void OnSetServerName(SetServerNameEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server name is set
            /// </summary> 
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onsetservername"), "round.onsetservername", ev.ServerName);
        }


    }
}

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

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            //Protip, don't turn this on.
            //plugin.SendMessageAsync("Checking if round has ended...");
        }

        public void OnConnect(ConnectEvent ev)
        {
            plugin.SendMessageAsync("Player attempting connection...");
        }

        public void OnDisconnect(DisconnectEvent ev)
        {
            plugin.SendMessageAsync("A player has disconnected. (This event cannot provide who)");
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            plugin.SendMessageAsync("**Round ended after " + (ev.Round.Duration/60) + " minutes.** \n" +
                "```\n" +
                "Escaped D-class:    " + ev.Round.Stats.ClassDEscaped + "/" + ev.Round.Stats.ClassDStart + "\n" +
                "Rescued Scientists: " + ev.Round.Stats.ScientistsEscaped + "/" + ev.Round.Stats.ScientistsStart + "\n" +
                "Contained SCPs:     " + ev.Round.Stats.SCPDead + "/" + ev.Round.Stats.SCPStart + "\n" +
                "Killed by SCP:      " + ev.Round.Stats.SCPKills + "\n" +
                "```");
        }

        public void OnRoundRestart(RoundRestartEvent ev)
        {
            plugin.SendMessageAsync("Round is restarting...");
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            plugin.SendMessageAsync("Round started.");
        }

        public void OnSetServerName(SetServerNameEvent ev)
        {
            plugin.SendMessageAsync("Server name set.");
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            plugin.SendMessageAsync("Server is ready and waiting for players.");
        }
    }
}

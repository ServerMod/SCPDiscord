using SCPDiscord.Properties;
using Smod2;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;
using System.Text;

namespace SCPDiscord
{
    class RoundEventListener : IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerConnect, IEventHandlerDisconnect, IEventHandlerWaitingForPlayers, 
        IEventHandlerCheckRoundEnd, IEventHandlerRoundRestart, IEventHandlerSetServerName, IEventHandlerSceneChanged
    {
        private readonly SCPDiscordPlugin plugin;
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
            plugin.SendMessageToBot(Config.channels.onroundstart, "round.onroundstart");
            roundHasStarted = true;
        }

        public void OnConnect(ConnectEvent ev)
        {
            /// <summary>
            ///  This is the event handler for connection events, before players have been created, so names and what not are available. See PlayerJoin if you need that information
            /// </summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress", ev.Connection.IpAddress.ToString() }
            };
            plugin.SendMessageToBot(Config.channels.onconnect, "round.onconnect", variables);
        }

        public void OnDisconnect(DisconnectEvent ev)
        {
            /// <summary>
            ///  This is the event handler for disconnection events.
            /// </summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress", ev.Connection.IpAddress.ToString() }
            };
            plugin.SendMessageToBot(Config.channels.ondisconnect, "round.ondisconnect", variables);
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            /// <summary>  
            ///  This event handler will call everytime the game checks for a round end
            /// </summary> 

            //Protip, don't turn this on.
            plugin.SendMessageToBot(Config.channels.oncheckroundend, "round.oncheckroundend");
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            /// <summary>  
            ///  This is the event handler for Round end events (when the stats appear on screen)
            /// </summary>
            if(roundHasStarted && ev.Round.Duration > 60)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "duration",          (ev.Round.Duration/60).ToString()            },
                    { "dclassalive",        ev.Round.Stats.ClassDAlive.ToString()       },
                    { "dclassdead",         ev.Round.Stats.ClassDDead.ToString()        },
                    { "dclassescaped",      ev.Round.Stats.ClassDEscaped.ToString()     },
                    { "dclassstart",        ev.Round.Stats.ClassDStart.ToString()       },
                    { "mtfalive",           ev.Round.Stats.NTFAlive.ToString()          },
                    { "scientistsalive",    ev.Round.Stats.ScientistsAlive.ToString()   },
                    { "scientistsdead",     ev.Round.Stats.ScientistsDead.ToString()    },
                    { "scientistsescaped",  ev.Round.Stats.ScientistsEscaped.ToString() },
                    { "scientistsstart",    ev.Round.Stats.ScientistsStart.ToString()   },
                    { "scpalive",           ev.Round.Stats.SCPAlive.ToString()          },
                    { "scpdead",            ev.Round.Stats.SCPDead.ToString()           },
                    { "scpkills",           ev.Round.Stats.SCPKills.ToString()          },
                    { "scpstart",           ev.Round.Stats.SCPStart.ToString()          },
                    { "warheaddetonated",   ev.Round.Stats.WarheadDetonated.ToString()  },
                    { "zombies",            ev.Round.Stats.Zombies.ToString()           }
                };
                plugin.SendMessageToBot(Config.channels.onroundend, "round.onroundend", variables);
                roundHasStarted = false;
            }
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server is waiting for players
            /// </summary> 
            plugin.SendMessageToBot(Config.channels.onwaitingforplayers, "round.onwaitingforplayers");
        }

        public void OnRoundRestart(RoundRestartEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server is about to restart
            /// </summary> 
            plugin.SendMessageToBot(Config.channels.onroundrestart, "round.onroundrestart");
        }

        public void OnSetServerName(SetServerNameEvent ev)
        {
            /// <summary>  
            ///  This event handler will call when the server name is set
            /// </summary>
            ev.ServerName = (ConfigManager.Manager.Config.GetBoolValue("discord_metrics", true)) ? ev.ServerName += "<color=#ffffff00><size=1>SCPD:" + plugin.Details.version + "</size></color>" : ev.ServerName; 

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "servername", ev.ServerName }
            };
            plugin.SendMessageToBot(Config.channels.onsetservername, "round.onsetservername", variables);
        }

        public void OnSceneChanged(SceneChangedEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "scenename", ev.SceneName }
            };
            plugin.SendMessageToBot(Config.channels.onscenechanged, "round.onscenechanged", variables);
        }
    }
}

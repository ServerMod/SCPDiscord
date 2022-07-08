using Smod2;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace SCPDiscord.EventListeners
{
	internal class RoundEventListener : IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerConnect, IEventHandlerWaitingForPlayers,
		IEventHandlerCheckRoundEnd, IEventHandlerRoundRestart, IEventHandlerSetServerName, IEventHandlerSceneChanged, IEventHandlerDisconnect, IEventHandlerPlayerLeave
	{
		private readonly SCPDiscord plugin;

		public RoundEventListener(SCPDiscord plugin)
		{
			this.plugin = plugin;
		}

		/// <summary>
		///  This is the event handler for Round start events (before people are spawned in)
		/// </summary>
		public void OnRoundStart(RoundStartEvent ev)
		{
			this.plugin.SendMessage(Config.GetArray("channels.onroundstart"), "round.onroundstart");
			this.plugin.roundStarted = true;
		}

		/// <summary>
		///  This is the event handler for connection events, before players have been created, so names and what not are available. See PlayerJoin if you need that information
		/// </summary>
		public void OnConnect(ConnectEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ipaddress", ev.Connection.IpAddress }
			};
			this.plugin.SendMessage(Config.GetArray("channels.onconnect"), "round.onconnect", variables);
		}

		/// <summary>
		///  This is the event handler for disconnection events.
		/// </summary>
		public void OnDisconnect(DisconnectEvent ev)
		{
			this.plugin.SendMessage(Config.GetArray("channels.ondisconnect"), "round.ondisconnect");
		}

		/// <summary>
		/// This is the event handler for players leaving
		/// </summary>
		public void OnPlayerLeave(PlayerLeaveEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ipaddress", ev.Player.IpAddress           },
				{ "name", ev.Player.Name                     },
				{ "steamid", ev.Player.GetParsedUserID()               },
				{ "playerid", ev.Player.PlayerId.ToString()  }
			};
			this.plugin.SendMessage(Config.GetArray("channels.onplayerleave"), "round.onplayerleave", variables);
		}

		/// <summary>
		///  This event handler will call everytime the game checks for a round end
		/// </summary>
		public void OnCheckRoundEnd(CheckRoundEndEvent ev)
		{
			//Protip, don't turn this on.
			this.plugin.SendMessage(Config.GetArray("channels.oncheckroundend"), "round.oncheckroundend");
		}

		/// <summary>
		///  This is the event handler for Round end events (when the stats appear on screen)
		/// </summary>
		public void OnRoundEnd(RoundEndEvent ev)
		{
			if (this.plugin.roundStarted && ev.Round.Duration > 60)
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
				this.plugin.SendMessage(Config.GetArray("channels.onroundend"), "round.onroundend", variables);
				this.plugin.roundStarted = false;
			}
		}

		/// <summary>
		///  This event handler will call when the server is waiting for players
		/// </summary>
		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			this.plugin.SendMessage(Config.GetArray("channels.onwaitingforplayers"), "round.onwaitingforplayers");
		}

		/// <summary>
		///  This event handler will call when the server is about to restart
		/// </summary>
		public void OnRoundRestart(RoundRestartEvent ev)
		{
			this.plugin.SendMessage(Config.GetArray("channels.onroundrestart"), "round.onroundrestart");
		}

		/// <summary>
		///  This event handler will call when the server name is set
		/// </summary>
		public void OnSetServerName(SetServerNameEvent ev)
		{
			// ReSharper disable once StringLiteralTypo
			ev.ServerName = (ConfigManager.Manager.Config.GetBoolValue("discord_metrics", true)) ? ev.ServerName += "<color=#ffffff00><size=1>SCPD:" + this.plugin.Details.version + "</size></color>" : ev.ServerName;

			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "servername", ev.ServerName }
			};
			this.plugin.SendMessage(Config.GetArray("channels.onsetservername"), "round.onsetservername", variables);
		}

		public void OnSceneChanged(SceneChangedEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "scenename", ev.SceneName }
			};
			this.plugin.SendMessage(Config.GetArray("channels.onscenechanged"), "round.onscenechanged", variables);
		}
	}
}
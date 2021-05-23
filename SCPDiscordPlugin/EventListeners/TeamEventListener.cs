using Smod2.EventHandlers;
using Smod2.EventSystem.Events;
using System.Collections.Generic;

namespace SCPDiscord.EventListeners
{
	class TeamEventListener : IEventHandlerTeamRespawn, IEventHandlerSetRoleMaxHP, IEventHandlerDecideTeamRespawnQueue, IEventHandlerSetSCPConfig, IEventHandlerSetNTFUnitName
	{
		private readonly SCPDiscord plugin;

		public TeamEventListener(SCPDiscord plugin)
		{
			this.plugin = plugin;
		}

		/// <summary>
		/// Called at the start, when the team respawn queue is being read. This happens BEFORE it fills it to full with filler_team_id.
		/// </summary>
		public void OnDecideTeamRespawnQueue(DecideRespawnQueueEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "teams", ev.Teams.ToString() },
			};
			this.plugin.SendMessage(Config.GetArray("channels.ondecideteamrespawnqueue"), "team.ondecideteamrespawnqueue", variables);
		}

		/// <summary>
		/// Called before MTF or CI respawn.
		/// </summary>
		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "players",    ev.PlayerList.ToString()    },
				{ "spawnchaos", ev.SpawnChaos.ToString()    }
			};

			if (ev.SpawnChaos)
			{
				this.plugin.SendMessage(Config.GetArray("channels.onteamrespawn.ci"), "team.onteamrespawn.ci", variables);
			}
			else
			{
				this.plugin.SendMessage(Config.GetArray("channels.onteamrespawn.mtf"), "team.onteamrespawn.mtf", variables);
			}

		}

		/// <summary>
		/// Called when the max HP of each role is being set. This happens every round.
		/// </summary>
		public void OnSetRoleMaxHP(SetRoleMaxHPEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "maxhp",  ev.MaxHP.ToString() },
				{ "role",   ev.Role.ToString()  }
			};
			this.plugin.SendMessage(Config.GetArray("channels.onsetrolemaxhp"), "team.onsetrolemaxhp", variables);
		}

		/// <summary>
		/// Called when the configs of SCPs are being set. This happens every round.
		/// </summary>
		public void OnSetSCPConfig(SetSCPConfigEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "banned049",      ev.Ban049.ToString()            },
				{ "banned079",      ev.Ban079.ToString()            },
				{ "banned096",      ev.Ban096.ToString()            },
				{ "banned106",      ev.Ban106.ToString()            },
				{ "banned173",      ev.Ban173.ToString()            },
				{ "banned939_53",   ev.Ban939_53.ToString()         },
				{ "banned939_89",   ev.Ban939_89.ToString()         },
				{ "049amount",      ev.SCP049amount.ToString()      },
				{ "079amount",      ev.SCP079amount.ToString()      },
				{ "096amount",      ev.SCP096amount.ToString()      },
				{ "106amount",      ev.SCP106amount.ToString()      },
				{ "173amount",      ev.SCP173amount.ToString()      },
				{ "939_53amount",   ev.SCP939_53amount.ToString()   },
				{ "939_89amount",   ev.SCP939_89amount.ToString()   }
			};
			this.plugin.SendMessage(Config.GetArray("channels.onsetscpconfig"), "team.onsetscpconfig", variables);
		}

		/// <summary>
		/// Called when the name of NTF unit is about to be set. This happens when NTF units respawn.
		/// <summary>
		public void OnSetNTFUnitName(SetNTFUnitNameEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "name", ev.Unit }
			};
			this.plugin.SendMessage(Config.GetArray("channels.onsetntfunitname"), "team.onsetntfunitname", variables);
		}
	}
}

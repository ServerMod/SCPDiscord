using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace SCPDiscord.EventListeners
{
	//Comments here are my own as there were none in the Smod2 api
	internal class AdminEventListener : IEventHandlerAdminQuery, IEventHandlerBan
	{
		private readonly SCPDiscord plugin;

		public AdminEventListener(SCPDiscord plugin)
		{
			this.plugin = plugin;
		}

		public void OnAdminQuery(AdminQueryEvent ev)
		{
			// Triggered whenever an admin uses an admin command, both gui and commandline RA
			if (ev.Query == "REQUEST_DATA PLAYER_LIST SILENT")
			{
				return;
			}
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ipaddress",      ev.Admin.IpAddress                  },
				{ "name",           ev.Admin.Name                       },
				{ "playerid",       ev.Admin.PlayerId.ToString()        },
				{ "steamid",        ev.Admin.GetParsedUserID()          },
				{ "class",          ev.Admin.TeamRole.Role.ToString()     },
				{ "team",           ev.Admin.TeamRole.Team.ToString()   },
				{ "handled",        ev.Handled.ToString()               },
				{ "output",         ev.Output                           },
				{ "query",          ev.Query                            },
				{ "successful",     ev.Successful.ToString()            }
			};

			this.plugin.SendMessage(Config.GetArray("channels.onadminquery"), "admin.onadminquery", variables);
		}

		public void OnBan(BanEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "allowban",               ev.AllowBan.ToString()              },
				{ "duration",               Utilities.SecondsToCompoundTime(ev.Duration)  },
				{ "reason",                 ev.Reason                           },
				{ "result",                 ev.Result                           },
				{ "playeripaddress",        ev.Player.IpAddress                 },
				{ "playername",             ev.Player.Name                      },
				{ "playerplayerid",         ev.Player.PlayerId.ToString()       },
				{ "playersteamid",          ev.Player.GetParsedUserID()         },
				{ "playerclass",            ev.Player.TeamRole.Role.ToString()    },
				{ "playerteam",             ev.Player.TeamRole.Team.ToString()  },
				{ "issuer",                 ev.Issuer                           }
			};
			if (ev.Issuer != "Server")
			{
				if (ev.Duration == 0)
				{
					this.plugin.SendMessage(Config.GetArray("channels.onban.admin.kick"), "admin.onban.admin.kick", variables);
				}
				else
				{
					this.plugin.SendMessage(Config.GetArray("channels.onban.admin.ban"), "admin.onban.admin.ban", variables);
				}
			}
			else
			{
				if (ev.Duration == 0)
				{
					this.plugin.SendMessage(Config.GetArray("channels.onban.console.kick"), "admin.onban.console.kick", variables);
				}
				else
				{
					this.plugin.SendMessage(Config.GetArray("channels.onban.console.ban"), "admin.onban.console.ban", variables);
				}
			}
		}
	}
}
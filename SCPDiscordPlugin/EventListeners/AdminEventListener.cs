using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace SCPDiscord
{
    //Comments here are my own as there were none in the Smod2 api
    internal class AdminEventListener : IEventHandlerAdminQuery, IEventHandlerAuthCheck, IEventHandlerBan
    {
        private readonly SCPDiscord plugin;

        public AdminEventListener(SCPDiscord plugin)
        {
            this.plugin = plugin;
        }

        public void OnAdminQuery(AdminQueryEvent ev)
        {
            ///Triggered whenever an adming uses an admin command, both gui and commandline RA
            if (ev.Query == "REQUEST_DATA PLAYER_LIST SILENT")
            {
                return;
            }
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",      ev.Admin.IpAddress                  },
                { "name",           ev.Admin.Name                       },
                { "playerid",       ev.Admin.PlayerId.ToString()        },
                { "steamid",        ev.Admin.SteamId                    },
                { "class",          ev.Admin.TeamRole.Role.ToString()   },
                { "team",           ev.Admin.TeamRole.Team.ToString()   },
                { "handled",        ev.Handled.ToString()               },
                { "output",         ev.Output                           },
                { "query",          ev.Query                            },
                { "successful",     ev.Successful.ToString()            }
            };

            plugin.SendMessage(Config.GetArray("channels.onadminquery"), "admin.onadminquery", variables);
        }

        public void OnAuthCheck(AuthCheckEvent ev)
        {
            ///Probably triggered when someone gains access to the admin panel using a password, not sure
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "allow",          ev.Allow.ToString()                     },
                { "authtype",       ev.AuthType.ToString()                  },
                { "deniedmessage",  ev.DeniedMessage                        },
                { "ipaddress",      ev.Requester.IpAddress                  },
                { "name",           ev.Requester.Name                       },
                { "playerid",       ev.Requester.PlayerId.ToString()        },
                { "steamid",        ev.Requester.SteamId                    },
                { "class",          ev.Requester.TeamRole.Role.ToString()   },
                { "team",           ev.Requester.TeamRole.Team.ToString()   }
            };
            plugin.SendMessage(Config.GetArray("channels.onauthcheck"), "admin.onauthcheck", variables);
        }

        public void OnBan(BanEvent ev)
        {
            if (ev.Admin != null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "allowban",               ev.AllowBan.ToString()              },
                    { "duration",              (ev.Duration/60).ToString()          },
                    { "reason",                 ev.Reason                           },
                    { "result",                 ev.Result                           },
                    { "playeripaddress",        ev.Player.IpAddress                 },
                    { "playername",             ev.Player.Name                      },
                    { "playerplayerid",         ev.Player.PlayerId.ToString()       },
                    { "playersteamid",          ev.Player.SteamId                   },
                    { "playerclass",            ev.Player.TeamRole.Role.ToString()  },
                    { "playerteam",             ev.Player.TeamRole.Team.ToString()  },
                    { "adminipaddress",         ev.Admin.IpAddress                  },
                    { "adminname",              ev.Admin.Name                       },
                    { "adminplayerid",          ev.Admin.PlayerId.ToString()        },
                    { "adminsteamid",           ev.Admin.SteamId                    },
                    { "adminclass",             ev.Admin.TeamRole.Role.ToString()   },
                    { "adminteam",              ev.Admin.TeamRole.Team.ToString()   }
                };
                if (ev.Duration == 0)
                {
                    plugin.SendMessage(Config.GetArray("channels.onban.admin.kick"), "admin.onban.admin.kick", variables);
                }
                else
                {
                    plugin.SendMessage(Config.GetArray("channels.onban.admin.ban"), "admin.onban.admin.ban", variables);
                }
            }
            else
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "allowban",               ev.AllowBan.ToString()              },
                    { "duration",               ev.Duration.ToString()              },
                    { "reason",                 ev.Reason                           },
                    { "result",                 ev.Result                           },
                    { "playeripaddress",        ev.Player.IpAddress                 },
                    { "playername",             ev.Player.Name                      },
                    { "playerplayerid",         ev.Player.PlayerId.ToString()       },
                    { "playersteamid",          ev.Player.SteamId                   },
                    { "playerclass",            ev.Player.TeamRole.Role.ToString()  },
                    { "playerteam",             ev.Player.TeamRole.Team.ToString()  }
                };
                if (ev.Duration == 0)
                {
                    plugin.SendMessage(Config.GetArray("channels.onban.console.kick"), "admin.onban.console.kick", variables);
                }
                else
                {
                    plugin.SendMessage(Config.GetArray("channels.onban.console.ban"), "admin.onban.console.ban", variables);
                }
            }
        }
    }
}
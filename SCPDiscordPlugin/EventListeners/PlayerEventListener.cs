using System.Collections.Generic;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord.EventListeners
{
    internal class PlayerEventListener : IEventHandlerPlayerJoin, IEventHandlerPlayerDie, IEventHandlerSpawn, IEventHandlerPlayerPickupItem,
        IEventHandlerPlayerDropItem, IEventHandlerNicknameSet, IEventHandlerInitialAssignTeam, IEventHandlerSetRole,
        IEventHandlerPocketDimensionExit, IEventHandlerPocketDimensionEnter, IEventHandlerPocketDimensionDie,
        IEventHandlerThrowGrenade, IEventHandlerInfected, IEventHandlerLure, IEventHandlerContain106, IEventHandlerMedkitUse,
        IEventHandler106CreatePortal, IEventHandler106Teleport, IEventHandlerHandcuffed,
        IEventHandlerRecallZombie, IEventHandlerCallCommand, IEventHandlerReload,
        IEventHandlerGeneratorUnlock, IEventHandlerGeneratorInsertTablet, IEventHandlerGeneratorEjectTablet, IEventHandler079LevelUp
    {
        private readonly SCPDiscord plugin;

        // First dimension is target player second dimension is attacking player
        private static readonly Dictionary<int, int> teamKillingMatrix = new Dictionary<int, int>
        {
            { 1, 3 },
            { 2, 4 },
            { 3, 1 },
            { 4, 2 }
        };

        public PlayerEventListener(SCPDiscord plugin)
        {
            this.plugin = plugin;
        }

        private bool IsTeamDamage(int attackerTeam, int targetTeam)
        {
            if(!this.plugin.roundStarted)
            {
                return false;
            }
            if (attackerTeam == targetTeam)
            {
                return true;
            }
            foreach (KeyValuePair<int, int> team in teamKillingMatrix)
            {
                if (attackerTeam == team.Value && targetTeam == team.Key)
                {
                    return true;
                }
            }
            return false;
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            if (ev.Player == null || ev.Player.TeamRole.Role == Role.UNASSIGNED)
            {
                return;
            }

            if (ev.Killer == null || ev.Player.PlayerId == ev.Killer.PlayerId)
            {
                Dictionary<string, string> noKillerVar = new Dictionary<string, string>
                {
                    { "damagetype",         ev.DamageTypeVar.ToString()         },
                    { "spawnragdoll",       ev.SpawnRagdoll.ToString()          },
                    { "playeripaddress",    ev.Player.IpAddress                 },
                    { "playername",         ev.Player.Name                      },
                    { "playerplayerid",     ev.Player.PlayerId.ToString()       },
                    { "playersteamid",      ev.Player.SteamId                   },
                    { "playerclass",        ev.Player.TeamRole.Role.ToString()  },
                    { "playerteam",         ev.Player.TeamRole.Team.ToString()  }
                };
                this.plugin.SendMessage(Config.GetArray("channels.onplayerdie.nokiller"), "player.onplayerdie.nokiller", noKillerVar);
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "damagetype",         ev.DamageTypeVar.ToString()         },
                { "spawnragdoll",       ev.SpawnRagdoll.ToString()          },
                { "attackeripaddress",  ev.Killer.IpAddress                 },
                { "attackername",       ev.Killer.Name                      },
                { "attackerplayerid",   ev.Killer.PlayerId.ToString()       },
                { "attackersteamid",    ev.Killer.SteamId                   },
                { "attackerclass",      ev.Killer.TeamRole.Role.ToString()  },
                { "attackerteam",       ev.Killer.TeamRole.Team.ToString()  },
                { "playeripaddress",    ev.Player.IpAddress                 },
                { "playername",         ev.Player.Name                      },
                { "playerplayerid",     ev.Player.PlayerId.ToString()       },
                { "playersteamid",      ev.Player.SteamId                   },
                { "playerclass",        ev.Player.TeamRole.Role.ToString()  },
                { "playerteam",         ev.Player.TeamRole.Team.ToString()  }
            };

            if (this.IsTeamDamage((int)ev.Killer.TeamRole.Team, (int)ev.Player.TeamRole.Team))
            {
                this.plugin.SendMessage(Config.GetArray("channels.onplayerdie.friendlyfire"), "player.onplayerdie.friendlyfire", variables);
                return;
            }
            this.plugin.SendMessage(Config.GetArray("channels.onplayerdie.default"), "player.onplayerdie.default", variables);
        }

        /// <summary>
        /// This is called when a player picks up an item.
        /// </summary>
        public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
        {
			Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "item",         ev.Item.ToString()                    },
                { "ipaddress",    ev.Player.IpAddress                   },
                { "name",         ev.Player.Name                        },
                { "playerid",     ev.Player.PlayerId.ToString()         },
                { "steamid",      ev.Player.SteamId                     },
                { "class",        ev.Player.TeamRole.Role.ToString()    },
                { "team",         ev.Player.TeamRole.Team.ToString()    }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onplayerpickupitem"), "player.onplayerpickupitem", variables);
        }

        /// <summary>
        /// This is called when a player drops up an item.
        /// </summary>
        public void OnPlayerDropItem(PlayerDropItemEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "item",         ev.Item.ToString()                    },
                { "ipaddress",    ev.Player.IpAddress                   },
                { "name",         ev.Player.Name                        },
                { "playerid",     ev.Player.PlayerId.ToString()         },
                { "steamid",      ev.Player.SteamId                     },
                { "class",        ev.Player.TeamRole.Role.ToString()    },
                { "team",         ev.Player.TeamRole.Team.ToString()    }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onplayerdropitem"), "player.onplayerdropitem", variables);
        }

        /// <summary>
        /// This is called when a player joins and is initialized.
        /// </summary>
        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",    ev.Player.IpAddress                   },
                { "name",         ev.Player.Name                        },
                { "playerid",     ev.Player.PlayerId.ToString()         },
                { "steamid",      ev.Player.SteamId                     },
                { "class",        ev.Player.TeamRole.Role.ToString()    },
                { "team",         ev.Player.TeamRole.Team.ToString()    }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onplayerjoin"), "player.onplayerjoin", variables);
        }

        /// <summary>
        /// This is called when a player attempts to set their nickname after joining. This will only be called once per game join.
        /// </summary>
        public void OnNicknameSet(PlayerNicknameSetEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "nickname",       ev.Nickname                         },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onnicknameset"), "player.onnicknameset", variables);
        }

        /// <summary>
        /// Called when a team is picked for a player. Nothing is assigned to the player, but you can change what team the player will spawn as.
        /// </summary>
        public void OnAssignTeam(PlayerInitialAssignTeamEvent ev)
        {
            if (ev.Team == Smod2.API.Team.NONE)
            {
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Team.ToString()                  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onassignteam"), "player.onassignteam", variables);
        }

        /// <summary>
        /// Called after the player is set a class, at any point in the game.
        /// </summary>
        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            if (ev.Role == Role.UNASSIGNED)
            {
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onsetrole"), "player.onsetrole", variables);
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "spawnpos",       ev.SpawnPos.ToString()              },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };

            this.plugin.SendMessage(Config.GetArray("channels.onspawn"), "player.onspawn", variables);
        }

        public void OnPocketDimensionExit(PlayerPocketDimensionExitEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onpocketdimensionexit"), "player.onpocketdimensionexit", variables);
        }

        /// <summary>
        /// Called when a player enters Pocket Demension
        /// </summary>
        public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "damage",             ev.Damage.ToString()                },
                { "attackeripaddress",  ev.Attacker.IpAddress               },
                { "attackername",       ev.Attacker.Name                    },
                { "attackerplayerid",   ev.Attacker.PlayerId.ToString()     },
                { "attackersteamid",    ev.Attacker.SteamId                 },
                { "attackerclass",      ev.Attacker.TeamRole.Role.ToString()},
                { "attackerteam",       ev.Attacker.TeamRole.Team.ToString()},
                { "playeripaddress",    ev.Player.IpAddress                 },
                { "playername",         ev.Player.Name                      },
                { "playerplayerid",     ev.Player.PlayerId.ToString()       },
                { "playersteamid",      ev.Player.SteamId                   },
                { "playerclass",        ev.Player.TeamRole.Role.ToString()  },
                { "playerteam",         ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onpocketdimensionenter"), "player.onpocketdimensionenter", variables);
        }

        /// <summary>
        /// Called when a player enters the wrong way of Pocket Demension. This happens before the player is killed.
        /// </summary>
        public void OnPocketDimensionDie(PlayerPocketDimensionDieEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onpocketdimensiondie"), "player.onpocketdimensiondie", variables);
        }

        /// <summary>
        /// Called after a player throws a grenade
        /// </summary>
        public void OnThrowGrenade(PlayerThrowGrenadeEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "type",               ev.GrenadeType.ToString()           },
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onthrowgrenade"), "player.onthrowgrenade", variables);
        }

        /// <summary>
        /// Called when a player is cured by SCP-049
        /// </summary>
        public void OnPlayerInfected(PlayerInfectedEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "damage",                 ev.Damage.ToString()                    },
                { "infecttime",             ev.InfectTime.ToString()                },
                { "attackeripaddress",      ev.Attacker.IpAddress                   },
                { "attackername",           ev.Attacker.Name                        },
                { "attackerplayerid",       ev.Attacker.PlayerId.ToString()         },
                { "attackersteamid",        ev.Attacker.SteamId                     },
                { "attackerclass",          ev.Attacker.TeamRole.Role.ToString()    },
                { "attackerteam",           ev.Attacker.TeamRole.Team.ToString()    },
                { "playeripaddress",        ev.Attacker.IpAddress                   },
                { "playername",             ev.Player.Name                          },
                { "playerplayerid",         ev.Player.PlayerId.ToString()           },
                { "playersteamid",          ev.Player.SteamId                       },
                { "playerclass",            ev.Player.TeamRole.Role.ToString()      },
                { "playerteam",             ev.Player.TeamRole.Team.ToString()      }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onplayerinfected"), "player.onplayerinfected", variables);
        }

        public void OnLure(PlayerLureEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "allowcontain",       ev.AllowContain.ToString()          },
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };

            this.plugin.SendMessage(Config.GetArray("channels.onlure"), "player.onlure", variables);
        }

        /// <summary>
        /// Called when a player presses the button to contain SCP-106
        /// </summary>
        public void OnContain106(PlayerContain106Event ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "activatecontainment",    ev.ActivateContainment.ToString()   },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.oncontain106"), "player.oncontain106", variables);
        }

        /// <summary>
        /// Called when a player uses Medkit
        /// </summary>
        public void OnMedkitUse(PlayerMedkitUseEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "recoveredhealth",        ev.RecoverHealth.ToString()         },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onmedkituse"), "player.onmedkituse", variables);
        }

        public void On106CreatePortal(Player106CreatePortalEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.on106createportal"), "player.on106createportal", variables);
        }

        /// <summary>
        /// Called when SCP-106 teleports through portals
        /// </summary>
        public void On106Teleport(Player106TeleportEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.on106teleport"), "player.on106teleport", variables);
        }

        public void OnHandcuffed(PlayerHandcuffedEvent ev)
        {
            if (ev.Owner != null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "cuffed",             ev.Handcuffed.ToString()                },
                    { "targetipaddress",    ev.Player.IpAddress                     },
                    { "targetname",         ev.Player.Name                          },
                    { "targetplayerid",     ev.Player.PlayerId.ToString()           },
                    { "targetsteamid",      ev.Player.SteamId                       },
                    { "targetclass",        ev.Player.TeamRole.Role.ToString()      },
                    { "targetteam",         ev.Player.TeamRole.Team.ToString()      },
                    { "playeripaddress",    ev.Owner.IpAddress                      },
                    { "playername",         ev.Owner.Name                           },
                    { "playerplayerid",     ev.Owner.PlayerId.ToString()            },
                    { "playersteamid",      ev.Owner.SteamId                        },
                    { "playerclass",        ev.Owner.TeamRole.Role.ToString()       },
                    { "playerteam",         ev.Owner.TeamRole.Team.ToString()       }
                };
                this.plugin.SendMessage(Config.GetArray("channels.onhandcuff.default"), "player.onhandcuff.default", variables);
            }
            else
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "cuffed",             ev.Handcuffed.ToString()                },
                    { "targetipaddress",    ev.Player.IpAddress                     },
                    { "targetname",         ev.Player.Name                          },
                    { "targetplayerid",     ev.Player.PlayerId.ToString()           },
                    { "targetsteamid",      ev.Player.SteamId                       },
                    { "targetclass",        ev.Player.TeamRole.Role.ToString()      },
                    { "targetteam",         ev.Player.TeamRole.Team.ToString()      }
                };
                this.plugin.SendMessage(Config.GetArray("channels.onhandcuff.nootherplayer"), "player.onhandcuff.nootherplayer", variables);
            }
        }

        public void OnRecallZombie(PlayerRecallZombieEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "allowrecall",        ev.AllowRecall.ToString()               },
                { "playeripaddress",    ev.Player.IpAddress                     },
                { "playername",         ev.Player.Name                          },
                { "playerplayerid",     ev.Player.PlayerId.ToString()           },
                { "playersteamid",      ev.Player.SteamId                       },
                { "playerclass",        ev.Player.TeamRole.Role.ToString()      },
                { "playerteam",         ev.Player.TeamRole.Team.ToString()      },
                { "targetipaddress",    ev.Target.IpAddress                     },
                { "targetname",         ev.Target.Name                          },
                { "targetplayerid",     ev.Target.PlayerId.ToString()           },
                { "targetsteamid",      ev.Target.SteamId                       },
                { "targetclass",        ev.Target.TeamRole.Role.ToString()      },
                { "targetteam",         ev.Target.TeamRole.Team.ToString()      },
            };
            this.plugin.SendMessage(Config.GetArray("channels.onrecallzombie"), "player.onrecallzombie", variables);
        }

        public void OnCallCommand(PlayerCallCommandEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "command",                ev.Command                          },
                { "returnmessage",          ev.ReturnMessage                    },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.oncallcommand"), "player.oncallcommand", variables);
        }

        public void OnReload(PlayerReloadEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "weapon",                     ev.Weapon.ToString()                    },
                { "normalmaxclipsize",          ev.NormalMaxClipSize.ToString()         },
                { "ammoremoved",                ev.AmmoRemoved.ToString()               },
                { "clipammocountafterreload",   ev.ClipAmmoCountAfterReload.ToString()  },
                { "currentammototal",           ev.CurrentAmmoTotal.ToString()          },
                { "currentclipammocount",       ev.CurrentClipAmmoCount.ToString()      },
                { "ipaddress",                  ev.Player.IpAddress                     },
                { "name",                       ev.Player.Name                          },
                { "playerid",                   ev.Player.PlayerId.ToString()           },
                { "steamid",                    ev.Player.SteamId                       },
                { "class",                      ev.Player.TeamRole.Role.ToString()      },
                { "team",                       ev.Player.TeamRole.Team.ToString()      }
            };
            this.plugin.SendMessage(Config.GetArray("channels.onreload"), "player.onreload", variables);
        }

        public void OnGeneratorUnlock(PlayerGeneratorUnlockEvent ev)
        {
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "engaged",                    ev.Generator.Engaged.ToString()         },
                    { "hastablet",                  ev.Generator.HasTablet.ToString()       },
                    { "locked",                     ev.Generator.Locked.ToString()          },
                    { "open",                       ev.Generator.Open.ToString()            },
                    { "room",                       ev.Generator.Room.RoomType.ToString()   },
                    { "starttime",                  ev.Generator.StartTime.ToString()       },
                    { "timeleft",                   ev.Generator.TimeLeft.ToString()        },
                    { "ipaddress",                  ev.Player.IpAddress                     },
                    { "name",                       ev.Player.Name                          },
                    { "playerid",                   ev.Player.PlayerId.ToString()           },
                    { "steamid",                    ev.Player.SteamId                       },
                    { "class",                      ev.Player.TeamRole.Role.ToString()      },
                    { "team",                       ev.Player.TeamRole.Team.ToString()      }
                };
                this.plugin.SendMessage(Config.GetArray("channels.ongeneratorunlock"), "player.ongeneratorunlock", variables);
            }
        }

        public void OnGeneratorInsertTablet(PlayerGeneratorInsertTabletEvent ev)
        {
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "engaged",                    ev.Generator.Engaged.ToString()         },
                    { "hastablet",                  ev.Generator.HasTablet.ToString()       },
                    { "locked",                     ev.Generator.Locked.ToString()          },
                    { "open",                       ev.Generator.Open.ToString()            },
                    { "room",                       ev.Generator.Room.RoomType.ToString()   },
                    { "starttime",                  ev.Generator.StartTime.ToString()       },
                    { "timeleft",                   ev.Generator.TimeLeft.ToString()        },
                    { "ipaddress",                  ev.Player.IpAddress                     },
                    { "name",                       ev.Player.Name                          },
                    { "playerid",                   ev.Player.PlayerId.ToString()           },
                    { "steamid",                    ev.Player.SteamId                       },
                    { "class",                      ev.Player.TeamRole.Role.ToString()      },
                    { "team",                       ev.Player.TeamRole.Team.ToString()      }
                };
                this.plugin.SendMessage(Config.GetArray("channels.ongeneratorinserttablet"), "player.ongeneratorinserttablet", variables);
            }
        }

        /// <summary>
        /// Called when a player ejects the tablet.
        /// </summary>
        public void OnGeneratorEjectTablet(PlayerGeneratorEjectTabletEvent ev)
        {
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "engaged",                    ev.Generator.Engaged.ToString()         },
                    { "hastablet",                  ev.Generator.HasTablet.ToString()       },
                    { "locked",                     ev.Generator.Locked.ToString()          },
                    { "open",                       ev.Generator.Open.ToString()            },
                    { "room",                       ev.Generator.Room.RoomType.ToString()   },
                    { "starttime",                  ev.Generator.StartTime.ToString()       },
                    { "timeleft",                   ev.Generator.TimeLeft.ToString()        },
                    { "ipaddress",                  ev.Player.IpAddress                     },
                    { "name",                       ev.Player.Name                          },
                    { "playerid",                   ev.Player.PlayerId.ToString()           },
                    { "steamid",                    ev.Player.SteamId                       },
                    { "class",                      ev.Player.TeamRole.Role.ToString()      },
                    { "team",                       ev.Player.TeamRole.Team.ToString()      }
                };
                this.plugin.SendMessage(Config.GetArray("channels.ongeneratorejecttablet"), "player.ongeneratorejecttablet", variables);
            }
        }

        public void On079LevelUp(Player079LevelUpEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",                  ev.Player.IpAddress                 },
                { "name",                       ev.Player.Name                      },
                { "playerid",                   ev.Player.PlayerId.ToString()       },
                { "steamid",                    ev.Player.SteamId                   },
                { "class",                      ev.Player.TeamRole.Role.ToString()  },
                { "team",                       ev.Player.TeamRole.Team.ToString()  }
            };
            this.plugin.SendMessage(Config.GetArray("channels.on079levelup"), "player.on079levelup", variables);
        }
    }
}
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace SCPDiscord
{
    internal class PlayerEventListener : IEventHandlerPlayerJoin, IEventHandlerPlayerDie, IEventHandlerSpawn, IEventHandlerPlayerPickupItem,
        IEventHandlerPlayerDropItem, IEventHandlerNicknameSet, IEventHandlerInitialAssignTeam, IEventHandlerSetRole,
        IEventHandlerPocketDimensionExit, IEventHandlerPocketDimensionEnter, IEventHandlerPocketDimensionDie,
        IEventHandlerThrowGrenade, IEventHandlerInfected, IEventHandlerLure, IEventHandlerContain106, IEventHandlerMedkitUse,
        IEventHandler106CreatePortal, IEventHandler106Teleport, IEventHandlerHandcuffed,
        IEventHandlerRecallZombie, IEventHandlerCallCommand, IEventHandlerReload,
        IEventHandlerGeneratorUnlock, IEventHandlerGeneratorInsertTablet, IEventHandler079LevelUp
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
            if(plugin.roundStarted)
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
            /// <summary>
            /// This is called before the player is about to die. Be sure to check if player is SCP106 (classID 3) and if so, set spawnRagdoll to false.
            /// In case the killer can't be passed, attacker will be null, so check for that before doing something.
            /// </summary>

            if (ev.Player == null || ev.Player.TeamRole.Role == Smod2.API.Role.UNASSIGNED)
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
                plugin.SendMessage(Config.GetArray("channels.onplayerdie.nokiller"), "player.onplayerdie.nokiller", noKillerVar);
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

            if (IsTeamDamage((int)ev.Killer.TeamRole.Team, (int)ev.Player.TeamRole.Team))
            {
                plugin.SendMessage(Config.GetArray("channels.onplayerdie.friendlyfire"), "player.onplayerdie.friendlyfire", variables);
                return;
            }
            plugin.SendMessage(Config.GetArray("channels.onplayerdie.default"), "player.onplayerdie.default", variables);
        }

        public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
        {
            /// <summary>
            /// This is called when a player picks up an item.
            /// </summary>
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
            plugin.SendMessage(Config.GetArray("channels.onplayerpickupitem"), "player.onplayerpickupitem", variables);
        }

        public void OnPlayerDropItem(PlayerDropItemEvent ev)
        {
            /// <summary>
            /// This is called when a player drops up an item.
            /// </summary>
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
            plugin.SendMessage(Config.GetArray("channels.onplayerdropitem"), "player.onplayerdropitem", variables);
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            /// <summary>
            /// This is called when a player joins and is initialised.
            /// </summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",    ev.Player.IpAddress                   },
                { "name",         ev.Player.Name                        },
                { "playerid",     ev.Player.PlayerId.ToString()         },
                { "steamid",      ev.Player.SteamId                     },
                { "class",        ev.Player.TeamRole.Role.ToString()    },
                { "team",         ev.Player.TeamRole.Team.ToString()    }
            };
            plugin.SendMessage(Config.GetArray("channels.onplayerjoin"), "player.onplayerjoin", variables);
        }

        public void OnNicknameSet(PlayerNicknameSetEvent ev)
        {
            /// <summary>
            /// This is called when a player attempts to set their nickname after joining. This will only be called once per game join.
            /// </summary>
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
            plugin.SendMessage(Config.GetArray("channels.onnicknameset"), "player.onnicknameset", variables);
        }

        public void OnAssignTeam(PlayerInitialAssignTeamEvent ev)
        {
            /// <summary>
            /// Called when a team is picked for a player. Nothing is assigned to the player, but you can change what team the player will spawn as.
            /// <summary>
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
            plugin.SendMessage(Config.GetArray("channels.onassignteam"), "player.onassignteam", variables);
        }

        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            /// <summary>
            /// Called after the player is set a class, at any point in the game.
            /// <summary>
            if (ev.Role == Smod2.API.Role.UNASSIGNED)
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
            plugin.SendMessage(Config.GetArray("channels.onsetrole"), "player.onsetrole", variables);
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            /// <summary>
            /// Called when a player spawns into the world
            /// <summary>
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

            plugin.SendMessage(Config.GetArray("channels.onspawn"), "player.onspawn", variables);
        }

        public void OnPocketDimensionExit(PlayerPocketDimensionExitEvent ev)
        {
            /// <summary>
            /// Called when a player escapes from Pocket Demension
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onpocketdimensionexit"), "player.onpocketdimensionexit", variables);
        }

        public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
        {
            /// <summary>
            /// Called when a player enters Pocket Demension
            /// <summary>
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
            plugin.SendMessage(Config.GetArray("channels.onpocketdimensionenter"), "player.onpocketdimensionenter", variables);
        }

        public void OnPocketDimensionDie(PlayerPocketDimensionDieEvent ev)
        {
            /// <summary>
            /// Called when a player enters the wrong way of Pocket Demension. This happens before the player is killed.
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onpocketdimensiondie"), "player.onpocketdimensiondie", variables);
        }

        public void OnThrowGrenade(PlayerThrowGrenadeEvent ev)
        {
            /// <summary>
            /// Called after a player throws a grenade
            /// <summary>
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
            plugin.SendMessage(Config.GetArray("channels.onthrowgrenade"), "player.onthrowgrenade", variables);
        }

        public void OnPlayerInfected(PlayerInfectedEvent ev)
        {
            /// <summary>
            /// Called when a player is cured by SCP-049
            /// <summary>

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
            plugin.SendMessage(Config.GetArray("channels.onplayerinfected"), "player.onplayerinfected", variables);
        }

        public void OnLure(PlayerLureEvent ev)
        {
            /// <summary>
            /// Called when a player enters FemurBreaker
            /// <summary>
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

            plugin.SendMessage(Config.GetArray("channels.onlure"), "player.onlure", variables);
        }

        public void OnContain106(PlayerContain106Event ev)
        {
            /// <summary>
            /// Called when a player presses the button to contain SCP-106
            /// <summary>
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
            plugin.SendMessage(Config.GetArray("channels.oncontain106"), "player.oncontain106", variables);
        }

        public void OnMedkitUse(PlayerMedkitUseEvent ev)
        {
            /// <summary>
            /// Called when a player uses Medkit
            /// <summary>

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
            plugin.SendMessage(Config.GetArray("channels.onmedkituse"), "player.onmedkituse", variables);
        }

        public void On106CreatePortal(Player106CreatePortalEvent ev)
        {
            /// <summary>
            /// Called when SCP-106 creates a portal
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.on106createportal"), "player.on106createportal", variables);
        }

        public void On106Teleport(Player106TeleportEvent ev)
        {
            /// <summary>
            /// Called when SCP-106 teleports through portals
            /// <summary>

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.on106teleport"), "player.on106teleport", variables);
        }

        public void OnHandcuffed(PlayerHandcuffedEvent ev)
        {
            /// <summary>
            /// Called when a player handcuffs/releases another player
            /// <summary>
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
                plugin.SendMessage(Config.GetArray("channels.onhandcuff.default"), "player.onhandcuff.default", variables);
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
                plugin.SendMessage(Config.GetArray("channels.onhandcuff.nootherplayer"), "player.onhandcuff.nootherplayer", variables);
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
            plugin.SendMessage(Config.GetArray("channels.onrecallzombie"), "player.onrecallzombie", variables);
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
            plugin.SendMessage(Config.GetArray("channels.oncallcommand"), "player.oncallcommand", variables);
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
            plugin.SendMessage(Config.GetArray("channels.onreload"), "player.onreload", variables);
        }

        public void OnGeneratorUnlock(PlayerGeneratorUnlockEvent ev)
        {
            /// <summary>
            /// Called when a player attempts to unlock a generator.
            /// </summary>
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
                plugin.SendMessage(Config.GetArray("channels.ongeneratorunlock"), "player.ongeneratorunlock", variables);
            }
        }

        public void OnGeneratorInsertTablet(PlayerGeneratorInsertTabletEvent ev)
        {
            /// <summary>
            /// Called when a player puts a tablet in.
            /// </summary>
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
                plugin.SendMessage(Config.GetArray("channels.ongeneratorinserttablet"), "player.ongeneratorinserttablet", variables);
            }
        }

        public void OnGeneratorEjectTablet(PlayerGeneratorEjectTabletEvent ev)
        {
            /// <summary>
            /// Called when a player ejects the tablet.
            /// </summary>
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
                plugin.SendMessage(Config.GetArray("channels.ongeneratorejecttablet"), "player.ongeneratorejecttablet", variables);
            }
        }

        public void On079LevelUp(Player079LevelUpEvent ev)
        {
            /// <summary>
            /// Called when a player's SCP-079 level is incremented.
            /// </summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",                  ev.Player.IpAddress                 },
                { "name",                       ev.Player.Name                      },
                { "playerid",                   ev.Player.PlayerId.ToString()       },
                { "steamid",                    ev.Player.SteamId                   },
                { "class",                      ev.Player.TeamRole.Role.ToString()  },
                { "team",                       ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.on079levelup"), "player.on079levelup", variables);
        }
    }
}
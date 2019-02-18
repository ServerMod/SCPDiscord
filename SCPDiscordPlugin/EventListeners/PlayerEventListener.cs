using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace SCPDiscord
{
    internal class PlayerEventListener : IEventHandlerPlayerJoin, IEventHandlerPlayerDie, IEventHandlerSpawn, IEventHandlerPlayerHurt, IEventHandlerPlayerPickupItem,
        IEventHandlerPlayerDropItem, IEventHandlerNicknameSet, IEventHandlerInitialAssignTeam, IEventHandlerSetRole, IEventHandlerCheckEscape, IEventHandlerDoorAccess,
        IEventHandlerIntercom, IEventHandlerIntercomCooldownCheck, IEventHandlerPocketDimensionExit, IEventHandlerPocketDimensionEnter, IEventHandlerPocketDimensionDie,
        IEventHandlerThrowGrenade, IEventHandlerInfected, IEventHandlerSpawnRagdoll, IEventHandlerLure, IEventHandlerContain106, IEventHandlerMedkitUse, IEventHandlerShoot,
        IEventHandler106CreatePortal, IEventHandler106Teleport, IEventHandlerElevatorUse, IEventHandlerHandcuffed, IEventHandlerPlayerTriggerTesla, IEventHandlerSCP914ChangeKnob,
        IEventHandlerRadioSwitch, IEventHandlerMakeNoise, IEventHandlerRecallZombie, IEventHandlerCallCommand, IEventHandlerReload, IEventHandlerGrenadeExplosion, IEventHandlerGrenadeHitPlayer,
        IEventHandlerGeneratorUnlock, IEventHandlerGeneratorAccess, IEventHandlerGeneratorInsertTablet, IEventHandlerGeneratorEjectTablet, IEventHandler079Door, IEventHandler079Lock,
        IEventHandler079Elevator, IEventHandler079TeslaGate, IEventHandler079AddExp, IEventHandler079LevelUp, IEventHandler079UnlockDoors, IEventHandler079CameraTeleport, IEventHandler079StartSpeaker,
        IEventHandler079StopSpeaker, IEventHandler079Lockdown, IEventHandler079ElevatorTeleport
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

        private static bool IsTeamDamage(int attackerTeam, int targetTeam)
        {
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

        public void OnPlayerHurt(PlayerHurtEvent ev)
        {
            /// <summary>
            /// This is called before the player is going to take damage.
            /// In case the attacker can't be passed, attacker will be null (fall damage etc)
            /// This may be broken into two events in the future
            /// </summary>

            if (ev.Player == null || ev.Player.TeamRole.Role == Smod2.API.Role.UNASSIGNED)
            {
                return;
            }

            if (ev.Attacker == null || ev.Player.PlayerId == ev.Attacker.PlayerId)
            {
                Dictionary<string, string> noAttackerVar = new Dictionary<string, string>
                {
                    { "damage",             ev.Damage.ToString()                },
                    { "damagetype",         ev.DamageType.ToString()            },
                    { "playeripaddress",    ev.Player.IpAddress                 },
                    { "playername",         ev.Player.Name                      },
                    { "playerplayerid",     ev.Player.PlayerId.ToString()       },
                    { "playersteamid",      ev.Player.SteamId                   },
                    { "playerclass",        ev.Player.TeamRole.Role.ToString()  },
                    { "playerteam",         ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.onplayerhurt.noattacker"), "player.onplayerhurt.noattacker", noAttackerVar);
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "damage",             ev.Damage.ToString()                    },
                { "damagetype",         ev.DamageType.ToString()                },
                { "attackeripaddress",  ev.Attacker.IpAddress                   },
                { "attackername",       ev.Attacker.Name                        },
                { "attackerplayerid",   ev.Attacker.PlayerId.ToString()         },
                { "attackersteamid",    ev.Attacker.SteamId                     },
                { "attackerclass",      ev.Attacker.TeamRole.Role.ToString()    },
                { "attackerteam",       ev.Attacker.TeamRole.Team.ToString()    },
                { "playeripaddress",    ev.Player.IpAddress                     },
                { "playername",         ev.Player.Name                          },
                { "playerplayerid",     ev.Player.PlayerId.ToString()           },
                { "playersteamid",      ev.Player.SteamId                       },
                { "playerclass",        ev.Player.TeamRole.Role.ToString()      },
                { "playerteam",         ev.Player.TeamRole.Team.ToString()      }
            };

            if (IsTeamDamage((int)ev.Attacker.TeamRole.Team, (int)ev.Player.TeamRole.Team))
            {
                plugin.SendMessage(Config.GetArray("channels.onplayerhurt.friendlyfire"), "player.onplayerhurt.friendlyfire", variables);
                return;
            }

            plugin.SendMessage(Config.GetArray("channels.onplayerhurt.default"), "player.onplayerhurt.default", variables);
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

        public void OnCheckEscape(PlayerCheckEscapeEvent ev)
        {
            /// <summary>
            /// Called when a player is checking if they should escape (this is regardless of class)
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "allowescape",    ev.AllowEscape.ToString()           },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };

            if (ev.AllowEscape)
            {
                plugin.SendMessage(Config.GetArray("channels.oncheckescape.allowed"), "player.oncheckescape.allowed", variables);
            }
            else
            {
                plugin.SendMessage(Config.GetArray("channels.oncheckescape.denied"), "player.oncheckescape.denied", variables);
            }
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

        public void OnDoorAccess(PlayerDoorAccessEvent ev)
        {
            /// <summary>
            /// Called when a player attempts to access a door that requires perms
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "doorname",       ev.Door.Name                        },
                { "permission",     ev.Door.Permission                  },
                { "locked",         ev.Door.Locked.ToString()           },
                { "lockcooldown",   ev.Door.LockCooldown.ToString()     },
                { "open",           ev.Door.Open.ToString()             },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };
            if (ev.Allow)
            {
                plugin.SendMessage(Config.GetArray("channels.ondooraccess.allowed"), "player.ondooraccess.allowed", variables);
            }
            else
            {
                plugin.SendMessage(Config.GetArray("channels.ondooraccess.denied"), "player.ondooraccess.denied", variables);
            }
        }

        public void OnIntercom(PlayerIntercomEvent ev)
        {
            /// <summary>
            /// Called when a player attempts to use intercom.
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "cooldowntime",   ev.CooldownTime.ToString()          },
                { "speechtime",     ev.SpeechTime.ToString()            },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };

            plugin.SendMessage(Config.GetArray("channels.onintercom"), "player.onintercom", variables);
        }

        public void OnIntercomCooldownCheck(PlayerIntercomCooldownCheckEvent ev)
        {
            /// <summary>
            /// Called when a player attempts to use intercom. This happens before the cooldown check.
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "currentcooldown",    ev.CurrentCooldown.ToString()       },
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };

            plugin.SendMessage(Config.GetArray("channels.onintercomcooldowncheck"), "player.onintercomcooldowncheck", variables);
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

        public void OnSpawnRagdoll(PlayerSpawnRagdollEvent ev)
        {
            /// <summary>
            /// Called when a ragdoll is spawned
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Role.ToString()                  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onspawnragdoll"), "player.onspawnragdoll", variables);
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

        public void OnShoot(PlayerShootEvent ev)
        {
            /// <summary>
            /// Called when a player shoots
            /// <summary>

            if (ev.Player == null)
            {
                return;
            }

            if (ev.Target == null)
            {
                Dictionary<string, string> noTargetVars = new Dictionary<string, string>
                {
                    { "weapon",                 ev.Weapon.ToString()                },
                    { "attackeripaddress",      ev.Player.IpAddress                 },
                    { "attackername",           ev.Player.Name                      },
                    { "attackerplayerid",       ev.Player.PlayerId.ToString()       },
                    { "attackersteamid",        ev.Player.SteamId                   },
                    { "attackerclass",          ev.Player.TeamRole.Role.ToString()  },
                    { "attackerteam",           ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.onshoot.notarget"), "player.onshoot.notarget", noTargetVars);
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "weapon",                 ev.Weapon.ToString()                },
                { "attackeripaddress",      ev.Player.IpAddress                 },
                { "attackername",           ev.Player.Name                      },
                { "attackerplayerid",       ev.Player.PlayerId.ToString()       },
                { "attackersteamid",        ev.Player.SteamId                   },
                { "attackerclass",          ev.Player.TeamRole.Role.ToString()  },
                { "attackerteam",           ev.Player.TeamRole.Team.ToString()  },
                { "playeripaddress",        ev.Target.IpAddress                 },
                { "playername",             ev.Target.Name                      },
                { "playerplayerid",         ev.Target.PlayerId.ToString()       },
                { "playersteamid",          ev.Target.SteamId                   },
                { "playerclass",            ev.Target.TeamRole.Role.ToString()  },
                { "playerteam",             ev.Target.TeamRole.Team.ToString()  }
            };

            if (ev.Player.SteamId != ev.Target.SteamId && IsTeamDamage((int)ev.Player.TeamRole.Team, (int)ev.Target.TeamRole.Team))
            {
                plugin.SendMessage(Config.GetArray("channels.onshoot.friendlyfire"), "player.onshoot.friendlyfire", variables);
            }

            plugin.SendMessage(Config.GetArray("channels.onshoot.default"), "player.onshoot.default", variables);
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

        public void OnElevatorUse(PlayerElevatorUseEvent ev)
        {
            /// <summary>
            /// Called when a player uses an elevator
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "elevatorname",           ev.Elevator.ElevatorType.ToString() },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onelevatoruse"), "player.onelevatoruse", variables);
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

        public void OnPlayerTriggerTesla(PlayerTriggerTeslaEvent ev)
        {
            /// <summary>
            /// Called when a player triggers a tesla gate
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

            if (ev.Triggerable)
            {
                plugin.SendMessage(Config.GetArray("channels.onplayertriggertesla.default"), "player.onplayertriggertesla.default", variables);
            }
            else
            {
                plugin.SendMessage(Config.GetArray("channels.onplayertriggertesla.ignored"), "player.onplayertriggertesla.ignored", variables);
            }
        }

        public void OnSCP914ChangeKnob(PlayerSCP914ChangeKnobEvent ev)
        {
            /// <summary>
            /// Called when a player changes the knob of SCP-914
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "setting",                ev.KnobSetting.ToString()           },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onscp914changeknob"), "player.onscp914changeknob", variables);
        }

        public void OnPlayerRadioSwitch(PlayerRadioSwitchEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "setting",                ev.ChangeTo.ToString()              },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onplayerradioswitch"), "player.onplayerradioswitch", variables);
        }

        public void OnMakeNoise(PlayerMakeNoiseEvent ev)
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
            plugin.SendMessage(Config.GetArray("channels.onmakenoise"), "player.onmakenoise", variables);
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

        public void OnGrenadeExplosion(PlayerGrenadeExplosion ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",                  ev.Player.IpAddress                     },
                { "name",                       ev.Player.Name                          },
                { "playerid",                   ev.Player.PlayerId.ToString()           },
                { "steamid",                    ev.Player.SteamId                       },
                { "class",                      ev.Player.TeamRole.Role.ToString()      },
                { "team",                       ev.Player.TeamRole.Team.ToString()      }
            };
            plugin.SendMessage(Config.GetArray("channels.ongrenadeexplosion"), "player.ongrenadeexplosion", variables);
        }

        public void OnGrenadeHitPlayer(PlayerGrenadeHitPlayer ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "playeripaddress",    ev.Player.IpAddress                     },
                { "playername",         ev.Player.Name                          },
                { "playerplayerid",     ev.Player.PlayerId.ToString()           },
                { "playersteamid",      ev.Player.SteamId                       },
                { "playerclass",        ev.Player.TeamRole.Role.ToString()      },
                { "playerteam",         ev.Player.TeamRole.Team.ToString()      },
                { "targetipaddress",    ev.Victim.IpAddress                     },
                { "targetname",         ev.Victim.Name                          },
                { "targetplayerid",     ev.Victim.PlayerId.ToString()           },
                { "targetsteamid",      ev.Victim.SteamId                       },
                { "targetclass",        ev.Victim.TeamRole.Role.ToString()      },
                { "targetteam",         ev.Victim.TeamRole.Team.ToString()      },
            };
            plugin.SendMessage(Config.GetArray("channels.ongrenadehitplayer"), "player.ongrenadehitplayer", variables);
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

        public void OnGeneratorAccess(PlayerGeneratorAccessEvent ev)
        {
            /// <summary>
            /// Called when a player attempts to open/close a generator.
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
                if (ev.Generator.Open)
                {
                    plugin.SendMessage(Config.GetArray("channels.ongeneratoraccess.closed"), "player.ongeneratoraccess.closed", variables);
                }
                else
                {
                    plugin.SendMessage(Config.GetArray("channels.ongeneratoraccess.opened"), "player.ongeneratoraccess.opened", variables);
                }
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

        public void On079Door(Player079DoorEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 opens/closes doors.
            /// </summary>
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "apdrain",                    ev.APDrain.ToString()               },
                    { "door",                       ev.Door.Name                        },
                    { "open",                       ev.Door.Open.ToString()             },
                    { "ipaddress",                  ev.Player.IpAddress                 },
                    { "name",                       ev.Player.Name                      },
                    { "playerid",                   ev.Player.PlayerId.ToString()       },
                    { "steamid",                    ev.Player.SteamId                   },
                    { "class",                      ev.Player.TeamRole.Role.ToString()  },
                    { "team",                       ev.Player.TeamRole.Team.ToString()  }
                };
                if (ev.Door.Open)
                {
                    plugin.SendMessage(Config.GetArray("channels.on079door.closed"), "player.on079door.closed", variables);
                }
                else
                {
                    plugin.SendMessage(Config.GetArray("channels.on079door.opened"), "player.on079door.opened", variables);
                }
            }
        }

        public void On079Lock(Player079LockEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 locks/unlocks doors.
            /// </summary>
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "apdrain",                    ev.APDrain.ToString()               },
                    { "door",                       ev.Door.Name                        },
                    { "open",                       ev.Door.Open.ToString()             },
                    { "ipaddress",                  ev.Player.IpAddress                 },
                    { "name",                       ev.Player.Name                      },
                    { "playerid",                   ev.Player.PlayerId.ToString()       },
                    { "steamid",                    ev.Player.SteamId                   },
                    { "class",                      ev.Player.TeamRole.Role.ToString()  },
                    { "team",                       ev.Player.TeamRole.Team.ToString()  }
                };
                if (ev.Door.Locked)
                {
                    plugin.SendMessage(Config.GetArray("channels.on079lock.unlocked"), "player.on079lock.unlocked", variables);
                }
                else
                {
                    plugin.SendMessage(Config.GetArray("channels.on079lock.locked"), "player.on079lock.locked", variables);
                }
            }
        }

        public void On079Elevator(Player079ElevatorEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 sends an elevator up/down.
            /// </summary>
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "apdrain",                    ev.APDrain.ToString()               },
                    { "elevator",                   ev.Elevator.ElevatorType.ToString() },
                    { "status",                     ev.Elevator.ElevatorStatus.ToString()},
                    { "ipaddress",                  ev.Player.IpAddress                 },
                    { "name",                       ev.Player.Name                      },
                    { "playerid",                   ev.Player.PlayerId.ToString()       },
                    { "steamid",                    ev.Player.SteamId                   },
                    { "class",                      ev.Player.TeamRole.Role.ToString()  },
                    { "team",                       ev.Player.TeamRole.Team.ToString()  }
                };
                if (ev.Elevator.ElevatorStatus == ElevatorStatus.Down)
                {
                    plugin.SendMessage(Config.GetArray("channels.on079elevator.up"), "player.on079elevator.up", variables);
                }
                else if (ev.Elevator.ElevatorStatus == ElevatorStatus.Up)
                {
                    plugin.SendMessage(Config.GetArray("channels.on079elevator.down"), "player.on079elevator.down", variables);
                }
            }
        }

        public void On079TeslaGate(Player079TeslaGateEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 triggers a tesla gate.
            /// </summary>
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "apdrain",                    ev.APDrain.ToString()               },
                    { "ipaddress",                  ev.Player.IpAddress                 },
                    { "name",                       ev.Player.Name                      },
                    { "playerid",                   ev.Player.PlayerId.ToString()       },
                    { "steamid",                    ev.Player.SteamId                   },
                    { "class",                      ev.Player.TeamRole.Role.ToString()  },
                    { "team",                       ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.on079teslagate"), "player.on079teslagate", variables);
            }
        }

        public void On079AddExp(Player079AddExpEvent ev)
        {
            /// <summary>
            /// Called when a player's SCP-079 experience is added to.
            /// </summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "xptype",                     ev.ExperienceType.ToString()        },
                { "amount",                     ev.ExpToAdd.ToString()              },
                { "ipaddress",                  ev.Player.IpAddress                 },
                { "name",                       ev.Player.Name                      },
                { "playerid",                   ev.Player.PlayerId.ToString()       },
                { "steamid",                    ev.Player.SteamId                   },
                { "class",                      ev.Player.TeamRole.Role.ToString()  },
                { "team",                       ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.on079addexp"), "player.on079addexp", variables);
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

        public void On079UnlockDoors(Player079UnlockDoorsEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 unlocks all doors.
            /// </summary>
            if (ev.Allow)
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
                plugin.SendMessage(Config.GetArray("channels.on079unlockdoors"), "player.on079unlockdoors", variables);
            }
        }

        public void On079CameraTeleport(Player079CameraTeleportEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 teleports to a new camera.
            /// </summary>
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "apdrain",                    ev.APDrain.ToString()               },
                    { "ipaddress",                  ev.Player.IpAddress                 },
                    { "name",                       ev.Player.Name                      },
                    { "playerid",                   ev.Player.PlayerId.ToString()       },
                    { "steamid",                    ev.Player.SteamId                   },
                    { "class",                      ev.Player.TeamRole.Role.ToString()  },
                    { "team",                       ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.on079camerateleport"), "player.on079camerateleport", variables);
            }
        }

        public void On079StartSpeaker(Player079StartSpeakerEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 starts using a speaker.
            /// </summary>
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "apdrain",                    ev.APDrain.ToString()               },
                    { "room",                       ev.Room.RoomType.ToString()         },
                    { "ipaddress",                  ev.Player.IpAddress                 },
                    { "name",                       ev.Player.Name                      },
                    { "playerid",                   ev.Player.PlayerId.ToString()       },
                    { "steamid",                    ev.Player.SteamId                   },
                    { "class",                      ev.Player.TeamRole.Role.ToString()  },
                    { "team",                       ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.on079startspeaker"), "player.on079startspeaker", variables);
            }
        }

        public void On079StopSpeaker(Player079StopSpeakerEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 stops using a speaker.
            /// </summary>
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "room",                       ev.Room.RoomType.ToString()         },
                    { "ipaddress",                  ev.Player.IpAddress                 },
                    { "name",                       ev.Player.Name                      },
                    { "playerid",                   ev.Player.PlayerId.ToString()       },
                    { "steamid",                    ev.Player.SteamId                   },
                    { "class",                      ev.Player.TeamRole.Role.ToString()  },
                    { "team",                       ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.on079stopspeaker"), "player.on079stopspeaker", variables);
            }
        }

        public void On079Lockdown(Player079LockdownEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 uses the lockdown (warning sign) ability.
            /// </summary>
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "apdrain",                    ev.APDrain.ToString()               },
                    { "room",                       ev.Room.RoomType.ToString()         },
                    { "ipaddress",                  ev.Player.IpAddress                 },
                    { "name",                       ev.Player.Name                      },
                    { "playerid",                   ev.Player.PlayerId.ToString()       },
                    { "steamid",                    ev.Player.SteamId                   },
                    { "class",                      ev.Player.TeamRole.Role.ToString()  },
                    { "team",                       ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.on079lockdown"), "player.on079lockdown", variables);
            }
        }

        public void On079ElevatorTeleport(Player079ElevatorTeleportEvent ev)
        {
            /// <summary>
            /// Called when SCP-079 uses an elevator to teleport to a new floor.
            /// </summary>
            if (ev.Allow)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "apdrain",                    ev.APDrain.ToString()               },
                    { "elevator",                   ev.Elevator.ElevatorType.ToString() },
                    { "ipaddress",                  ev.Player.IpAddress                 },
                    { "name",                       ev.Player.Name                      },
                    { "playerid",                   ev.Player.PlayerId.ToString()       },
                    { "steamid",                    ev.Player.SteamId                   },
                    { "class",                      ev.Player.TeamRole.Role.ToString()  },
                    { "team",                       ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.on079elevatorteleport"), "player.on079elevatorteleport", variables);
            }
        }
    }
}
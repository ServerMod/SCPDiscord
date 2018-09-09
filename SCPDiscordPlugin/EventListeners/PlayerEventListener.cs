using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCPDiscord
{
    class PlayerEventListener : IEventHandlerPlayerJoin, IEventHandlerPlayerDie, IEventHandlerSpawn, IEventHandlerPlayerHurt, IEventHandlerPlayerPickupItem, 
        IEventHandlerPlayerDropItem, IEventHandlerNicknameSet, IEventHandlerInitialAssignTeam, IEventHandlerSetRole, IEventHandlerCheckEscape, IEventHandlerDoorAccess,
        IEventHandlerIntercom, IEventHandlerIntercomCooldownCheck, IEventHandlerPocketDimensionExit, IEventHandlerPocketDimensionEnter, IEventHandlerPocketDimensionDie, 
        IEventHandlerThrowGrenade, IEventHandlerInfected, IEventHandlerSpawnRagdoll, IEventHandlerLure, IEventHandlerContain106, IEventHandlerMedkitUse, IEventHandlerShoot,
        IEventHandler106CreatePortal, IEventHandler106Teleport, IEventHandlerElevatorUse
    {
        private readonly SCPDiscordPlugin plugin;
        // First dimension is target player second dimension is attacking player
        private readonly Dictionary<int,int> teamKillingMatrix = new Dictionary<int, int>
        {
            { 1, 3 },
            { 2, 4 },
            { 3, 1 },
            { 4, 2 }
        };

        public PlayerEventListener(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        private bool IsTeamDamage(int attackerTeam, int targetTeam)
        {
            if(attackerTeam == targetTeam)
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
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerhurt"), "player.onplayerhurt.noattacker", noAttackerVar);
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
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerhurt"), "player.onplayerhurt.friendlyfire", variables);
                return;
            }

            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerhurt"), "player.onplayerhurt", variables);
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
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerdie"), "player.onplayerdie.nokiller", noKillerVar);
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
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerdie"), "player.onplayerdie.friendlyfire", variables);
                return;
            }
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerdie"), "player.onplayerdie", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerpickupitem"), "player.onplayerpickupitem", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerdropitem"), "player.onplayerdropitem", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerjoin"), "player.onplayerjoin", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onnicknameset"), "player.onnicknameset", variables);
        }

        public void OnAssignTeam(PlayerInitialAssignTeamEvent ev)
        {
            /// <summary>  
            /// Called when a team is picked for a player. Nothing is assigned to the player, but you can change what team the player will spawn as.
            /// <summary>
            if(ev.Team == Smod2.API.Team.NONE)
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onassignteam"), "player.onassignteam", variables);
        }

        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            /// <summary>  
            /// Called after the player is set a class, at any point in the game. 
            /// <summary>  
            if(ev.Role == Smod2.API.Role.UNASSIGNED)
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onsetrole"), "player.onsetrole", variables);
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

            if(ev.AllowEscape)
            {
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_oncheckescape"), "player.oncheckescape", variables);
            }
            else
            {
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_oncheckescape"), "player.oncheckescape.denied", variables);
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

            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onspawn"), "player.onspawn", variables);
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
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_ondooraccess"), "player.ondooraccess", variables);
            }
            else
            {
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_ondooraccess"), "player.ondooraccess.notallowed", variables);
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

            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onintercom"), "player.onintercom", variables);
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

            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onintercomcooldowncheck"), "player.onintercomcooldowncheck", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onpocketdimensionexit"), "player.onpocketdimensionexit", variables);
        }

        public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
        {
            /// <summary>  
            /// Called when a player enters Pocket Demension
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "damage",             ev.Damage.ToString()                },
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onpocketdimensionenter"), "player.onpocketdimensionenter", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onpocketdimensiondie"), "player.onpocketdimensiondie", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onthrowgrenade"), "player.onthrowgrenade", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onplayerinfected"), "player.onplayerinfected", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onspawnragdoll"), "player.onspawnragdoll", variables);
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

            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onlure"), "player.onplayerinfected.", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_oncontain106"), "player.oncontain106", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onmedkituse"), "player.onmedkituse", variables);
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

            if(ev.Target == null)
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
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onshoot"), "player.onshoot.notarget", noTargetVars);
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
                plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onshoot"), "player.onshoot.friendlyfire", variables);
            }

            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onshoot"), "player.onshoot", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_on106createportal"), "player.on106createportal", variables);
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_on106teleport"), "player.on106teleport", variables);
        }

        public void OnElevatorUse(PlayerElevatorUseEvent ev)
        {
            /// <summary>  
            /// Called when a player uses an elevator
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
            plugin.SendMessageToBot(plugin.GetConfigString("discord_channel_onelevatoruse"), "player.onelevatoruse", variables);
        }
    }
}

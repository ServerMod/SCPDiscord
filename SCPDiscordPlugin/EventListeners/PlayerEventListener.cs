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
        IEventHandlerThrowGrenade, IEventHandlerInfected, IEventHandlerSpawnRagdoll, IEventHandlerLure, IEventHandlerContain106
    {
        private SCPDiscordPlugin plugin;
        // First dimension is target player second dimension is attacking player
        int[][] teamKillingMatrix = new int[6][] {
            new int[] { 0 },
            new int[] { 1, 3 },
            new int[] { 2, 4 },
            new int[] { 3, 1 },
            new int[] { 4, 2 },
            new int[] { 5 }
        };

        public PlayerEventListener(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnPlayerHurt(PlayerHurtEvent ev)
        {
            /// <summary>  
            /// This is called before the player is going to take damage.
            /// In case the attacker can't be passed, attacker will be null (fall damage etc)
            /// This may be broken into two events in the future
            /// </summary> 

            if (ev.Player == null)
            {
                return;
            }

            if (ev.Attacker == null)
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
                plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerhurt"), "player.onplayerhurt.noattacker", noAttackerVar);
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

            if (ev.Player.SteamId != ev.Attacker.SteamId)
            {
                foreach (int friendlyTeam in teamKillingMatrix[(int)ev.Attacker.TeamRole.Team])
                {
                    if ((int)ev.Player.TeamRole.Team == friendlyTeam)
                    {
                        plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerhurt"), "player.onplayerhurt.friendlyfire", variables);
                        return;
                    }
                }
            }

            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerhurt"), "player.onplayerhurt", variables);
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            /// <summary>  
            /// This is called before the player is about to die. Be sure to check if player is SCP106 (classID 3) and if so, set spawnRagdoll to false.
            /// In case the killer can't be passed, attacker will be null, so check for that before doing something.
            /// </summary> 

            if (ev.Player == null)
            {
                return;
            }

            if(ev.Killer == null)
            {
                Dictionary<string, string> noKillerVar = new Dictionary<string, string>
                {
                    { "spawnragdoll",       ev.SpawnRagdoll.ToString()          },
                    { "playeripaddress",    ev.Player.IpAddress                 },
                    { "playername",         ev.Player.Name                      },
                    { "playerplayerid",     ev.Player.PlayerId.ToString()       },
                    { "playersteamid",      ev.Player.SteamId                   },
                    { "playerclass",        ev.Player.TeamRole.Role.ToString()  },
                    { "playerteam",         ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerdie"), "player.onplayerdie.nokiller", noKillerVar);
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
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

            if (ev.Player.SteamId != ev.Killer.SteamId)
            {
                foreach(int friendlyTeam in teamKillingMatrix[(int)ev.Killer.TeamRole.Team])
                {
                    if((int)ev.Player.TeamRole.Team == friendlyTeam)
                    {
                        plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerdie"), "player.onplayerdie.friendlyfire", variables);
                        return;
                    }
                }
            }

            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerdie"), "player.onplayerdie", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerpickupitem"), "player.onplayerpickupitem", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerdropitem"), "player.onplayerdropitem", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerjoin"), "player.onplayerjoin", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onnicknameset"), "player.onnicknameset", variables);
        }

        public void OnAssignTeam(PlayerInitialAssignTeamEvent ev)
        {
            /// <summary>  
            /// Called when a team is picked for a player. Nothing is assigned to the player, but you can change what team the player will spawn as.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Team.ToString()                  }
            };
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onassignteam"), "player.onassignteam", variables);
        }

        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            /// <summary>  
            /// Called after the player is set a class, at any point in the game. 
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onsetrole"), "player.onsetrole", variables);
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
                plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_oncheckescape"), "player.oncheckescape", variables);
            }
            else
            {
                plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_oncheckescape"), "player.oncheckescape.denied", variables);
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

            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onspawn"), "player.onspawn", variables);
        }

        public void OnDoorAccess(PlayerDoorAccessEvent ev)
        {
            /// <summary>  
            /// Called when a player attempts to access a door that requires perms
            /// <summary> 
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
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
                plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_ondooraccess"), "player.ondooraccess", variables);
            }
            else
            {
                plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_ondooraccess"), "player.ondooraccess.notallowed", variables);
            }
        }

        public void OnIntercom(PlayerIntercomEvent ev)
        {
            /// <summary>  
            /// Called when a player attempts to use intercom.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "allowspeech",    ev.AllowSpeech.ToString()           },
                { "cooldowntime",   ev.CooldownTime.ToString()          },
                { "speechtime",     ev.SpeechTime.ToString()            },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };

            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onintercom"), "player.onintercom", variables);
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

            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onintercomcooldowncheck"), "player.onintercomcooldowncheck", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onpocketdimensionexit"), "player.onpocketdimensionexit", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onpocketdimensionenter"), "player.onpocketdimensionenter", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onpocketdimensiondie"), "player.onpocketdimensiondie", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onthrowgrenade"), "player.onthrowgrenade", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onplayerinfected"), "player.onplayerinfected", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onspawnragdoll"), "player.onspawnragdoll", variables);
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

            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_onlure"), "player.onplayerinfected.", variables);
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
            plugin.SendDiscordMessage(plugin.GetConfigString("discord_channel_oncontain106"), "player.oncontain106", variables);
        }
    }
}

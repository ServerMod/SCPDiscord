# Bot commands

## Time

Time is expressed in the format NumberUnit where unit is a unit of time and number is the amount of that time unit, for example 6M represents six months.

Valid time units:

|  Letter   |   Unit    |
|:---------:|:---------:|
|    `s`    |  Seconds  |
|    `m`    |  Minutes  |
|    `h`    |   Hours   |
|    `d`    |   Days    |
|    `w`    |   Weeks   |
|    `M`    |  Months   |
|    `y`    |   Years   |

---



### Ban

**Command usage:**

`ban <steamid> <duration> (reason)` - Bans a player from the server.

**Variables:**

* `<steamid>` - The SteamID of the player to be banned.

* `<duration>` - The duration of the ban, time formatting info above.

* `(reason)` - Optional reason for the ban.

Example: `+ban 76561138022363616 4d Bad man.` Bans the player for four days with the reason "Bad man."

---

### Unban

**Command usage:**

`unban <steamid/ip>` - Unbans a player from the server.

**Variables:**

* `<steamid/ip>` - The SteamID or IP of the player to be unbanned.

---

### Kick

**Command usage:**

`kick <steamid> (reason)` - Kicks a player from the server.

**Variables:**

* `<steamid>` - The SteamID of the player to be kicked.
* `(reason)` - Optional reason for the kick.

---

### Kickall

**Command usage:**

`kickall (reason)` - Kicks all players from the server with a message, useful for server shutdowns.

**Variables:**

* `(reason)` - Reason to be displayed to all players kicked.

---

### List

**Command usage:**

`list` - Lists all online players.

---

### Syncsteamid

**Command usage:**

`syncsteamid <steamid>` - Syncs your discord role to the game.

**Variables:**

* `<steamid>` - The SteamID of your account.

---

### Syncip

**Command usage:**

`syncip <ip>` - Syncs your discord role to the game. (For servers not using steam)

**Variables:**

* `<ip>` - Your ip address.

---
### Unsyncrole

**Command usage:**

`unsyncrole` - Removes the steam account from being synced with your discord account.

---


### Any other console commands

**Command usage:**

Enter the console command starting with your command prefix and `server`.

Example with command prefix `+`: `+server roundrestart`.

---

# Plugin console commands

### Reload

**Command usage:**

`scpd_reload` - Reloads the plugin, all configs and files and reconnects.

---

### Reconnect

**Command usage:**

`scpd_reconnect, scpd_rc` - Reconnects to the bot.

---

### Unsync

**Command usage:**

`scpd_unsync <discordid>` - Manually remove a player from being synced to discord.

**Variables:**

* `<discordid>` - The Discord user ID of the player to be removed.

---

### GrantVanillaRank

**Command usage:**

`scpd_grantvanillarank/scpd_gvr <steamid/playerid> <rank>` - Gives a player a vanilla rank for their current session.

**Variables:**

* `<steamid/playerid>` - Either the steamid or playerid of the player to grant a rank.
* `<rank>` - The name of the rank as defined in the vanilla config.

---

### GrantReservedSlot

**Command usage:**

`scpd_grantreservedslot/scpd_grs <steamid>` - Gives a player a reserved slot on the server.

**Variables:**

* `<steamid>` - The steamid of the player to add.

---

### RemoveReservedSlot

**Command usage:**

`scpd_removereservedslot/scpd_rrs <steamid>` - Removes a reserved slot from a player.

**Variables:**

* `<steamid>` - The steamid of the player to be removed.

---

### Validate

**Command usage:**

`scpd_validate` - Creates a config and language validation report in the console.

---

### SetNickname

**Command usage:**

`scpd_setnickname <player id/steamid> <nickname>` - Sets a player's nickname, useful for the rolesync system if you want to sync discord names.

---

# Permissions

(These only apply when using server commands via remote admin, not through discord)

| Permission                      | Description                                                                      |
|---------------------------------|----------------------------------------------------------------------------------|
| `scpdiscord.reload`             | Allows a player to reload the plugin.                                            |
| `scpdiscord.reconnect`          | Allows a player to reconnect the plugin to the bot.                              |
| `scpdiscord.unsync`             | Allows a player to unsync other players from discord.                            |
| `scpdiscord.verbose`            | Allows a player to toggle the verbose setting.                                   |
| `scpdiscord.debug`              | Allows a player to toggle the debug setting.                                     |
| `scpdiscord.grantreservedslot`  | Allows a player to grant reserved slots.                                         |
| `scpdiscord.removereservedslot` | Allows a player to remove reserved slots.                                        |
| `scpdiscord.grantvanillarank`   | Allows a player to grant vanilla ranks.                                          |
| `scpdiscord.validate`           | Allows a player to print a config and language validation report in the console. |
| `scpdiscord.setnickname`        | Allows a player to set someones nickname.                                        |
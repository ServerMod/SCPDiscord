# SCPDiscord

Ever wished to interact with your SCP:SL server without having to open your SSH client?

SCPDiscord links features from your server console to Discord channels, such as printing out server events to Discord and using server commands through Discord. It is incredibly customisable as you can read more about below.

I was going to wait with making this plugin until SCP:SL updates to .NET 4.6 from the current .NET 3.5 but apperently that has been delayed (indefinitely?). This means I can't use a C# Discord API directly from the plugin and thus have to seperate it into two different programs.

They send messages along like this: SCPDiscordPlugin -> SCPDiscordBot -> Discord Channel

## Current features:

*Ability to log any ServerMod event to Discord. All are optional and can be toggled individually.

Complete multi-channel support letting you post different events to different channels. All can be individually re-routed.
Support for MultiAdmin managed servers, each sub-server can post to the same channel, different channels or even different bots and different Discord servers.

You can for instance have one public channel for each of your servers where things like player joins, kills, round starts and round ends are posted. You could then add one channel for each server visible only to moderators, showing things like admin actions and logging who attacks who on each server to check for teamkillers.

Ability to use different commands through Discord, currently kick, ban and unban.
To my knowledge this is the only plugin that currently offers banning offline players, so hackers leaving as soon as they see an admin coming on is no longer an issue. It is also the only plugin I'm aware of that lets you unban players.

The ability to completely customise every single message from the plugin and use different languages. More info [here](https://github.com/KarlOfDuty/SCPDiscord/wiki/Adding-a-language-or-switching-language).

## Upcoming features

This plugin will in the future let you sync information such as how many players are online both to bot status and channel topic.

It will also be possible to sync ranks from discord to the game, letting you automate things like patreon rewards and moderator positions. 

It will also be possible to list the number of players on your server in the bot's status and a more detailed status in a channel's topic field.

## Bot commands

`setavatar <url>/<path>` - Sets the avatar picture for your bot.

Variables: 

* `<url>/<path>` - Either a local path or url to an image to set as your bot's avatar.

---
`ban <steamid> <duration> (reason)` - Bans a player from the server. (Ban permission in Discord required)

Variables: 

* `<steamid>` - The SteamID of the player to be banned.

* `<duration>` - The duration of the ban, time formatting info below.

* `(reason)` - Optional reason for the ban.

Example: `+ban 76561138022363616 4d Bad man.` Bans the player for four days with the reason "Bad man."

---

`unban <steamid/ip>` - Unbans a player from the server. (Ban permission in Discord required)

Variables: 

* `<steamid/ip>` - The SteamID or IP of the player to be unbanned.

---

`kick <steamid>` - Kicks a player from the server. (Kick permission in Discord required)

Variables: 

* `<steamid>` - The SteamID of the player to be kicked.

---
**Time**

Time is expressed in the format NumberUnit where unit is a unit of time and number is the amount of that time unit, for example 6M represents six months.

Valid time units:

* `s` - Seconds

* `m` - Minutes

* `h` - Hours

* `d` - Days

* `w` - Weeks

* `M` - Months

* `y` - Years

## Installation and configuration

Check out the [Wiki tab](https://github.com/KarlOfDuty/SCPDiscord/wiki) for all guides on installation and configs.

# SCPDiscord

Ever wished to interact with your SCP:SL server without having to open your ssh client? ~~Now~~ *Soon* you can, this plugin will let you execute commands remotely from Discord, sync information such as how many players are online, send messages from the server with info such as teamkilling, players joining and log admin actions.

It will also be possible to sync ranks from discord to the game, letting you automate things like patreon rewards and moderator positions. 

It will also be possible to list the number of players on your server in the bot's status and a more detailed status in a channel's topic field.

I was going to wait with making this plugin until SCP:SL updates to .NET 4.6 from the current .NET 3.5 but apperently that has been delayed (indefinitely?). This means I can't use a C# Discord API directly from the plugin and thus have to seperate it into two different programs.

They send messages along like this: SCPDiscordPlugin -> SCPDiscordBot -> Discord Channel

## Current features:

You can log any event from the server to discord with multi-channel support.

You can for instance have one public channel for each of your servers where things like player joins, kills round starts and round ends are posted. You could then add one channel for each server visible only to moderators, showing things like admin actions and logging who attacks who on each server to check for teamkillers.

## Bot commands

`setavatar <url>/<path>` - Sets the avatar picture for your bot.

Variables: 

* `<url>/<path>` - Either a local path or url to an image to set as your bot's avatar.

---
`ban <steamid> <duration> (reason)` - Bans a player from the server.

Variables: 

* `<steamid>` - The SteamID of the player to be banned.

* `<duration>` - The duration of the ban, time formatting info below.

* `(reason)` - Optional reason for the ban.

Example: `+ban 76561138022363616 4d Bad man.` Bans the player for four days with the reason "Bad man."

---

`kick <steamid>` - Kicks a player from the server.

Variables: 

* `<steamid>` - The SteamID of the player to be kicked.

---

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

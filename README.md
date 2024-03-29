# SCPDiscord [![Build Status](https://jenkins.karlofduty.com/job/ServerMod/job/SCPDiscord/job/master/badge/icon)](https://jenkins.karlofduty.com/blue/organizations/jenkins/ServerMod%2FSCPDiscord/activity) [![Release](https://img.shields.io/github/release/ServerMod/SCPDiscord.svg)](https://github.com/ServerMod/SCPDiscord/releases) [![Downloads](https://img.shields.io/github/downloads/ServerMod/SCPDiscord/total.svg)](https://github.com/ServerMod/SCPDiscord/releases) [![Discord Server](https://img.shields.io/discord/430468637183442945.svg?label=discord)](https://discord.gg/C5qMvkj) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/8144e5bff03c4912b08fd189b4b7f668)](https://www.codacy.com/manual/xkaess22/SCPDiscord?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=ServerMod/SCPDiscord&amp;utm_campaign=Badge_Grade)

### This is the old Smod version, looking for the new NWAPI plugin? It's available [here](https://github.com/KarlOfDuty/SCPDiscord)

SCPDiscord links features from your SCP:SL server console to Discord channels, such as printing out server events to Discord and using server commands through Discord. It is incredibly customisable as you can read more about below.

## Features:

* Ability to log any ServerMod event to Discord. All are optional and can be toggled individually.

* Complete multi-channel support letting you post different events to different channels. All can be individually re-routed.

* Support for MultiAdmin managed servers, each sub-server can post to the same channel, different channels or even different bots and different Discord servers.

* You can for instance have one public channel for each of your servers where things like player joins, kills, round starts and round ends are posted. 
You could then add one channel for each server visible only to moderators, showing things like admin actions and logging who attacks who on each server to check for teamkillers.

* Ability to use different commands through Discord, currently kick, ban and unban. To my knowledge this is the only plugin that currently offers banning offline players, so hackers leaving as soon as they see an admin coming on is no longer an issue. 
It is also the only plugin I'm aware of that lets you unban players.

* The ability to completely customise every single message from the plugin and use different languages. More info [here](docs/Languages.md).

* Player count is displayed in the bot activity field. The bot's status changes from dnd when the scp server is off but the bot server is on, away when there are no players on a server and online when a server has players.

* You can sync ranks from discord to the game, letting you automate things like supporter rewards and moderator positions.

## Guides

- [Installation](docs/Installation.md)

- [Editing messages and languages](docs/Languages.md)

- [Commands and permissions](docs/CommandsAndPermissions.md)

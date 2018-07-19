# SCPDiscord

Ever wished to interact with your SCP:SL server without having to open your ssh client? ~~Now~~ *Soon* you can, this plugin will let you execute commands remotely from Discord, sync information such as how many players are online, send messages from the server with info such as teamkilling, players joining and log admin actions.

It will also be possible to sync ranks from discord to the game, letting you automate things like patreon rewards and moderator positions. 

It will also be possible to list the number of players on your server in the bot's status and a more detailed status in a channel's topic field.

I was going to wait with making this plugin until SCP:SL updates to .NET 4.6 from the current .NET 3.5 but apperently that has been delayed (indefinitely?). This means I can't use a C# Discord API directly from the plugin and thus have to seperate it into two different programs.

## Current features:

You can log any event from the server to discord with multi-channel support.

You can for instance have one public channel for each of your servers where things like player joins, kills round starts and round ends are posted. You could then add one channel for each server visible only to moderators, showing things like admin actions and logging who attacks who on each server to check for teamkillers.

## Bot commands

`setavatar <url>/<path>` Either a path or url to an image to set as your bot's avatar.

## Installation:

**Bot:**

1. Install Node: https://nodejs.org/en/download/package-manager

2. Download the SCPDiscordBot archive and extract them anywhere you wish outside of the server directory.

3. Follow the bot config guide below.

4. Run start.bat or start.sh to start the bot. Alternatively use `node .` from commandline while in that directory. The start.sh script starts the server in a screen so you can simply press `ctrl-A` `ctrl-d` to have it run in the background. For more info google `linux screen`.

**Plugin:**

Put SCPDiscord.dll in your sm_plugins directory and follow the config instructions below.

## Bot config guide:

First of all you need to create a bot and add it to your server from here: https://discordapp.com/developers/ 

This is very simple, and you can follow this guide: https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token

If you have downloaded it directly from the repo rather than from the releases tab you have to rename the config from `default_config.json` to `config.json`. This is to make sure I or anyone else who adds to this bot don't accidentially post their bot token or other sensitive information in the public repository.

The different options are described in the config:

```json
{
  "_description": "The bot token from https://discordapp.com/developers/",
  "token": "add-your-token-here",

  "_description": "Prefix for discord commands.",
  "prefix": "+",

  "_description": "Port to listen on.",
  "port": 8888,

  "_description": "Channel to post generic plugin messages in. Everything set to 'default' in the plugin config goes to this channel",
  "genericMessagesChannel": "add-channel-id-here"
}
```

Get the channel ID by turning on developer mode in Discord and right click on the channel. You must add the bot token and channel id for the bot to work.

## Plugin config guide:

All options that are not included in the config act as set to off.

If you are running the bot script on the same server as this plugin you will not have to port forward or open firewall ports and you can leave those settings at the default.

Possible channel settings:

* Off - Event is not sent to Discord.
* Default - The event is sent to the default Discord channel designated designated in the Bot's config.
* `<channel-id>` - The event is sent to this channel, get the channel ID by turning on developer mode in Discord and right click on the channel.



Recommended config settings:

```yaml
# Connection settings
discord_bot_ip: 127.0.0.1
discord_bot_port: 8888

# Round events. Options: off, default or a channel id
discord_channel_onroundstart: default
discord_channel_onconnect: off
discord_channel_ondisconnect: default
discord_channel_oncheckroundend: off
discord_channel_onroundend: default
discord_channel_onwaitingforplayers: default
discord_channel_onroundrestart: off
discord_channel_onsetservername: off

# Environment events. Options: off, default or a channel id
discord_channel_onscp914activate: off
discord_channel_onstartcountdown: default
discord_channel_onstopcountdown: default
discord_channel_ondetonate: default
discord_channel_ondecontaminate: default

# Player events. Options: off, default or a channel id
discord_channel_onplayerdie: default
discord_channel_onplayerhurt: default
discord_channel_onplayerpickupitem: off
discord_channel_onplayerdropitem: off
discord_channel_onplayerjoin: default
discord_channel_onnicknameset: off
discord_channel_onassignteam: off
discord_channel_onsetrole: default
discord_channel_oncheckescape: default
discord_channel_onspawn: off
discord_channel_ondooraccess: off
discord_channel_onintercom: off
discord_channel_onintercomcooldowncheck: off
discord_channel_onpocketdimensionexit: default
discord_channel_onpocketdimensionenter: default
discord_channel_onpocketdimensiondie: default
discord_channel_onthrowgrenade: default

# Admin events. Options: off, default or a channel id
discord_channel_onadminquery: off
discord_channel_onauthcheck: off
discord_channel_onban: default

# Team events. Options: off, default or a channel id
discord_channel_ondecideteamrespawnqueue: off
discord_channel_onsetrolemaxhp: off
discord_channel_onteamrespawn: default
discord_channel_onsetscpconfig: off
```

You can find descriptions for each event here: https://github.com/Grover-c13/Smod2/tree/master/Smod2/Smod2/EventSystem/EventHandlers

All events are listed inside of these files.

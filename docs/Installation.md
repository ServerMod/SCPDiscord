# Important information, read this first

----

**If you have download the bot directly from the repo** rather than from the releases tab or dev builds you have to make a copy of `default_config.yml` and name it `config.yml`. 

This is to make sure I or anyone else who adds to this bot don't accidentally post their bot token or other sensitive information in the public repository.

----

**SECURITY WARNING:**

**The bot has no authorization of incoming connections**, this means you cannot allow the plugin's port through your firewall or anyone will be able to send fake messages to it.

If you really need to run the SCP:SL server on one system and the bot on another connected over the internet you can try this:

```bash
sudo ufw allow from 111.111.111.111 to any port 8888
```

This allows only the IP 111.111.111.111 to connect to the bot on port 8888 as long as your default setting is to deny all (which it is by default).

----

**SECURITY WARNING:**

If you ever reveal the bot token to anyone or post it somewhere where it can be read from the internet you need to reset it immediately.
It gives others full access to the bot's Discord account so they can use it to do whatever they want with your Discord server, including deleting it.

It is also highly recommended to make sure the bot only has the necessary Discord permissions and no more in order to limit the damage if you accidentally post your bot token somewhere.

----

# Installation

----

## 1. Download

Download the SCPDiscord archive, either a [release](https://github.com/KarlOfDuty/SCPDiscord/releases) or [dev build](https://jenkins.karlofduty.com/blue/organizations/jenkins/CI%2FSCPDiscord/activity/).

### **Bot:**

Extract the bot anywhere you wish outside of the server directory.

### **Plugin:**

Extract the plugin and dependencies directory into the `<server_dir>/sm_plugins` directory:
```
sm_plugins/
    dependencies/
        Google.Protobuf.dll
        Newtonsoft.Json.dll
        YamlDotNet.dll
    SCPDiscord.dll
```

----

## 2. Server config

This plugin does not use the server config for configuration like most others, instead it has it's own plugin config in your config directory. 

The only thing configured in the server config is where to place the plugin's files:

| Name (in config_gameplay.txt) | Default value | Description                                                                                              |
|-------------------------------|:-------------:|----------------------------------------------------------------------------------------------------------|
| `scpdiscord_config_global`    |    `true`     | Decides whether to place the plugin config in the global config directory instead of the local one.      |
| `scpdiscord_rolesync_global`  |    `true`     | Decides whether to place the rolesync data in the global config directory instead of the local one.      |
| `scpdiscord_languages_global` |    `true`     | Decides whether to place the language directory in the global config directory instead of the local one. |

Depending on how you have set up your server the local config directory should be in `<server_dir>/servers/servername/` for multiadmin servers or `~/config/SCP Secret Laboratory/config/<server_port>/` for localadmin servers and multiadmin servers if you do not create the local server directory.

The global config directory is most likely in `~/config/SCP Secret Laboratory` on linux or `%Appdata%/SCP Secret Laboratory` on windows.

The plugin config is automatically created for you the first time you run the plugin. The path is printed in the server console on startup so you know for sure where it is.

----

## 3. Plugin config

[Click here to view default config](https://github.com/KarlOfDuty/SCPDiscord/blob/master/SCPDiscordPlugin/config.yml)

Remember to set the bot port to something **different from the scpsl server port**, or everything will break.

Remember this port as you will need to put it in the bot config too.

**Note:** Keeping the bot and plugin on different devices is not supported but is possible, you will have to deal with the issues this may result in yourself if you choose to do so.
Simply change the bot ip in the config below to correspond with the other device.

----

## 4. Bot setup and config:

### Bot setup:

Set up your bot in the discord control panel according to the guide [here](CreateBot.md).

### Config setup:

[Click here to view default config](https://github.com/KarlOfDuty/SCPDiscord/blob/master/SCPDiscordBot/default_config.yml)

The different options are described in the config. Get Discord IDs by turning on developer mode in Discord and right clicking on a server, role or user.

Run the bot using:
```yaml
# Linux:
./SCPDiscordBot
# Windows
./SCPDiscordBot.exe
```

----

### If you have followed all the steps you should now have a working Discord bot, otherwise contact me in Discord.
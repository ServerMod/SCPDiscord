console.log("Config loading...");
const fs = require("fs");
const YAML = require("yaml");
const file = fs.readFileSync("./config.yml", "utf8");
const { token, prefix, listeningPort, statusChannels, commandChannels, verbose, cooldown, permissions, serverID, roleSync } = YAML.parse(file);
console.log("Config loaded.");

var connectedToDiscord = false;
const Discord = require("discord.js");
const discordClient = new Discord.Client({ autoReconnect: true });

var messageQueue = {};

var sockets = [];
var tcpServer = require("net").createServer();

// Discord functions
function setChannelTopic(channelID, topic)
{
    var verifiedChannel = discordClient.channels.get(channelID);
    if (verifiedChannel != null)
    {
        if (verifiedChannel.manageable)
        {
            if (verbose)
            {
                console.log("Changed to topic: " + topic);
            }
            verifiedChannel.setTopic(topic);
        }
        else if (verbose)
        {
            console.warn("No permission to change channel topic.");
        }
    }
    else if (verbose)
    {
        console.warn("Server status channel was not found.");
    }
}

function hasPermission(member, command)
{
    if (member.hasPermission("ADMINISTRATOR"))
    {
        return true;
    }

    var permissionRoles = Object.keys(permissions);
    var memberRoles = member.roles;
    for (var [roleID, role] of memberRoles)
    {
        if (permissionRoles.includes(role.name.toLowerCase()))
        {
            if (permissions[roleID].includes(command))
            {
                return true;
            }
        }
    }
    return false;
}

function syncRoleCommand(message, args)
{
    var output = "command " + message.channel.id + " syncrole ";
    if (args.length < 1)
    {
        message.channel.send("```diff\n- Missing arguments.```");
        return;
    }

    if (args[0].length !== 17 || isNaN(args[0]))
    {
        message.channel.send("```diff\n- Not a valid SteamID64.```");
        return;
    }
    output += args[0];
    output += " ";
    output += message.member.id;
    sockets.forEach((socket) =>
    {
        socket.write(output + "\n");
    });
}

function unsyncRoleCommand(message)
{
    sockets.forEach((socket) =>
    {
        socket.write("command " + message.channel.id + " unsyncrole " +  message.member.id + "\n");
    });
}

// Connection event
tcpServer.on("connection", (socket) =>
{
    sockets.push(socket);

    socket.setKeepAlive(true, 1000);

    socket.setEncoding("utf8");

    console.log("Plugin connected.");

    // Messages from the plugin
    socket.on("data", (data) =>
    {
        if (discordClient == null)
        {
            console.log("Recieved " + data + " but Discord client was null.");
            return;
        }

        if (!connectedToDiscord)
        {
            console.log("Recieved " + data + " but was not connected to Discord yet.");
            return;
        }

        var messages = data.split("\u0000");

        messages.forEach(function (packet)
        {
            console.log(packet);
            if (packet.slice(0, 12) === "channeltopic")
            {
                var channel = packet.slice(12, 30);
                setChannelTopic(channel, packet.slice(30));
            }
            else if (packet.slice(0, 11) === "botactivity" && discordClient.user != null)
            {
                if (packet.slice(11)[0] === "0")
                {
                    discordClient.user.setStatus("idle");
                }
                else
                {
                    discordClient.user.setStatus("available");
                }

                discordClient.user.setActivity(packet.slice(11),
                {
                    type: "PLAYING"
                });

                if (verbose)
                {
                    console.warn("Set activity to " + packet.slice(11));
                }
            }
            else if (packet.slice(0, 9) === "rolequery" && discordClient.user != null)
            {
                var words = packet.split(" ");
                var steamID = words[1];
                var discordID = words[2];
                var server = discordClient.guilds.get(serverID);
                var member = server.members.get(discordID);

                if (member != null && server != null)
                {
                    for (var key in roleSync)
                    {
                        if (member.roles.find(x => x.id === key) != null)
                        {
                            socket.write("roleresponse " + steamID + " " + roleSync[key]);
                            break;
                        }
                    }
                }
                else
                {
                    console.log("Tried to sync the role of " + words[1] + " but they were not found on the discord server.");
                }
            }
            else
            {
                var destinationChannel = packet.slice(0, 18);
                var message = packet.slice(18);
                if (message !== "")
                {
                    // If this channel has not been used yet it must be initialized
                    if (messageQueue[destinationChannel] == null)
                    {
                        messageQueue[destinationChannel] = message + "\n";
                    }
                    else
                    {
                        messageQueue[destinationChannel] += message + "\n";
                    }
                }
            }
        });
        for (var channelID in messageQueue)
        {
            var verifiedChannel = discordClient.channels.get(channelID);
            if (verifiedChannel != null)
            {
                //Message is copied to a new variable as it's deletion later may happen before the send function finishes
                var message = messageQueue[channelID].slice(0, -1);

                // If message is too long, split it up
                while (message.length >= 2000)
                {
                    var cutMessage = message.slice(0, 1999);
                    message = message.slice(1999);
                    if (cutMessage != null && cutMessage !== " " && cutMessage !== "")
                    {
                        if (discordClient.status)
                        {
                            verifiedChannel.send(cutMessage);
                            if (verbose)
                            {
                                console.log("Sent: " + channelID + ": '" + cutMessage + "' to Discord.");
                            }
                        }
                    }
                }

                // Send remaining message
                if (message != null && message !== " " && message !== "")
                {
                    verifiedChannel.send(message);
                    if (verbose)
                    {
                        console.log("Sent: " + channelID + ": '" + message + "' to Discord.");
                    }
                }
            }
            else
            {
                if (verbose)
                {
                    console.warn("Channel not found for message: " + messageQueue[channelID]);
                }
            }
            messageQueue[channelID] = "";
        }

        // Wait for the rate limit
        var waitTill = new Date(new Date().getTime() + cooldown);
        while (waitTill > new Date()) { } //eslint-disable-line
    });

    //Connection issues
    socket.on("error", (data) =>
    {
        if (verbose === true)
        {
            console.log("Socket error <" + data.message + ">");
        }
        socket.destroy();
    });

    socket.on("close", () =>
    {
        console.log("Plugin connection lost.");
        sockets.splice(sockets.indexOf(socket), 1);
        for (var i = 0; i < statusChannels.length; i++)
        {
            var verifiedChannel = discordClient.channels.get(statusChannels[i]);
            if (verifiedChannel != null)
            {
                verifiedChannel.send("```diff\n- SCP:SL server connection lost.```");
            }
            else if (verbose)
            {
                console.warn("Error sending status to Discord.");
            }
        }
        discordClient.user.setStatus("dnd");
        discordClient.user.setActivity("for server startup.",
        {
            type: "WATCHING"
        });
    });
});


discordClient.on("ready", () =>
{
    console.log("Discord connection established.");
    for (var i = 0; i < statusChannels.length; i++)
    {
        var verifiedChannel = discordClient.channels.get(statusChannels[i]);
        if (verifiedChannel != null)
        {
            verifiedChannel.send("```diff\n+ Bot Online.```");
        }
        else if (verbose)
        {
            console.warn("Error sending status to Discord.");
        }
    }
    discordClient.user.setStatus("dnd");
    discordClient.user.setActivity("for server startup.",
        {
            type: "WATCHING"
        });
    connectedToDiscord = true;
});

//Messages from Discord
discordClient.on("message", (message) =>
{
    //Abort if message does not start with the prefix, if the sender is a bot, if the message is not from the right channel or if it does not contain any letters
    if (!message.content.startsWith(prefix) || message.author.bot || !commandChannels.includes(message.channel.id) || message.content.length <= prefix.length)
    {
        return;
    }

    //Cut message into base command and arguments
    const args = message.content.slice(prefix.length).split(/ +/);
    const command = args.shift().toLowerCase();
    if (hasPermission(message.member, command))
    {
        //message.channel.send("```diff\n+ You are allowed to use this command```");
        if (sockets.length < 1)
        {
            message.channel.send("```diff\n- The SCP:SL server is not currently connected to the bot server, could not deliver command.```");
            return;
        }

        if (command === "syncrole")
        {
            syncRoleCommand(message, args);
        }
        else if (command === "unsyncrole")
        {
            unsyncRoleCommand(message, args);
        }
        else
        {
            sockets.forEach((socket) =>
            {
                console.log("RELAYED: command " + message.channel.id + " " + message.content.slice(prefix.length).replace("\\_", "_") + "\n");
                socket.write("command " + message.channel.id + " " + message.content.slice(prefix.length).replace("\\_", "_") + "\n");
            });
        }
    }
    else
    {
        message.channel.send("```diff\n- You are not allowed to use this command.```");
    }
});

discordClient.on("error", (e) =>
{
    if (e.message === "getaddrinfo ENOTFOUND gateway.discord.gg gateway.discord.gg:443")
    {
        connectedToDiscord = false;
        console.error("Discord connection broken, retrying...");
    }
    else
    {
        console.error(e.message);
    }
});

discordClient.on("warn", (e) =>
{
    if (verbose)
    {
        console.warn(e);
    }
});

console.log("Connecting to Discord...");
discordClient.login(token)
    .then(() =>
    {
        if (serverID != null && serverID !== "")
        {
            var roles = discordClient.guilds.get(serverID).roles;
            roles = roles.sort((a, b) => b.position - a.position || b.id - a.id);
            console.log("##################### Discord roles #####################");
            for (var [roleID, role] of roles)
            {
                console.log("# " + role.name.padEnd(35, ".") + roleID + " #");
            }
            console.log("#########################################################");
        }

    })
    .catch((e) =>
    {
        if (e.code === "ENOTFOUND")
        {
            console.error("ERROR: Connection to Discord could not be established. Are HTTP or HTTPS ports blocked (80 & 443)?");
        }
        else
        {
            console.error(e);
        }
    });

console.log("Binding TCP port...");
tcpServer.listen(listeningPort, () =>
{
    console.log("Server is listening on port " + listeningPort);
});


tcpServer.on("error", (e) =>
{
    if (e.code === "EADDRINUSE")
    {
        console.log("Error: Could not bind to port " + listeningPort + ", is it already in use?");
    }
    else
    {
        console.log(e);
    }
    process.exit(0);
});

// Runs when the server shuts down
function shutdown()
{
    sockets.forEach((socket) =>
    {
        socket.destroy();
    });

    tcpServer.close(() =>
    {
        console.log("TCP server closed.");
        tcpServer.unref();
    });

    if (connectedToDiscord)
    {
        // Look, I have no idea how js works, its fine
        console.log("Signing out of Discord...");
        discordClient.user.setStatus("dnd")
        .then(() =>
        {
            for (var i = 0; i < statusChannels.length; i++)
            {
                var verifiedChannel = discordClient.channels.get(statusChannels[i]);
                if (verifiedChannel != null)
                {
                    verifiedChannel.send("```diff\n- Bot shutting down...```");
                }
                else if (verbose)
                {
                    console.warn("Error sending status to Discord.");
                }
            }
        })
        .then(() => discordClient.user.setActivity("for server startup.", { type: "WATCHING" })
        .then(() => discordClient.destroy()));
    }
}
process.on("exit", () => shutdown());
process.on("SIGINT", () => shutdown());
process.on("SIGUSR1", () => shutdown());
process.on("SIGUSR2", () => shutdown());
process.on("SIGHUP", () => shutdown());

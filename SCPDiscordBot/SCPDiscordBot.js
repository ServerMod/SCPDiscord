console.log("Config loading...");
const fs = require("fs");
const YAML = require("yaml");
const file = fs.readFileSync("./config.yml", "utf8");
const { token, prefix, listeningPort, defaultChannel, verbose, cooldown, requirepermission } = YAML.parse(file);
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

// Connection event
tcpServer.on("connection", (socket) =>
{
    sockets.push(socket);

    socket.setKeepAlive(true, 1000);

    socket.setEncoding("utf8");

    console.log("Plugin connected.");

    // Messages from the plugin
    socket.on("data", data =>
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
                    console.warn("Channel not found for message: " + messageQueue);
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
            socket.destroy();
        }
    });

    socket.on("close", () =>
    {
        console.log("Plugin connection lost.");
        var verifiedChannel = discordClient.channels.get(defaultChannel);
        if (verifiedChannel != null)
        {
            verifiedChannel.send("```diff\n- SCP:SL server connection lost.```");
            discordClient.user.setStatus("dnd");
            discordClient.user.setActivity("for server startup.",
            {
                type: "WATCHING"
            });
        }
        else if (verbose)
        {
            console.warn("Error sending status to Discord.");
        }
        sockets.splice(sockets.indexOf(socket), 1);
    });

    socket.on("timeout", () =>
    {
        socket.destroy();
    });

});


discordClient.on("ready", () =>
{
    console.log("Discord connection established.");
    discordClient.channels.get(defaultChannel).send("```diff\n+ Bot Online.```");
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
    if (!message.content.startsWith(prefix) || message.author.bot || message.channel.id !== defaultChannel || !/[a-z]/i.test(message.content))
    {
        return;
    }

    //Cut message into base command and arguments
    const args = message.content.slice(prefix.length).split(/ +/);
    const command = args.shift().toLowerCase();

    //Add commands here, I only verify permissions and that the command exists here
    if (command === "setavatar" && (message.member.hasPermission("ADMINISTRATOR") || requirepermission === false))
    {
        var url = args.shift();
        discordClient.user.setAvatar(url);
        message.channel.send("```diff\n+ Avatar updated.```");
    }
    else if (command === "ban" && (message.member.hasPermission("BAN_MEMBERS") || requirepermission === false))
    {
        sockets.forEach((socket) =>
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        });

    }
    else if (command === "unban" && (message.member.hasPermission("BAN_MEMBERS") || requirepermission === false))
    {
        sockets.forEach((socket) =>
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        });
    }
    else if (command === "kick" && (message.member.hasPermission("KICK_MEMBERS") || requirepermission === false))
    {
        sockets.forEach((socket) =>
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        });
    }
    else if (command === "kickall" && (message.member.hasPermission("KICK_MEMBERS") || requirepermission === false))
    {
        sockets.forEach((socket) =>
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        });
    }
    else if ((command === "hidetag" || command === "showtag") && (message.member.hasPermission("MANAGE_NICKNAMES") || requirepermission === false))
    {
        sockets.forEach((socket) =>
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        });
    }
    else
    {
        if (message.member.hasPermission("ADMINISTRATOR") || requirepermission === false)
        {
            sockets.forEach((socket) =>
            {
                socket.write("command " + message.content.slice(prefix.length) + "\n");
            });
        }
        else
        {
            message.channel.send("```diff\n- You are not allowed to use this command.```");
        }
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
discordClient.login(token).then().catch((e) =>
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
        discordClient.channels.get(defaultChannel).send("```diff\n- Bot shutting down...```");
        console.log("Signing out of Discord...");
        discordClient.user.setStatus("dnd");
        discordClient.user.setActivity("for server startup.",
        {
            type: "WATCHING"
        });
    }
    discordClient.destroy();
}
process.on("exit", () => shutdown());
process.on("SIGINT", () => shutdown());
process.on("SIGUSR1", () => shutdown());
process.on("SIGUSR2", () => shutdown());
process.on("SIGHUP", () => shutdown());

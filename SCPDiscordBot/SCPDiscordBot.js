console.log('Config loading...');

const { token, prefix, listeningPort, defaultChannel, verbose, cooldown, requirepermission } = require('./config.json');

console.log('Config loaded.');

const Discord = require('discord.js');

const client = new Discord.Client({ autoReconnect: true });

var messageQueue = JSON.parse('{}');

console.log('Binding TCP port...');
var listenServer = require('net');
listenServer.createServer(function (socket)
{
    socket.setEncoding("utf8");

    console.log('Plugin connected.');

    // Messages from the plugin
    socket.on('data', function (data)
    {
		console.log("Recieved " + data + ".");
		socket.write("waddup\n");
        if (client == null)
        {
            console.log("Recieved " + data + " but Discord client was null.");
            return;
        }
        var messages = data.split('\u0000');

        messages.forEach(function (packet)
        {
            if (packet.slice(0, 12) === "channeltopic")
            {
                var channel = packet.slice(12, 30);
                if (channel === "000000000000000000")
                {
                    channel = defaultChannel;
                }

                // Try finding the channel
                var verifiedChannel = client.channels.get(channel);
                if (verifiedChannel != null)
                {
                    if (verifiedChannel.manageable)
                    {
                        if (verbose)
                            console.log("Changed to topic: " + packet.slice(30));
                        verifiedChannel.setTopic(packet.slice(30));
                    }
                    else
                    {
                        if(verbose)
                            console.warn("No permission to change channel topic.");
                    }
                }
                else
                {
                    if(verbose)
                        console.warn("Server status channel was not found.");
                }
            }
            else if (packet.slice(0, 11) === "botactivity" && client.user != null)
            {
                if (packet.slice(11)[0] === "0")
                {
                    client.user.setStatus('idle');
                }
                else
                {
                    client.user.setStatus('available');
                }
                client.user.setActivity(packet.slice(11),
                {
                    type: "PLAYING"
                });
                if (verbose)
                    console.warn("Set activity to " + packet.slice(11));
            }
            else
            {
                var destinationChannel = packet.slice(0, 18);
                var message = packet.slice(18);
                if (message !== "")
                {
                    //Switch the default channel key for the actual default channel id
                    if (destinationChannel === "000000000000000000")
                    {
                        destinationChannel = defaultChannel;
                    }

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
            var verifiedChannel = client.channels.get(channelID);
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
                        if (client.status)
                        verifiedChannel.send(cutMessage);
                        if (verbose)
                        {
                            console.log("Sent: " + channelID + ": '" + cutMessage + "' to Discord.");
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
    socket.on('error', function (data)
    {
        if (data.message === "read ECONNRESET")
        {
            console.log('Plugin connection lost.');
            var verifiedChannel = client.channels.get(defaultChannel);
            if (verifiedChannel != null)
            {
                verifiedChannel.send("Plugin connection lost.");
                client.user.setStatus('dnd');
                client.user.setActivity("for server startup.",
                {
                    type: "WATCHING"
                });
            }
            else
            {
                if (verbose)
                    console.warn("Error sending status to Discord.");
            }
        }
        else if (verbose === true)
        {
            console.log("Socket error <" + data.message + ">");
        }
    });

    //Messages from Discord
    client.on('message', message =>
    {
        //Abort if message does not start with the prefix, if the sender is a bot, if the message is not from the right channel or if it does not contain any letters
        if (!message.content.startsWith(prefix) || message.author.bot || message.channel.id !== defaultChannel || !/[a-z]/i.test(message.content))
            return;

        console.log("Command recieved.");
        //Cut message into base command and arguments
        const args = message.content.slice(prefix.length).split(/ +/);
        const command = args.shift().toLowerCase();

        //Add commands here, I only verify permissions and that the command exists here
        if (command === 'setavatar' && (message.member.hasPermission("ADMINISTRATOR") || requirepermission === false))
        {
            var url = args.shift();
            client.user.setAvatar(url);
            message.channel.send('Avatar Updated.');
        }
        else if (command === 'ban' && (message.member.hasPermission("BAN_MEMBERS") || requirepermission === false))
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        }
        else if (command === 'unban' && (message.member.hasPermission("BAN_MEMBERS") || requirepermission === false))
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        }
        else if (command === 'kick' && (message.member.hasPermission("KICK_MEMBERS") || requirepermission === false))
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        }
        else
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        }
    });

    client.on("error", (e) =>
    {
        console.error(e);
    });

    client.on("warn", (e) =>
    {
        if (verbose)
        {
            console.warn(e);
        }
    });

    client.on("debug", (e) =>
    {
        if (verbose)
        {
            console.info(e);
        }
    });
}).listen(listeningPort);
{
    console.log('Server is listening on port ' + listeningPort);
}

console.log('Connecting to Discord...');
client.on('ready', () =>
{
    console.log('Discord connection established.');
    client.channels.get(defaultChannel).send("Bot Online.");
    client.user.setStatus('dnd');
    client.user.setActivity("for server startup.",
    {
        type: "WATCHING"
    });
});

process.on('exit', function ()
{
    client.send("Bot shutting down...");
    console.log('Signing out...');
    if (client != null)
    {
        client.destroy();
    }
});
process.on('SIGINT', function ()
{
    client.send("Bot shutting down...");
    console.log('Signing out...');
    if (client != null)
    {
        client.destroy();
    }
});

process.on('SIGUSR1', function ()
{
    client.send("Bot shutting down...");
    console.log('Signing out...');
    if (client != null)
    {
        client.destroy();
    }
});

process.on('SIGUSR2', function ()
{
    client.send("Bot shutting down...");
    console.log('Signing out...');
    if (client != null)
    {
        client.destroy();
    }
});

process.on('SIGHUP', function ()
{
    client.send("Bot shutting down...");
    console.log('Signing out...');
    if (client != null)
    {
        client.destroy();
    }
});
client.login(token);
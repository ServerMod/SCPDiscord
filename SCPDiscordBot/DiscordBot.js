console.log('Config loading...');

const { token, prefix, listeningPort, defaultChannel, verbose, cooldown } = require('./config.json');

console.log('Config loaded.');

const Discord = require('discord.js');

const client = new Discord.Client();

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
        var messages = data.split('\u0000')
        messages.forEach(function (packet)
        {
            var destinationChannel = packet.slice(0, 18)
            var message = packet.slice(18)
            if (message != "")
            {
                //Switch the default channel key for the actual default channel id
                if (destinationChannel === "000000000000000000")
                {
                    destinationChannel = defaultChannel;
                }

                // If this channel has not been used yet it must be initialized
                if (messageQueue[destinationChannel] == null)
                {
                    messageQueue[destinationChannel] = (message + "\n");
                }
                else
                {
                    messageQueue[destinationChannel] += (message + "\n");
                }

            }
        });
        for (var channelID in messageQueue)
        {
            if (client !== null)
            {
                var verifiedChannel = client.channels.get(channelID);
                if (verifiedChannel != null)
                {
                    //Message is copied to a new variable as it's deletion later may happen before the send function finishes
                    var message = messageQueue[channelID].slice(0, -1);
                    if (message != null && message != " " && message != "")
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
                        console.log("Channel not found for message: " + messageQueue);
                    }
                }
                messageQueue[channelID] = "";
            }
        }

        // Wait for the rate limit
        var waitTill = new Date(new Date().getTime() + cooldown);
        while (waitTill > new Date()) { }
    });

    //Connection issues
    socket.on('error', function (data)
    {
        console.log('Plugin connection lost.');
        var verifiedChannel = client.channels.get(defaultChannel);
        if (verifiedChannel != null)
        {
            verifiedChannel.send("Plugin connection lost.");
        }
    });

    //Messages from Discord
    client.on('message', message =>
    {
        //Abort if message does not start with the prefix
        if (!message.content.startsWith(prefix) || message.author.bot || message.channel.id != defaultChannel)
            return;

        //Cut message into base command and arguments
        const args = message.content.slice(prefix.length).split(/ +/);
        const command = args.shift().toLowerCase();

        //Add commands here, I only verify permissions and that the command exists here
        if (command === 'setavatar' && message.member.hasPermission("ADMINISTRATOR"))
        {
            var url = args.shift();
            client.user.setAvatar(url);
            message.channel.send('Avatar Updated.');
        }
        else if (command === 'test' && message.member.hasPermission("ADMINISTRATOR"))
        {
            socket.write(message.member.displayName + " used the command 'test'. If you can read this it means everything works as it should.\n");
            console.log("Forwarded test message to plugin.");
            message.channel.send('Check your SCP server console for confirmation.');
        }
        else if (command === 'ban' && message.member.hasPermission("BAN_MEMBERS"))
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        }
        else if (command === 'unban' && message.member.hasPermission("BAN_MEMBERS"))
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        }
        else if (command === 'kick' && message.member.hasPermission("KICK_MEMBERS"))
        {
            socket.write("command " + message.content.slice(prefix.length) + "\n");
        }
        else
        {
            message.channel.send('Invalid SCPDiscord command, or you do not have permission to use it.');
        }
    });

    client.on("error", (e) =>
    {
        console.error(e)
    });

    client.on("warn", (e) =>
    {
        console.warn(e)
    });

    client.on("debug", (e) =>
    {
        if (verbose)
        {
            console.info(e)
        }
    });
}).listen(listeningPort)
{
    console.log('Server is listening on port ' + listeningPort);
}

console.log('Connecting to Discord...');
client.on('ready', () =>
{
    console.log('Discord connection established.');
    client.channels.get(defaultChannel).send("Bot Online.");
});

client.login(token);
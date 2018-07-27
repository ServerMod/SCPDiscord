console.log('Config loading...');

const { token, prefix, listeningPort, genericMessagesChannel } = require('./config.json');

console.log('Config loaded.');

const Discord = require('discord.js');

const client = new Discord.Client();

var messageQueue = JSON.parse('{}');

console.log('Binding TCP port...');
var listenSocket = require('net');
listenSocket.createServer(function (socket)
{
    socket.setEncoding("utf8");
    // Handle incoming messages
    socket.on('connection', function ()
    {
        console.log('Plugin connected.');
    })

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
                    destinationChannel = genericMessagesChannel;
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
                    var message = messageQueue[channelID];
                    verifiedChannel.send(message);
                    console.log("Sent: " + channelID + ": '" + message + "' to Discord.");
                }
                else
                {
                    console.log("Channel not found for message: " + messageQueue);
                }
                messageQueue[channelID] = "";
            }
        }
    });
    //Parsing comands
    client.on('message', message =>
    {
        //Abort if message does not start with the prefix
        if (!message.content.startsWith(prefix) || message.author.bot || message.channel.id != genericMessagesChannel) return;

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
            socket.write(message.member.displayName + " used the command 'test'. If you can read this it means everything works as it should.");
            message.channel.send('Check your SCP server console for confirmation.');
        }
        else if (command === 'ban' && message.member.hasPermission("BAN_MEMBERS"))
        {
            socket.write("command " + message.content.slice(prefix.length) + "\r\n");
        }
        else if (command === 'kick' && message.member.hasPermission("KICK_MEMBERS"))
        {
            socket.write("command " + message.content.slice(prefix.length) + "\r\n");
        }
        else
        {
            message.channel.send('Invalid SCPDiscord command, or you do not have permission to use it.');
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
    client.channels.get(genericMessagesChannel).send("Bot Online.");
});


client.login(token);
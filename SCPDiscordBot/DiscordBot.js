console.log('Config loading...');

const { token, prefix, port, genericMessagesChannel } = require('./config.json');

console.log('Config loaded.');

const Discord = require('discord.js');

const client = new Discord.Client();

var messageQueue = [];

console.log('Binding TCP port...');
var net = require('net');
net.createServer(function (socket)
{
    socket.setEncoding("utf8");
    // Handle incoming messages
    socket.on('data', function (data)
    {
        var messages = data.split('\n')
        messages.forEach(function (element)
        {
            var destinationChannel = element.slice(0, 18)
            if (destinationChannel === "000000000000000000")
            {
                destinationChannel = genericMessagesChannel;
            }
            var message = element.slice(18)
            if (client !== null && message != "")
            {
                client.channels.get(destinationChannel).send(message);
                console.log(message);
            }
        });
    });
}).listen(port)
{
    console.log('Server is listening on port ' + port);
}

console.log('Connecting to Discord...');

client.on('ready', () =>
{
    console.log('Discord connection established.');
    client.channels.get(genericMessagesChannel).send("Bot Online.");
});

//Parsing comands
client.on('message', message =>
{
    //Abort if message does not start with the prefix
    if (!message.content.startsWith(prefix) || message.author.bot) return;

    //Cut message into base command and arguments
    const args = message.content.slice(prefix.length).split(/ +/);
    const command = args.shift().toLowerCase();
    if (command === 'setavatar' && message.member.hasPermission("ADMINISTRATOR"))
    {
        var url = args.shift();
        client.user.setAvatar(url);
        message.channel.send('Avatar Updated.');
    }
});
client.login(token);
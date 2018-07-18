console.log('Config loading...');

const { token, prefix, avatarURL, port, channelID } = require('./config.json');

console.log('Config loaded.');

const Discord = require('discord.js');

const client = new Discord.Client();

console.log('Binding TCP port...');
var net = require('net');
net.createServer(function (socket)
{
    socket.setEncoding("utf8");
    // Handle incoming messages
    socket.on('data', function (data)
    {
        console.log(data);
        if (client != null)
        {
            client.channels.get(channelID).send(data);
        }
    });
}).listen(port)
{
    console.log('Server is listening on port ' + port);
}

console.log('Connecting to Discord...');

client.on('ready', () =>
{
    console.log('Discord connection established.');
    //client.user.setAvatar(avatarURL);
    client.channels.get(channelID).send("Bot Online.");
});

//Parsing comands
client.on('message', message =>
{
    //Abort if message does not start with the prefix
    if (!message.content.startsWith(prefix) || message.author.bot) return;

    //Cut message into base command and arguments
    const args = message.content.slice(prefix.length).split(/ +/);
    const command = args.shift().toLowerCase();
    if (command === 'test')
    {
        message.channel.send('Test');
    }
});
client.login(token);
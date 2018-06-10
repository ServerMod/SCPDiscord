using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;

using DSharpPlus;
using System.Threading.Tasks;

namespace SCPDiscord
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "SCPDiscord",
        description = "Plugin which outputs server events to Discord.",
        id = "karlofduty.scpdiscord",
        version = "3.0",
        SmodMajor = 3,
        SmodMinor = 0,
        SmodRevision = 0
        )]
    internal class ExamplePlugin : Plugin
    {
        private static DiscordClient discord;

        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            this.Info("SCPDiscord enabled.");
        }

        static public async Task TestFunction()
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "token",
                TokenType = TokenType.Bot
            });

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().StartsWith("!ping"))
                    await e.Message.RespondAsync("pong!");
            };

            await discord.ConnectAsync();
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public override void Register()
        {
        }
    }
}
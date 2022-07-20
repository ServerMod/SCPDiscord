using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class ServerCommand : BaseCommandModule
	{
		[Command("server")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, [RemainingText] string serverCommand = "")
		{
			if (!ConfigParser.IsCommandChannel(command.Channel.Id)) return;
			if (!ConfigParser.ValidatePermission(command)) return;

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				ConsoleCommand = new Interface.ConsoleCommand
				{
					ChannelID = command.Channel.Id,
					DiscordID = command.Member?.Id ?? 0,
					Command = serverCommand
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending '" + command.Message.Content + "' to plugin from " + command.Member?.Username + "#" + command.Member?.Discriminator, LogID.DISCORD);
		}
	}
}

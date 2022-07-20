using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class KickAllCommand : BaseCommandModule
	{
		[Command("kickall")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, [RemainingText] string kickReason = "")
		{
			if (!ConfigParser.IsCommandChannel(command.Channel.Id)) return;
			if (!ConfigParser.ValidatePermission(command)) return;

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				KickallCommand = new Interface.KickallCommand
				{
					ChannelID = command.Channel.Id,
					AdminTag = command.Member?.Username + "#" + command.Member?.Discriminator,
					Reason = kickReason
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending '" + command.Message.Content + "' to plugin from " + command.Member?.Username + "#" + command.Member?.Discriminator, LogID.DISCORD);
		}
	}
}

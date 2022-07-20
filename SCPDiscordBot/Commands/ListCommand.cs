using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class ListCommand : BaseCommandModule
	{
		[Command("list")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command)
		{
			if (!ConfigParser.IsCommandChannel(command.Channel.Id)) return;
			if (!ConfigParser.ValidatePermission(command)) return;

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				ListCommand = new Interface.ListCommand
				{
					ChannelID = command.Channel.Id
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending '" + command.Message.Content + "' to plugin from " + command.Member?.Username + "#" + command.Member?.Discriminator, LogID.DISCORD);
		}
	}
}

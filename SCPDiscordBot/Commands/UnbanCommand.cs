using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class UnbanCommand : BaseCommandModule
	{
		[Command("unban")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, string steamIDOrIP)
		{
			if (!ConfigParser.IsCommandChannel(command.Channel.Id)) return;
			if (!ConfigParser.ValidatePermission(command)) return;

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				UnbanCommand = new Interface.UnbanCommand
				{
					ChannelID = command.Channel.Id,
					SteamIDOrIP = steamIDOrIP,
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending '" + command.Message.Content + "' to plugin from " + command.Member.Username + "#" + command.Member.Discriminator, LogID.Discord);
		}
	}
}

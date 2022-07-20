using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class KickCommand : BaseCommandModule
	{
		[Command("kick")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, string steamID, [RemainingText] string reason = "")
		{
			if (!ConfigParser.IsCommandChannel(command.Channel.Id)) return;
			if (!ConfigParser.ValidatePermission(command)) return;

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				KickCommand = new Interface.KickCommand
				{
					ChannelID = command.Channel.Id,
					SteamID = steamID,
					AdminTag = command.Member.Username + "#" + command.Member.Discriminator,
					Reason = reason
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending '" + command.Message.Content + "' to plugin from " + command.Member.Username + "#" + command.Member.Discriminator, LogID.DISCORD);
		}
	}
}

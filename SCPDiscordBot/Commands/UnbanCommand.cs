using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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

			// Check if the user has permission to use this command.
			if (!ConfigParser.HasPermission(command.Member, "unban"))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "You do not have permission to use this command."
				};
				await command.RespondAsync(error);
				Logger.Log(command.Member.Username + "#" + command.Member.Discriminator + " tried to use the unban command but did not have permission.", LogID.Command);
				return;
			}

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				UnbanCommand = new Interface.UnbanCommand
				{
					ChannelID = command.Channel.Id,
					SteamIDOrIP = steamIDOrIP,
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending unban request to plugin for " + command.Member.Username + "#" + command.Member.Discriminator, LogID.Discord);
		}
	}
}

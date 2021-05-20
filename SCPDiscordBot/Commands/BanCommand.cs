using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class BanCommand : BaseCommandModule
	{
		[Command("ban")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, string steamID, string duration, [RemainingText] string reason = "")
		{
			// Check if the user has permission to use this command.
			if (!ConfigParser.HasPermission(command.Member, "ban"))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "You do not have permission to use this command."
				};
				await command.RespondAsync(error);
				Logger.Log(command.Member.Username + "#" + command.Member.Discriminator + " tried to use the ban command but did not have permission.", LogID.Command);
				return;
			}

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				BanCommand = new Interface.BanCommand
				{
					ChannelID = command.Channel.Id,
					SteamID = steamID,
					Duration = duration,
					AdminTag = command.Member.Username + "#" + command.Member.Discriminator,
					Reason = reason
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending ban request to plugin for " + command.Member.Username + "#" + command.Member.Discriminator, LogID.Discord);
		}
	}
}

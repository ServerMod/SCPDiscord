using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class ListCommand : BaseCommandModule
	{
		[Command("list")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command)
		{
			// Check if the user has permission to use this command.
			if (!ConfigParser.HasPermission(command.Member, "list"))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "You do not have permission to use this command."
				};
				await command.RespondAsync(error);
				Logger.Log(command.Member.Username + "#" + command.Member.Discriminator + " tried to use the list command but did not have permission.", LogID.Command);
				return;
			}

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				ListCommand = new Interface.ListCommand
				{
					ChannelID = command.Channel.Id
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending list request to plugin for " + command.Member.Username + "#" + command.Member.Discriminator, LogID.Discord);
		}
	}
}

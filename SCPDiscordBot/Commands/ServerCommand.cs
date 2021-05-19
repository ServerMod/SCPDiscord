using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace SCPDiscord.Commands
{
	public class ServerCommand : BaseCommandModule
	{
		[Command("server")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, [RemainingText] string serverCommand = "")
		{
			// Check if the user has permission to use this command.
			if (!ConfigParser.HasPermission(command.Member, "server " + serverCommand))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "You do not have permission to use this command."
				};
				await command.RespondAsync(error);
				Logger.Log(command.Member.Username + "#" + command.Member.Discriminator + " tried to use the server command but did not have permission.", LogID.Command);
				return;
			}

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				ConsoleCommand = new Interface.ConsoleCommand
				{
					ChannelID = command.Channel.Id,
					DiscordID = command.Member.Id,
					Command = serverCommand
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending server command request to plugin for " + command.Member.Username + "#" + command.Member.Discriminator, LogID.Discord);
		}
	}
}

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class SyncRoleCommand : BaseCommandModule
	{
		[Command("syncrole")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, ulong steamID)
		{
			// Check if the user has permission to use this command.
			if (!ConfigParser.HasPermission(command.Member, "syncrole"))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "You do not have permission to use this command."
				};
				await command.RespondAsync(error);
				Logger.Log("User tried to use the syncrole command but did not have permission.", LogID.Command);
				return;
			}

			if(steamID.ToString().Length != 17)
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "That SteamID doesn't seem to be the right length."
				};
				await command.RespondAsync(error);
				return;
			}

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				SyncRoleCommand = new Interface.SyncRoleCommand
				{
					ChannelID = command.Channel.Id,
					DiscordID = command.Member.Id,
					DiscordTag = command.Member.Username,
					SteamID = steamID
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending syncrole request to plugin for " + command.Member.Username + "#" + command.Member.Discriminator, LogID.Discord);
		}
	}
}

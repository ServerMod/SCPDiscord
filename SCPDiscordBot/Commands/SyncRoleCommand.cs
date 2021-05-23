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
			if (!ConfigParser.IsCommandChannel(command.Channel.Id)) return;
			if (!ConfigParser.ValidatePermission(command)) return;

			if (steamID.ToString().Length != 17)
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
			Logger.Debug("Sending '" + command.Message.Content + "' to plugin from " + command.Member.Username + "#" + command.Member.Discriminator, LogID.Discord);
		}
	}
}

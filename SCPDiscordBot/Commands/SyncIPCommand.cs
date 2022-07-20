using System.Text.RegularExpressions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class SyncIPCommand : BaseCommandModule
	{
		[Command("syncip")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, string ip)
		{
			if (!ConfigParser.IsCommandChannel(command.Channel.Id)) return;
			if (!ConfigParser.ValidatePermission(command)) return;

			if (Regex.IsMatch(ip, "^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)(\\.(?!$)|$)){4}$"))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "That IP doesn't seem to be in the right format, it should look something like \"255.255.255.255\"."
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
					SteamIDOrIP = ip
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending '" + command.Message.Content + "' to plugin from " + command.Member.Username + "#" + command.Member.Discriminator, LogID.DISCORD);
		}
	}
}

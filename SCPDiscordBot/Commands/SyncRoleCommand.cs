using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class SyncRoleCommand : BaseCommandModule
	{
		[Command("syncrole")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, ulong SteamID)
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

			DiscordEmbed response = new DiscordEmbedBuilder
			{
				Color = DiscordColor.Green,
				Description = "Account synced, " + command.Member.Mention + "!"
			};
			await command.RespondAsync(response);
		}
	}
}

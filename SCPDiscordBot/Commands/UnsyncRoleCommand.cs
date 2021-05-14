using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace SCPDiscord.Commands
{
	public class UnsyncRoleCommand : BaseCommandModule
	{
		[Command("unsyncrole")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command)
		{
			// Check if the user has permission to use this command.
			if (!ConfigParser.HasPermission(command.Member, "unsyncrole"))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "You do not have permission to use this command."
				};
				await command.RespondAsync(error);
				Logger.Log("User tried to use the unsyncrole command but did not have permission.", LogID.Command);
				return;
			}

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				UnsyncRoleCommand = new Interface.UnsyncRoleCommand
				{
					ChannelID = command.Channel.Id,
					DiscordID = command.Member.Id,
					DiscordTag = command.Member.Username
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending unsyncrole request to plugin for " + command.Member.Username + "#" + command.Member.Discriminator, LogID.Discord);
		}
	}
}

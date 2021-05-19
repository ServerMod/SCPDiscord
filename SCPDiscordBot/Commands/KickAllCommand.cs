﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SCPDiscord.Commands
{
	public class KickAllCommand : BaseCommandModule
	{
		[Command("kickall")]
		[Cooldown(1, 5, CooldownBucketType.User)]
		public async Task OnExecute(CommandContext command, [RemainingText] string kickReason = "")
		{
			// Check if the user has permission to use this command.
			if (!ConfigParser.HasPermission(command.Member, "kickall"))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "You do not have permission to use this command."
				};
				await command.RespondAsync(error);
				Logger.Log(command.Member.Username + "#" + command.Member.Discriminator + " tried to use the kickall command but did not have permission.", LogID.Command);
				return;
			}

			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				KickallCommand = new Interface.KickallCommand
				{
					ChannelID = command.Channel.Id,
					AdminTag = command.Member.Username + "#" + command.Member.Discriminator,
					Reason = command.RawArgumentString
				}
			};
			NetworkSystem.SendMessage(message);
			Logger.Debug("Sending kickall request to plugin for " + command.Member.Username + "#" + command.Member.Discriminator, LogID.Discord);
		}
	}
}

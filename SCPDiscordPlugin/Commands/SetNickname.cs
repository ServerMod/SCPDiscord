using System;
using System.Collections.Generic;
using System.Linq;
using Smod2.API;
using Smod2.Commands;

namespace SCPDiscord.Commands
{
	public class SetNickname : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Sets a nickname for a player.";
		}

		public string GetUsage()
		{
			return "scpd_setnickname <steamid/playerid> <name>";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player admin)
			{
				if (!admin.HasPermission("scpdiscord.setnickname"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}

			if (args.Length <= 1)
			{
				return new[] { "Invalid arguments." };
			}

			string steamIDOrPlayerID = args[0].Replace("@steam", ""); // Remove steam suffix if there is one

			List<Player> matchingPlayers = new List<Player>();
			try
			{
				SCPDiscord.plugin.Debug("Looking for player with SteamID/PlayerID: " + steamIDOrPlayerID);
				foreach (Player pl in SCPDiscord.plugin.Server.GetPlayers())
				{
					SCPDiscord.plugin.Debug("Player " + pl.PlayerId + ": SteamID " + pl.UserId + " PlayerID " + pl.PlayerId);
					if (pl.GetParsedUserID() == steamIDOrPlayerID)
					{
						SCPDiscord.plugin.Debug("Matching SteamID found");
						matchingPlayers.Add(pl);
					}
					else if (pl.PlayerId.ToString() == steamIDOrPlayerID)
					{
						SCPDiscord.plugin.Debug("Matching playerID found");
						matchingPlayers.Add(pl);
					}
				}
			}
			catch (Exception) { /* ignored */ }

			if (!matchingPlayers.Any())
			{
				return new[] { "Player \"" + args[0] + "\"not found." };
			}
			
			foreach (Player matchingPlayer in matchingPlayers)
			{
				matchingPlayer.DisplayedNickname = string.Join(" ", args.Skip(1));
			}

			return new[] { "Player nickname updated." };
		}
	}
}
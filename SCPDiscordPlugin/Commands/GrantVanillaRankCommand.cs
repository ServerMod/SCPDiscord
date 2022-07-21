using Smod2.API;
using Smod2.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCPDiscord.Commands
{
	public class GrantVanillaRankCommand : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Gives a player the vanilla rank provided.";
		}

		public string GetUsage()
		{
			return "scpd_givevanillarank/scpd_gvr <steamid/playerid> <rank>";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player admin)
			{
				if (!admin.HasPermission("scpdiscord.grantvanillarank"))
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
			catch (Exception)
			{
				return new[] { "Player \"" + args[0] + "\"not found." };
			}
			
			try
			{
				foreach (Player matchingPlayer in matchingPlayers)
				{
					matchingPlayer.SetRank(null, null, args[1]);
				}
			}
			catch (Exception)
			{
				return new[] { "Vanilla rank \"" + args[1] + "\" not found. Are you sure you are using the RA config role name and not the role title/badge?" };
			}
			
			return new[] { "Player rank updated." };
		}
	}
}
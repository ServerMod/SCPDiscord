using Smod2.API;
using Smod2.Commands;
using System;
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

			try
			{
				Player player = SCPDiscord.plugin.Server.GetPlayers(args[0]).First();
				player.SetRank(null, null, args[1]);
				return new[] { "Player rank updated." };

			}
			catch (InvalidOperationException)
			{
				return new[] { "Player not found." };
			}
		}
	}
}
using System;
using System.Linq;
using Smod2.API;
using Smod2.Commands;

namespace SCPDiscord.Commands
{
	public class GrantReservedSlotCommand : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Gives a user a reserved slot.";
		}

		public string GetUsage()
		{
			return "scpd_grantreservedslot/scpd_grs <steamid>";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player admin)
			{
				if (!admin.HasPermission("scpdiscord.grantreservedslot"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}

			if (args.Length <= 0)
			{
				return new[] { "Invalid arguments." };
			}

			if (ReservedSlot.GetSlots().Any(slot => slot.SteamID == args[0].Trim()))
			{
				return new[] { "This user already has a reserved slot." };
			}

			Player player = SCPDiscord.plugin.Server.GetPlayers(args[0]).FirstOrDefault(null);
			if (player == null)
			{
				new ReservedSlot("", args[0], "Offline player added via SCPDiscord " + DateTime.Now).AppendToFile();
			}
			else
			{
				new ReservedSlot(player.IpAddress, player.SteamId, player.Name + ", added via SCPDiscord " + DateTime.Now).AppendToFile();
			}
			return new[] { "Reserved slot added." };
		}
	}
}
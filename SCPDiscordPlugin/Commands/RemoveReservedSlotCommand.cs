using System.Linq;
using Smod2.API;
using Smod2.Commands;

namespace SCPDiscord.Commands
{
	public class RemoveReservedSlotCommand : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Removes a reserved slot from a player.";
		}

		public string GetUsage()
		{
			return "scpd_removereservedslot/scpd_rrs <steamid>";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.removereservedslot"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}

			if (args.Length <= 0)
			{
				return new[] { "Invalid arguments." };
			}

			if (ReservedSlot.GetSlots().All(slot => slot.SteamID != args[0].Trim()))
			{
				return new[] { "This user does not have a reserved slot." };
			}

			ReservedSlot.GetSlots().First(slot => slot.SteamID == args[0])?.RemoveSlotFromFile();

			return new[] { "Reserved slot removed." };
		}
	}
}
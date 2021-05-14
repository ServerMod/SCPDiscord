using Smod2.API;
using Smod2.Commands;

namespace SCPDiscord.Commands
{
	public class UnsyncCommand : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Removes a user from having their discord role synced to the server.";
		}

		public string GetUsage()
		{
			return "scpd_unsync <discord id>";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.unsync"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}

			if (args.Length > 0 && ulong.TryParse(args[0], out ulong discordID))
			{
				return new[] { SCPDiscord.plugin.roleSync.RemovePlayer(discordID) };
			}

			return new[] { "Invalid arguments." };
		}
	}
}
using Smod2.API;
using Smod2.Commands;

namespace SCPDiscord.Commands
{
	public class DebugCommand : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Toggles debug messages.";
		}

		public string GetUsage()
		{
			return "scpd_debug";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.debug"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}
			Config.SetBool("settings.debug", !Config.GetBool("settings.debug"));
			return new[] { "Debug messages: " + Config.GetBool("settings.debug") };
		}
	}
}
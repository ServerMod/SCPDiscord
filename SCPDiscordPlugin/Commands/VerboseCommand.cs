using Smod2.API;
using Smod2.Commands;

namespace SCPDiscord.Commands
{
	public class VerboseCommand : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Toggles verbose messages.";
		}

		public string GetUsage()
		{
			return "scpd_verbose";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.verbose"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}
			Config.SetBool("settings.verbose", !Config.GetBool("settings.verbose"));
			return new[] { "Verbose messages: " + Config.GetBool("settings.verbose") };
		}
	}
}
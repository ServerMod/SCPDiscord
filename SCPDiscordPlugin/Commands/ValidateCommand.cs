using Smod2.API;
using Smod2.Commands;

namespace SCPDiscord.Commands
{
	public class ValidateCommand : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Creates a config validation report.";
		}

		public string GetUsage()
		{
			return "scpd_validate";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.validate"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}

			Config.ValidateConfig(SCPDiscord.plugin);
			Language.ValidateLanguageStrings();

			return new[] { "Validation report posted in server console." };
		}
	}
}
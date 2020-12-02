using Smod2.API;
using Smod2.Commands;

namespace SCPDiscord.Commands
{
	public class ReloadCommand : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Reloads all plugin configs and data files and then reconnects.";
		}

		public string GetUsage()
		{
			return "scpd_reload";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.reload"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}

			SCPDiscord.plugin.Info("Reloading plugin...");
			SCPDiscord.plugin.LoadConfig();
			Language.Reload();
			SCPDiscord.plugin.maxPlayers = SCPDiscord.plugin.GetConfigInt("max_players");
			SCPDiscord.plugin.roleSync.Reload();
			if (NetworkSystem.IsConnected())
			{
				NetworkSystem.Disconnect();
			}

			return new[] { "Reload complete." };
		}
	}
}
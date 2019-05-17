using Smod2.API;
using Smod2.Commands;

namespace SCPDiscord.Commands
{
	public class ReconnectCommand : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Attempts to close the connection to the Discord bot and reconnect.";
		}

		public string GetUsage()
		{
			return "scpd_rc/scpd_reconnect";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.reconnect"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}

			if (NetworkSystem.IsConnected())
			{
				NetworkSystem.Disconnect();
				return new[] { "Connection closed, reconnecting will begin shortly." };
			}
			else
			{
				return new[] { "Connection was already closed, reconnecting is in progress." };
			}
		}
	}
}
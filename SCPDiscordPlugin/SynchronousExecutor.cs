using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SCPDiscord.Interface;
using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
	public class SynchronousExecutor : IEventHandlerFixedUpdate
	{
		private readonly SCPDiscord plugin;
		private readonly ConcurrentQueue<ConsoleCommand> queuedCommands = new ConcurrentQueue<ConsoleCommand>();
		private readonly ConcurrentQueue<string> queuedRoleSyncCommands = new ConcurrentQueue<string>();
		public SynchronousExecutor(SCPDiscord pl)
		{
			plugin = pl;
		}

		public void ScheduleDiscordCommand(ConsoleCommand command)
		{
			queuedCommands.Enqueue(command);
		}

		public void ScheduleRoleSyncCommand(string command)
        {
			queuedRoleSyncCommands.Enqueue(command);
        }

		public void OnFixedUpdate(FixedUpdateEvent ev)
		{
			while(queuedCommands.TryDequeue(out ConsoleCommand command))
			{
				string[] words = command.Command.Split(' ');
				string response = ConsoleCommand(plugin.PluginManager.Server, words[0], words.Skip(1).ToArray());
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "feedback", response }
				};

				EmbedMessage embed = new EmbedMessage
				{
					Colour = EmbedMessage.Types.DiscordColour.Orange,
					ChannelID = command.ChannelID
				};

				plugin.SendEmbedWithMessageByID(embed, "botresponses.consolecommandfeedback", variables);
			}

			while(queuedRoleSyncCommands.TryDequeue(out string stringCommand))
			{
				string[] words = stringCommand.Split(' ');
				plugin.Debug("RoleSync command response: " + ConsoleCommand(plugin.PluginManager.Server, words[0], words.Skip(1).ToArray()));
			}
		}

		private string ConsoleCommand(ICommandSender user, string command, string[] arguments)
		{
			if (user == null)
			{
				user = plugin.Server;
			}

			string[] feedback = plugin.PluginManager.CommandManager.CallCommand(user, command, arguments);

			StringBuilder builder = new StringBuilder();
			foreach (string line in feedback)
			{
				builder.Append(line + "\n");
			}
			return builder.ToString();
		}
	}
}
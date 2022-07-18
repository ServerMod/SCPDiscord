using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SCPDiscord.Interface;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
	public class SynchronousExecutor : IEventHandlerFixedUpdate
	{
		private readonly SCPDiscord plugin;
		private readonly ConcurrentQueue<ConsoleCommand> queuedCommands = new ConcurrentQueue<ConsoleCommand>();

		public SynchronousExecutor(SCPDiscord pl)
		{
			plugin = pl;
		}

		public void ScheduleConsoleCommand(ConsoleCommand command)
		{
			queuedCommands.Enqueue(command);
		}
		
		public void OnFixedUpdate(FixedUpdateEvent ev)
		{
			while(queuedCommands.TryDequeue(out ConsoleCommand command))
			{
				string[] words = command.Command.Split(' ');
				string response = plugin.ConsoleCommand(plugin.PluginManager.Server, words[0], words.Skip(1).ToArray());
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "feedback", response }
				};
				plugin.SendMessageByID(command.ChannelID, "botresponses.consolecommandfeedback", variables);
			}
		}
	}
}
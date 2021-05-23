using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord.EventListeners
{
	class TickCounter : IEventHandlerUpdate
	{
		private static short ticks;
		public static short Reset()
		{
			short value = ticks;
			ticks = 0;
			return value;
		}

		void IEventHandlerUpdate.OnUpdate(UpdateEvent ev)
		{
			ticks++;
		}
	}
}

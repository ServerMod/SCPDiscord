using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    class TickCounter : IEventHandlerUpdate
    {
        private static short ticks = 0;
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

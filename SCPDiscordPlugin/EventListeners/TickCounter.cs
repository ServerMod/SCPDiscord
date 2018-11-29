using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SCPDiscord
{
    class TickCounter : IEventHandlerUpdate
    {
        private static short ticks = 0;
        public static short Reset()
        {
            short value = ticks;
            ticks = 0;
            return ticks;
        }

        public void OnUpdate(UpdateEvent ev)
        {
            ticks++;
        }
    }
}

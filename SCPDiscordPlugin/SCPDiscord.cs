using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "SCPDiscord",
        description = "Plugin which outputs server events to Discord.",
        id = "karlofduty.scpdiscord",
        version = "3.0",
        SmodMajor = 3,
        SmodMinor = 0,
        SmodRevision = 0
        )]
    class ExamplePlugin : Plugin
    {

        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            this.Info("SCPDiscord enabled.");
        }

        public override void Register()
        {
        }
    }
}
using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;

using System.Net;
using System.Net.Sockets;
using System;

using System.Threading;

namespace SCPDiscord
{
    [PluginDetails(
        author = "Karl Essinger",
        name = "SCPDiscord",
        description = "Plugin which outputs server events to Discord.",
        id = "karlofduty.scpdiscord",
        version = "0.0.1",
        SmodMajor = 3,
        SmodMinor = 1,
        SmodRevision = 8
        )]
    class SCPDiscordPlugin : Plugin
    {
        public TcpClient clientSocket = new TcpClient();

        public override void Register()
        {

        }

        public override void OnEnable()
        {
            try
            {
                clientSocket.Connect("127.0.0.1", 8888);
            }
            catch(SocketException e)
            {
                this.Info("Error occured while connecting to discord bot server.\n" + e.ToString());
                this.pluginManager.DisablePlugin(this);
            }
            catch (ObjectDisposedException e)
            {
                this.Info("TCP client was unexpectedly closed.\n" + e.ToString());
                this.pluginManager.DisablePlugin(this);
            }
            catch (ArgumentOutOfRangeException e)
            {
                this.Info("Invalid port.\n" + e.ToString());
                this.pluginManager.DisablePlugin(this);
            }
            catch (ArgumentNullException e)
            {
                this.Info("IP address is null.\n" + e.ToString());
                this.pluginManager.DisablePlugin(this);
            }

            this.AddEventHandlers(new RoundEventHandler(this));
            this.AddEventHandlers(new PlayerEventHandler(this));

            this.Info("SCPDiscord enabled.");
            SendMessageAsync("Plugin Enabled.");
        }

        public void SendMessageAsync(string message)
        {
            Thread messageThread = new Thread(new ThreadStart(() => new AsyncMessage(this, message)));
            messageThread.Start();
        }

        public override void OnDisable()
        {
            this.Info("SCPDiscord disabled.");
        }
    }
}
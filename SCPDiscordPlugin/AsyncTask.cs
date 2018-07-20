using Smod2;
using System;
using System.Net.Sockets;

namespace SCPDiscord
{
    class AsyncMessage
    {
        public AsyncMessage(SCPDiscordPlugin plugin, string channelID, string message)
        {
            if(message != null && message != "")
            {
                if(channelID == "default")
                {
                    channelID = "000000000000000000";
                }

                // Reconnect feature
                try
                {
                    NetworkStream serverStream = plugin.clientSocket.GetStream();
                    byte[] outStream = System.Text.Encoding.ASCII.GetBytes(channelID + message + '\0');
                    serverStream.Write(outStream, 0, outStream.Length);

                    plugin.Info("Sent message '" + message + "' to discord.");
                }
                catch (SocketException e)
                {
                    plugin.Info("Error occured while connecting to discord bot server.\n" + e.ToString());
                    plugin.Info("Restarting plugin...");
                    plugin.clientSocket.Close();
                    plugin.OnEnable();
                }
                catch (ObjectDisposedException e)
                {
                    plugin.Info("TCP client was unexpectedly closed.\n" + e.ToString());
                    plugin.Info("Restarting plugin...");
                    plugin.clientSocket.Close();
                    plugin.OnEnable();
                }
            }
            else
            {
                plugin.Warn("Tried to send empty message to discord.");
            }
        }
    }
}

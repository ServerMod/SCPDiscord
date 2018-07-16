using Smod2;
using System.Net.Sockets;

namespace SCPDiscord
{
    class AsyncMessage
    {
        public AsyncMessage(SCPDiscordPlugin plugin, string message)
        {
            if(message != null && message != "")
            {
                NetworkStream serverStream = plugin.clientSocket.GetStream();
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(message);
                serverStream.Write(outStream, 0, outStream.Length);

                plugin.Info("Sent message '" + message + "' to discord.");
            }
            else
            {
                plugin.Warn("Tried to send empty message to discord.");
            }
        }
    }
}

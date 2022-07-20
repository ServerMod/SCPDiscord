using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SCPDiscord
{
	public class SCPDiscordBot
	{
		public static SCPDiscordBot instance;
		public static bool discordConnected = false;

		public static void Main(string[] args)
		{
			new SCPDiscordBot().MainAsync().GetAwaiter().GetResult();
		}

		private async Task MainAsync()
		{
			instance = this;

			Logger.Log("Starting SCPDiscord version " + GetVersion() + "...", LogID.GENERAL);
			try
			{
				Reload();

				// Block this task until the program is closed.
				await Task.Delay(-1);
			}
			catch (Exception e)
			{
				Logger.Fatal("Fatal error:", LogID.GENERAL);
				Logger.Fatal(e.ToString(), LogID.GENERAL);
				Console.ReadLine();
			}
		}

		public async void Reload()
		{
			ConfigParser.loaded = false;
			NetworkSystem.ShutDown();

			Logger.Log("Loading config \"" + Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "config.yml\"", LogID.CONFIG);
			ConfigParser.LoadConfig();

			await DiscordAPI.Reset();

			new Thread(() => new StartNetworkSystem()).Start();
		}

		public static bool IsDiscordReady()
		{
			return discordConnected;
		}

		public static string GetVersion()
		{
			Version version = Assembly.GetEntryAssembly()?.GetName().Version;
			return version?.Major + "." + version?.Minor + "." + version?.Build + (version?.Revision == 0 ? "" : "-" + (char)(64 + version?.Revision ?? 0));
		}
	}
}

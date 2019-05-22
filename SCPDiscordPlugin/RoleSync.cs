using Newtonsoft.Json.Linq;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SCPDiscord
{
    public class RoleSync
    {
        private Dictionary<string, string> syncedPlayers = new Dictionary<string, string>();

		// This variable is set when the config reloads instead of when the role system reloads as the config has to be read to get the info anyway
		public Dictionary<string, string[]> roleDictionary = new Dictionary<string, string[]>();

        private readonly SCPDiscord plugin;
        public RoleSync(SCPDiscord plugin)
        {
            this.plugin = plugin;
            Reload();
            plugin.Info("RoleSync system loaded.");
        }

        public void Reload()
        {
            plugin.SetUpFileSystem();
            syncedPlayers = JArray.Parse(File.ReadAllText(FileManager.GetAppFolder(plugin.GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json")).ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());
            plugin.Info("Successfully loaded config '" + FileManager.GetAppFolder(plugin.GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json'.");
        }

        private void SavePlayers()
        {
            // Save the state to file
            StringBuilder builder = new StringBuilder();
            builder.Append("[\n");
            foreach (KeyValuePair<string, string> player in syncedPlayers)
            {
                builder.Append("    {\"" + player.Key + "\": \"" + player.Value + "\"},\n");
            }
            builder.Append("]");
            File.WriteAllText(FileManager.GetAppFolder(plugin.GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json", builder.ToString());
        }

        public void SendRoleQuery(string steamID)
        {
            if (!syncedPlayers.ContainsKey(steamID))
            {
                return;
            }
            NetworkSystem.QueueMessage("rolequery " + steamID + " " + syncedPlayers[steamID]);
        }

        public void ReceiveQueryResponse(string steamID, string jsonString)
        {
	        try
	        {
	            Player player = plugin.Server.GetPlayers(steamID).First();
				
	            List<string> roles = JArray.Parse(jsonString).Values<string>().ToList();
				foreach (KeyValuePair<string, string[]> keyValuePair in this.roleDictionary)
				{
					if (roles.Contains(keyValuePair.Key))
					{
						Dictionary<string, string> variables = new Dictionary<string, string>
						{
							{ "ipaddress",    player.IpAddress                 },
							{ "name",         player.Name                      },
							{ "playerid",     player.PlayerId.ToString()       },
							{ "steamid",      player.SteamId                   }
						};
						foreach (string unparsedCommand in keyValuePair.Value)
						{
							string command = unparsedCommand;
							// Variable insertion
							foreach (KeyValuePair<string, string> variable in variables)
							{
								command = command.Replace("<var:" + variable.Key + ">", variable.Value);
							}
							plugin.Debug(this.plugin.ConsoleCommand(null, command.Split(' ')[0], command.Split(' ').Skip(1).ToArray()));
						}
			            
						plugin.Verbose("Synced " + player.Name);
						return;
					}
				}
	        }
	        catch (InvalidOperationException)
	        {
				this.plugin.Warn("Tried to run commands on a player who is not on the server anymore.");
	        }
        }

        public string AddPlayer(string steamID, string discordID)
        {
            if (syncedPlayers.ContainsKey(steamID))
            {
                return "SteamID is already linked to a Discord account";
            }

            if (syncedPlayers.ContainsValue(discordID))
            {
                return "Discord user ID is already linked to a Steam account";
            }

            string response = "";
            if (!CheckSteamAccount(steamID, ref response))
            {
                return response;
            }

            syncedPlayers.Add(steamID, discordID);
            SavePlayers();
            return "Successfully linked accounts.";
        }

        private bool CheckSteamAccount(string steamID, ref string response)
        {
            ServicePointManager.ServerCertificateValidationCallback = SSLValidation;
            HttpWebResponse webResponse = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://steamcommunity.com/profiles/" + steamID + "?xml=1");
                request.Method = "GET";

                webResponse = (HttpWebResponse)request.GetResponse();

                string xmlResponse = new StreamReader(webResponse.GetResponseStream() ?? new MemoryStream()).ReadToEnd();

                string[] foundStrings = xmlResponse.Split('\n').Where(w => w.Contains("steamID64")).ToArray();

                if (foundStrings.Length == 0)
                {
					response = "SteamID does not seem to exist.";
                    plugin.Debug(response);
                    return false;
                }
                response = "SteamID found.";
                return true;

            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    webResponse = (HttpWebResponse)e.Response;
                    response = "Error occured connecting to steam services.";
                    plugin.Error("Steam profile connection error: " + webResponse.StatusCode);
                }
                else
                {
                    response = "Error occured connecting to steam services.";
                    plugin.Error("Steam profile connection error: " + e.Status.ToString());
                }
            }
            finally
            {
	            webResponse?.Close();
            }
            return false;
        }

        private bool SSLValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
	            return true;
            }

            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            foreach (X509ChainStatus element in chain.ChainStatus)
            {
	            if (element.Status == X509ChainStatusFlags.RevocationStatusUnknown)
	            {
		            continue;
	            }

	            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
	            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
	            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
	            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;

	            // If chain is not valid
	            if (!chain.Build((X509Certificate2)certificate))
	            {
		            return false;
	            }
            }
            return true;
        }

        public string RemovePlayer(string discordID)
        {
            try
            {
                KeyValuePair<string, string> player = syncedPlayers.First(kvp => kvp.Value == discordID);
                syncedPlayers.Remove(player.Key);
                SavePlayers();
                return "Discord user ID link has been removed.";
            }
            catch(InvalidOperationException)
            {
                return "Discord user ID is not linked to a Steam account";
            }
            
        }
    }
}

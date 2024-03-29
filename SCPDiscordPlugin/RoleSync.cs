using Newtonsoft.Json.Linq;
using SCPDiscord.Interface;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SCPDiscord
{
	public class RoleSync
	{
		private Dictionary<string, ulong> syncedPlayers = new Dictionary<string, ulong>();

		// This variable is set when the config reloads instead of when the role system reloads as the config has to be read to get the info anyway
		public Dictionary<ulong, string[]> roleDictionary = new Dictionary<ulong, string[]>();

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
			syncedPlayers = JArray.Parse(File.ReadAllText(FileManager.GetAppFolder(true, !plugin.GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json")).ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<ulong>());
			plugin.Info("Successfully loaded rolesync '" + FileManager.GetAppFolder(true, !plugin.GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json'.");
		}

		private void SavePlayers()
		{
			// Save the state to file
			StringBuilder builder = new StringBuilder();
			builder.Append("[\n");
			foreach (KeyValuePair<string, ulong> player in syncedPlayers)
			{
				builder.Append("    {\"" + player.Key + "\": \"" + player.Value + "\"},\n");
			}
			builder.Append("]");
			File.WriteAllText(FileManager.GetAppFolder(true, !plugin.GetConfigBool("scpdiscord_rolesync_global")) + "SCPDiscord/rolesync.json", builder.ToString());
		}

		public void SendRoleQuery(Player player)
		{
			if (plugin.GetConfigBool("online_mode"))
			{
				if (player.UserIDType != UserIdType.STEAM || !syncedPlayers.ContainsKey(player.UserID))
				{
					return;
				}

				MessageWrapper message = new MessageWrapper
				{
					UserQuery = new UserQuery
					{
						SteamIDOrIP = player.UserID,
						DiscordID = syncedPlayers[player.UserID]
					}
				};

				NetworkSystem.QueueMessage(message);
			}
			else
			{
				if (!syncedPlayers.ContainsKey(player.IPAddress))
				{
					return;
				}

				MessageWrapper message = new MessageWrapper
				{
					UserQuery = new UserQuery
					{
						SteamIDOrIP = player.IPAddress,
						DiscordID = syncedPlayers[player.IPAddress]
					}
				};

				NetworkSystem.QueueMessage(message);
			}
		}

		public void ReceiveQueryResponse(UserInfo userInfo)
		{
			Task.Delay(1000);
			try
			{
				// For online servers this should always be one player but for offline servers it may match several
				List<Player> matchingPlayers = new List<Player>();
				try
				{
					plugin.Debug("Looking for player with SteamID/IP: " + userInfo.SteamIDOrIP);
					foreach (Player pl in plugin.Server.GetPlayers())
					{
						plugin.Debug("Player " + pl.PlayerID + ": SteamID " + pl.UserID + " IP " + pl.IPAddress);
						if (pl.UserID == userInfo.SteamIDOrIP)
						{
							plugin.Debug("Matching SteamID found");
							matchingPlayers.Add(pl);
						}
						else if (pl.IPAddress == userInfo.SteamIDOrIP)
						{
							plugin.Debug("Matching IP found");
							matchingPlayers.Add(pl);
						}
					}
				}
				catch (NullReferenceException e)
				{
					plugin.Error("Error getting player for RoleSync: " + e);
					return;
				}

				if (matchingPlayers.Count == 0)
				{
					plugin.Error("Could not get player for rolesync, did they disconnect immediately?");
					return;
				}

				foreach (Player player in matchingPlayers)
				{
					foreach (KeyValuePair<ulong, string[]> keyValuePair in roleDictionary)
					{
						plugin.Debug("User has discord role " + keyValuePair.Key + ": " + userInfo.RoleIDs.Contains(keyValuePair.Key));
						if (userInfo.RoleIDs.Contains(keyValuePair.Key))
						{
							Dictionary<string, string> variables = new Dictionary<string, string>
							{
								{ "ipaddress",                        player.IPAddress                          },
								{ "name",                             player.Name                               },
								{ "playerid",                         player.PlayerID.ToString()                },
								{ "userid",                           player.UserID                             },
								{ "steamid",                          player.GetParsedUserID()                  },
								{ "discorddisplayname",               userInfo.DiscordDisplayName               },
								{ "discordusername",                  userInfo.DiscordUsername                  },
								{ "discordusernamewithdiscriminator", userInfo.DiscordUsernameWithDiscriminator },
								{ "discordid",                        userInfo.DiscordID.ToString()             }
							};
							foreach (string unparsedCommand in keyValuePair.Value)
							{
								string command = unparsedCommand;
								// Variable insertion
								foreach (KeyValuePair<string, string> variable in variables)
								{
									command = command.Replace("<var:" + variable.Key + ">", variable.Value);
								}
								plugin.Debug("Running rolesync command: " + command);
								plugin.sync.ScheduleRoleSyncCommand(command);
							}

							plugin.Verbose("Synced " + player.Name + " (" + userInfo.SteamIDOrIP + ") with Discord role id " + keyValuePair.Key);
							return;
						}
					}
				}
			}
			catch (InvalidOperationException)
			{
				plugin.Warn("Tried to run commands on a player who is not on the server anymore.");
			}
		}

		public EmbedMessage AddPlayer(string steamIDOrIP, ulong discordID, ulong channelID)
		{
			if (plugin.GetConfigBool("online_mode"))
			{
				plugin.Warn("SERVER IS IN ONLINE MODE");
				if (syncedPlayers.ContainsKey(steamIDOrIP + "@steam"))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = channelID,
						Description = "SteamID is already linked to a Discord account. You will have to remove it first."
					};
				}

				if (syncedPlayers.ContainsValue(discordID))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = channelID,
						Description = "Discord user ID is already linked to a Steam account. You will have to remove it first."
					};
				}

				string response = "";
				if (!CheckSteamAccount(steamIDOrIP, ref response))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = channelID,
						Description = response
					};
				}

				syncedPlayers.Add(steamIDOrIP + "@steam", discordID);
				SavePlayers();
				return new EmbedMessage
				{
					Colour = EmbedMessage.Types.DiscordColour.Green,
					ChannelID = channelID,
					Description = "Successfully linked accounts."
				};
			}
			else
			{
				plugin.Warn("SERVER IS IN OFFLINE MODE");
				if (syncedPlayers.ContainsKey(steamIDOrIP))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = channelID,
						Description = "IP is already linked to a Discord account. You will have to remove it first."
					};
				}

				if (syncedPlayers.ContainsValue(discordID))
				{
					return new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Red,
						ChannelID = channelID,
						Description = "Discord user ID is already linked to an IP. You will have to remove it first."
					};
				}

				syncedPlayers.Add(steamIDOrIP, discordID);
				SavePlayers();
				return new EmbedMessage
				{
					Colour = EmbedMessage.Types.DiscordColour.Green,
					ChannelID = channelID,
					Description = "Successfully linked accounts."
				};
			}
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

		public EmbedMessage RemovePlayer(ulong discordID, ulong channelID)
		{
			if (!syncedPlayers.ContainsValue(discordID))
			{
				return new EmbedMessage
				{
					Colour = EmbedMessage.Types.DiscordColour.Red,
					ChannelID = channelID,
					Description = "Discord user ID is not linked to a Steam account or IP"
				};
			}

			KeyValuePair<string, ulong> player = syncedPlayers.First(kvp => kvp.Value == discordID);
			syncedPlayers.Remove(player.Key);
			SavePlayers();
			return new EmbedMessage
			{
				Colour = EmbedMessage.Types.DiscordColour.Green,
				ChannelID = channelID,
				Description = "Discord user ID link has been removed."
			};
		}

		public string RemovePlayerLocally(ulong discordID)
		{
			if (!syncedPlayers.ContainsValue(discordID))
			{
				return "Discord user ID is not linked to a Steam account or IP";
			}

			KeyValuePair<string, ulong> player = syncedPlayers.First(kvp => kvp.Value == discordID);
			syncedPlayers.Remove(player.Key);
			SavePlayers();
			return "Discord user ID link has been removed.";
		}
	}
}

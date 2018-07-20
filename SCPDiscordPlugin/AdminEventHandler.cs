using Smod2.EventHandlers;
using Smod2.Events;

namespace SCPDiscord
{
    //Comments here are my own as there were none in the Smod2 api
    class AdminEventHandler : IEventHandlerAdminQuery, IEventHandlerAuthCheck, IEventHandlerBan
    {
        private SCPDiscordPlugin plugin;

        public AdminEventHandler(SCPDiscordPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnAdminQuery(AdminQueryEvent ev)
        {
            ///Triggered whenever an adming uses an admin command, both gui and commandline RA
            if(ev.Query == "REQUEST_DATA PLAYER_LIST SILENT")
            {
                return;
            }
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onadminquery"), plugin.MultiLanguage(48) + ev.Admin.Name + " (" + ev.Admin.SteamId + plugin.MultiLanguage(49) + ev.Query + "'.");
        }

        public void OnAuthCheck(AuthCheckEvent ev)
        {
            ///Probably triggered when someone gains access to the admin panel using a password, not sure
            if(ev.Allow)
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onauthcheck"), ev.Requester + " (" + ev.Requester.SteamId + plugin.MultiLanguage(50));
            }
            else
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onauthcheck"), ev.Requester + " (" + ev.Requester.SteamId + plugin.MultiLanguage(51));
            }
        }

        public void OnBan(BanEvent ev)
        {
            ///Doesn't seem to trigger at all, not sure why
            if(ev.AllowBan)
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onban"), "Player " + ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(52) + ev.Admin.Name + " (" + ev.Admin.SteamId + plugin.MultiLanguage(53) + (ev.Duration / 60) + plugin.MultiLanguage(54));
            }
            else
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onban"), ev.Admin.Name + " (" + ev.Admin.SteamId + plugin.MultiLanguage(55) + ev.Player.Name + " (" + ev.Player.SteamId + plugin.MultiLanguage(56) + (ev.Duration / 60) + plugin.MultiLanguage(57));
            }
        }
    }
}

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
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onadminquery"), plugin.MultiLanguage(64) + " " + ev.Admin.Name + " (" + ev.Admin.SteamId + ") " + plugin.MultiLanguage(65) + " '" + ev.Query + "'.");
        }

        public void OnAuthCheck(AuthCheckEvent ev)
        {
            ///Probably triggered when someone gains access to the admin panel using a password, not sure
            if(ev.Allow)
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onauthcheck"), ev.Requester + " (" + ev.Requester.SteamId + ") " + plugin.MultiLanguage(66));
            }
            else
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onauthcheck"), ev.Requester + " (" + ev.Requester.SteamId + ") " + plugin.MultiLanguage(67));
            }
        }

        public void OnBan(BanEvent ev)
        {
            ///Doesn't seem to trigger at all, not sure why
            if(ev.AllowBan)
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onban"), plugin.MultiLanguage(68) + " " + ev.Player.Name + " (" + ev.Player.SteamId + ") " + plugin.MultiLanguage(69) + " " + ev.Admin.Name + " (" + ev.Admin.SteamId + ") " + plugin.MultiLanguage(70) + " " + (ev.Duration / 60) + " " + plugin.MultiLanguage(71));
            }
            else
            {
                plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onban"), ev.Admin.Name + " (" + ev.Admin.SteamId + ") " + plugin.MultiLanguage(72) + " " + ev.Player.Name + " (" + ev.Player.SteamId + ") " + plugin.MultiLanguage(73) + " " + (ev.Duration / 60) + " " + plugin.MultiLanguage(74));
            }
        }
    }
}

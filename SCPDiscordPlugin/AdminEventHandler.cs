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
            plugin.SendMessageAsync("Admin " + ev.Admin.Name + " (" + ev.Admin.SteamId + ") executed command '" + ev.Query + "'.");
        }

        public void OnAuthCheck(AuthCheckEvent ev)
        {
            ///Probably triggered when someone gains access to the admin panel using a password, not sure
            if(ev.Allow)
            {
                plugin.SendMessageAsync(ev.Requester + " (" + ev.Requester.SteamId + ") was granted access to the RA panel.");
            }
            else
            {
                plugin.SendMessageAsync(ev.Requester + " (" + ev.Requester.SteamId + ") was denied access to the RA panel.");
            }
        }

        public void OnBan(BanEvent ev)
        {
            ///Doesn't seem to trigger at all, not sure why
            if(ev.AllowBan)
            {
                plugin.SendMessageAsync("Player " + ev.Player.Name + " (" + ev.Player.SteamId + ") was banned by " + ev.Admin.Name + " (" + ev.Admin.SteamId + ") for " + (ev.Duration / 60) + " minutes.");
            }
            else
            {
                plugin.SendMessageAsync(ev.Admin.Name + " (" + ev.Admin.SteamId + ") tried to ban " + ev.Player.Name + " (" + ev.Player.SteamId + ") for " + (ev.Duration / 60) + " minutes but was not allowed.");
            }
        }
    }
}

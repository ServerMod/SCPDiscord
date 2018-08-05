using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace SCPDiscord
{
    //Comments here are my own as there were none in the Smod2 api
    class AdminEventListener : IEventHandlerAdminQuery, IEventHandlerAuthCheck, IEventHandlerBan
    {
        private SCPDiscordPlugin plugin;

        public AdminEventListener(SCPDiscordPlugin plugin)
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
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",      ev.Admin.IpAddress },
                { "name",           ev.Admin.Name },
                { "playerid",       ev.Admin.PlayerId.ToString() },
                { "steamid",        ev.Admin.SteamId },
                { "class",          ev.Admin.TeamRole.Role.ToString() },
                { "handled",        ev.Handled.ToString() },
                { "output",         ev.Output },
                { "query",          ev.Query },
                { "successful",     ev.Successful.ToString() }
            };

            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onadminquery"), "admin.onadminquery", variables);
        }

        public void OnAuthCheck(AuthCheckEvent ev)
        {
            ///Probably triggered when someone gains access to the admin panel using a password, not sure
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "allow",          ev.Allow.ToString() },
                { "authtype",       ev.AuthType.ToString() },
                { "deniedmessage",  ev.DeniedMessage },
                { "ipaddress",      ev.Requester.IpAddress },
                { "name",           ev.Requester.Name },
                { "playerid",       ev.Requester.PlayerId.ToString() },
                { "steamid",        ev.Requester.SteamId },
                { "class",          ev.Requester.TeamRole.Role.ToString() },
            };
            plugin.SendParsedMessageAsync(plugin.GetConfigString("discord_channel_onauthcheck"), "admin.onauthcheck", variables);
        }

        //public void OnBan(BanEvent ev)
        //{
        //    ///Doesn't seem to trigger at all, not sure why
        //    if(ev.AllowBan)
        //    {
        //        plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onban"), "Player " + ev.Player.Name + " (" + ev.Player.SteamId + ") was banned by " + ev.Admin.Name + " (" + ev.Admin.SteamId + ") for " + (ev.Duration / 60) + " minutes.");
        //    }
        //    else
        //    {
        //        plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onban"), ev.Admin.Name + " (" + ev.Admin.SteamId + ") tried to ban " + ev.Player.Name + " (" + ev.Player.SteamId + ") for " + (ev.Duration / 60) + " minutes but was not allowed.");
        //    }
        //}
        public void OnBan(BanEvent ev)
        {
            plugin.SendMessageAsync(plugin.GetConfigString("discord_channel_onban"), "Player " + ev.Player.Name + " (" + ev.Player.SteamId + ") was banned by " + ev.Admin.Name + " (" + ev.Admin.SteamId + ") for " + (ev.Duration / 60) + " minutes.");
        }
    }
}

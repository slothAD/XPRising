using ProjectM.Network;
using OpenRPG.Utils;
using OpenRPG.Systems;
using System.Linq;
using System;
using VampireCommandFramework;
using ProjectM;

namespace OpenRPG.Commands
{

    
    public static class BanUser
    {

        [Command("ban info", shortHand: "ban info", adminOnly: false, usage: "<playername>", description: "Check the status of specified player")]
        public static void InfoBan(ChatCommandContext ctx, string playername)
        {
            if (Helper.FindPlayer(playername, false, out _, out var targetUserEntity_))
            {
                var targetData_ = Plugin.Server.EntityManager.GetComponentData<User>(targetUserEntity_);
                if (BanSystem.IsUserBanned(targetData_.PlatformId, out var banData_))
                {
                    TimeSpan duration = banData_.BanUntil - DateTime.Now;
                    ctx.Reply($"Player:<color=#fffffffe> {playername}</color>");
                    ctx.Reply($"Status:<color=#fffffffe> Banned</color> | By:<color=#fffffffe> {banData_.BannedBy}</color>");
                    ctx.Reply($"Duration:<color=#fffffffe> {Math.Round(duration.TotalDays)}</color> day(s) [<color=#fffffffe>{banData_.BanUntil}</color>]");
                    ctx.Reply($"Reason:<color=#fffffffe> {banData_.Reason}</color>");
                }
                else
                {
                    throw ctx.Error("Specified user is not banned.");
                }
            }
            else
            {
                throw ctx.Error("Unable to find the specified player.");
            }
        }

        [Command("ban", shortHand: "ban", adminOnly: false, usage: "<playername> <days> \"<reason>\"", description: "Ban a player, 0 days is permanent.")]
        public static void Ban(ChatCommandContext ctx, string playername, int days, string reason)
        {

            if (reason.Length > 150)
            {
                throw ctx.Error("Keep the reason short will ya?!");
            }

            if (Helper.FindPlayer(playername, false, out _, out var targetUserEntity))
            {
                if (BanSystem.BanUser(ctx.Event.SenderUserEntity, targetUserEntity, days, reason, out var banData))
                {
                    var user = ctx.Event.User;
                    Helper.KickPlayer(targetUserEntity);
                    ctx.Reply($"Player \"{playername}\" is now banned.");
                    ctx.Reply($"Banned Until:<color=#fffffffe> {banData.BanUntil}</color>");
                    ctx.Reply($"Reason:<color=#fffffffe> {reason}</color>");
                    return;
                }
                else
                {
                    ctx.Error($"Failed to ban \"{playername}\".");
                    return;
                }
            }
            else
            {
                ctx.Error("Specified player not found.");
                return;
            }
        }

        [Command("unban", shortHand: "uban", adminOnly: false, usage: "<playername>", description: "Unban the specified player.")]
        public static void Unban(ChatCommandContext ctx, string playername)
        {

            if (Helper.FindPlayer(playername, false, out _, out var targetUserEntity))
            {
                if (BanSystem.UnbanUser(targetUserEntity))
                {
                    ctx.Reply($"Player \"{playername}\" is no longer banned.");
                    return;
                }
                else
                {
                    ctx.Error($"Specified player does not exist in the ban database.");
                    return;
                }
            }
            else
            {
                ctx.Error("Specified player not found.");
                return;
            }
        }
    }
}

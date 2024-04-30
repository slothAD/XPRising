using OpenRPG.Utils;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    public static class NoCooldown
    {

        [Command("nocooldown", shortHand:"nocd", description: "Toggles instant cooldown for all abilities.", adminOnly:true)]
        public static void NoCooldownCommand(ChatCommandContext ctx)
        {
            var steamID = ctx.Event.User.PlatformId;
            Database.nocooldownlist.TryGetValue(steamID, out bool isNoCooldown);
            var newIsNoCooldown = !isNoCooldown;
            UpdateCooldownList(steamID, newIsNoCooldown);
            var p = newIsNoCooldown ? "Activated" : "Deactivated";
            ctx.Reply($"No Cooldown is now <color=#ffff00>{p}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Helper.AppliedBuff);
        }

        public static void UpdateCooldownList(ulong steamID, bool isNoCooldown)
        {
            if (isNoCooldown) Database.nocooldownlist[steamID] = true;
            else Database.nocooldownlist.Remove(steamID);
        }
    }
}
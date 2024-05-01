using OpenRPG.Utils;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    
    public static class SunImmunity
    {
        [Command(name: "sunimmunity", shortHand: "si", adminOnly: false, usage: "", description: "Toggles sun immunity.")]
        public static void SunImmunityCommand(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isSunImmune = Database.sunimmunity.ContainsKey(SteamID);
            if (isSunImmune) isSunImmune = false;
            else isSunImmune = true;
            UpdateImmunity(ctx, isSunImmune);
            string s = isSunImmune ? "Activated" : "Deactivated";
            ctx.Reply($"Sun Immunity <color=#ffff00>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Helper.AppliedBuff);
        }

        public static bool UpdateImmunity(ChatCommandContext ctx, bool isSunImmune)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.sunimmunity.ContainsKey(SteamID);
            if (isExist || !isSunImmune) RemoveImmunity(ctx);
            else Database.sunimmunity.Add(SteamID, isSunImmune);
            return true;
        }
        
        public static bool RemoveImmunity(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Database.sunimmunity.ContainsKey(SteamID))
            {
                Database.sunimmunity.Remove(SteamID);
                return true;
            }
            return false;
        }
    }
}

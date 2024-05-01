using System.Collections.Generic;
using OpenRPG.Utils;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    public static class GodMode
    {
        [Command(name: "godmode", shortHand: "gm", adminOnly: false, usage: "", description: "Toggles god mode for the current user")]
        public static void GodModeCommand(ChatCommandContext ctx)
        {
            var steamID = ctx.Event.User.PlatformId;
            
            var isGodMode = Database.godmode.GetValueOrDefault(steamID, false);
            var newIsGodMode = !isGodMode;
            UpdateGodMode(steamID, newIsGodMode);
            var s = newIsGodMode ? "Activated" : "Deactivated";
            ctx.Reply($"God mode <color=#ffff00>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Helper.AppliedBuff);
        }

        public static void UpdateGodMode(ulong steamID, bool isGodMode)
        {
            if (isGodMode) Database.godmode[steamID] = true;
            else Database.godmode.Remove(steamID);
        }
    }
}

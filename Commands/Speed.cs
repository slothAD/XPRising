using OpenRPG.Utils;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    public static class Speed
    {
        [Command("speed", description: "Toggles increased movement speed.", adminOnly:true )]
        public static void SpeedCommand(ChatCommandContext ctx)
        {
            var steamID = ctx.Event.User.PlatformId;
            Database.speeding.TryGetValue(steamID, out bool isSpeeding);
            var newSpeedingValue = !isSpeeding;
            UpdateSpeed(steamID, newSpeedingValue);
            var s = newSpeedingValue ? "Activated" : "Deactivated";
            ctx.Reply($"Speed buff <color=#ffff00>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Helper.appliedBuff);
        }

        public static void UpdateSpeed(ulong steamID, bool isSpeeding)
        {
            if (isSpeeding) Database.speeding[steamID] = true;
            else Database.speeding.Remove(steamID);
        }
    }
}

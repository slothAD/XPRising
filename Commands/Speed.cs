using OpenRPG.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VampireCommandFramework;

namespace OpenRPG.Commands
{

    [CommandGroup("rpg")]
    public static class Speed
    {
        [Command("speed", usage: "", description: "Toggles increased movement speed.")]
        public static void SpeedCommand(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isSpeeding = Database.speeding.TryGetValue(SteamID, out bool isSpeeding_);
            if (isSpeeding) isSpeeding = false;
            else isSpeeding = true;
            UpdateSpeed(ctx, isSpeeding);
            string s = isSpeeding ? "Activated" : "Deactivated";
            ctx.Reply( $"Speed buff <color=#ffff00>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.Buff.Buff_VBlood_Perk_Moose);
        }

        public static bool UpdateSpeed(ChatCommandContext ctx, bool isGodMode)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.speeding.TryGetValue(SteamID, out bool isSpeeding_);
            if (isExist || !isGodMode) RemoveSpeed(ctx);
            else Database.speeding.Add(SteamID, isGodMode);
            return true;
        }

        public static void SaveSpeed()
        {
            File.WriteAllText(Plugin.SpeedingJson, JsonSerializer.Serialize(Database.speeding, Database.JSON_options));
        }

        public static bool RemoveSpeed(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId; ;
            if (Database.speeding.TryGetValue(SteamID, out bool isGodMode_))
            {
                Database.speeding.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadSpeed()
        {

            if (!Directory.Exists(Plugin.ConfigPath)) Directory.CreateDirectory(Plugin.ConfigPath);
            if (!Directory.Exists(Plugin.SavesPath)) Directory.CreateDirectory(Plugin.SavesPath);

            if (!File.Exists(Plugin.SpeedingJson))
            {
                var stream = File.Create(Plugin.SpeedingJson);
                stream.Dispose();
            }
            string json = File.ReadAllText(Plugin.SpeedingJson);
            try
            {
                Database.speeding = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("Speed DB Populated.");
            }
            catch
            {
                Database.speeding = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("Speed DB Created.");
            }
        }
    }
}

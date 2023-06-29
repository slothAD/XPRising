using ProjectM;
using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VampireCommandFramework;

namespace RPGMods.Commands
{

    public static class Speed {
        [Command("speed", description: "Toggles increased movement speed.", adminOnly:true )]
        public static void Initialize(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isSpeeding = Database.speeding.TryGetValue(SteamID, out bool isSpeeding_);
            if (isSpeeding) isSpeeding = false;
            else isSpeeding = true;
            UpdateSpeed(ctx, isSpeeding);
            string s = isSpeeding ? "Activated" : "Deactivated";
            ctx.Reply($"Speed buff <color=#ffff00>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Helper.appliedBuff);
        }

        public static bool UpdateSpeed(ChatCommandContext ctx, bool isGodMode)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.speeding.TryGetValue(SteamID, out bool isSpeeding_);
            if (isExist || !isGodMode) RemoveSpeed(ctx);
            else Database.speeding.Add(SteamID, isGodMode);
            return true;
        }

        public static void SaveSpeed(string saveFolder)
        {
            File.WriteAllText(saveFolder +"speeding.json", JsonSerializer.Serialize(Database.speeding, Database.JSON_options));
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

        public static void LoadSpeed(){
            string specificFile = "speeding.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder, specificFile);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder, specificFile);
            string json = File.ReadAllText(AutoSaveSystem.mainSaveFolder + specificFile);
            try {
                Database.speeding = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                if (Database.speeding == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificFile);
                    Database.speeding = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                }
                Plugin.Logger.LogWarning("Speed DB Populated.");
            } catch {
                Database.speeding = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("Speed DB Created.");
            }
        }
    }
}

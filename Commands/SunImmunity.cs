using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProjectM;
using RPGMods.Utils;
using VampireCommandFramework;

namespace RPGMods.Commands
{
    public static class SunImmunity {
        [Command("sunimmunity", "sun", description: "Toggles sun immunity.", adminOnly:true)]
        public static void Initialize(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isSunImmune = Database.sunimmunity.ContainsKey(SteamID);
            if (isSunImmune) isSunImmune = false;
            else isSunImmune = true;
            UpdateImmunity(ctx, isSunImmune);
            string s = isSunImmune ? "Activated" : "Deactivated";
            ctx.Reply($"Sun Immunity <color=#ffff00>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.Buff.Buff_VBlood_Perk_Moose);
        }

        public static bool UpdateImmunity(ChatCommandContext ctx, bool isSunImmune)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.sunimmunity.ContainsKey(SteamID);
            if (isExist || !isSunImmune) RemoveImmunity(ctx);
            else Database.sunimmunity.Add(SteamID, isSunImmune);
            return true;
        }

        public static void SaveImmunity()
        {
            File.WriteAllText(AutoSaveSystem.mainSaveFolder+"sunimmunity.json", JsonSerializer.Serialize(Database.sunimmunity, Database.JSON_options));
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

        public static void LoadSunImmunity(){
            string specificFile = "sunimmunity.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder, specificFile);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder, specificFile);
            string json = File.ReadAllText(AutoSaveSystem.mainSaveFolder +specificFile);
            try {
                Database.sunimmunity = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                if (Database.sunimmunity == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificFile);
                    Database.sunimmunity = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                }
                Plugin.Logger.LogWarning("SunImmunity DB Populated.");
            } catch {
                Database.sunimmunity = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("SunImmunity DB Created.");
            }

        }
    }
}

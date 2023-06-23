using ProjectM;
using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using VampireCommandFramework;

namespace RPGMods.Commands
{
    public static class NoCooldown
    {

        [Command("nocooldown", shortHand:"Nocd", description: "Toggles instant cooldown for all abilities.", adminOnly:true)]
        public static void Initialize(ChatCommandContext ctx)
        {
            Entity PlayerCharacter = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isNoCD = Database.nocooldownlist.TryGetValue(SteamID, out bool isNoCooldown_);
            if (isNoCD) isNoCD = false;
            else isNoCD = true;
            UpdateCooldownList(SteamID, isNoCD);
            string p = isNoCD ? "Activated" : "Deactivated";
            ctx.Reply($"No Cooldown is now <color=#ffff00>{p}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Helper.appliedBuff);
        }

        public static bool UpdateCooldownList(ulong SteamID, bool isNoCooldown)
        {
            bool isExist = Database.nocooldownlist.TryGetValue(SteamID, out bool isNoCooldown_);
            if (isExist || !isNoCooldown) RemoveCooldown(SteamID);
            else Database.nocooldownlist.Add(SteamID, isNoCooldown);
            return true;
        }

        public static void SaveCooldown(string saveFolder)
        {
            File.WriteAllText(saveFolder+"nocooldown.json", JsonSerializer.Serialize(Database.nocooldownlist, Database.JSON_options));
        }

        public static bool RemoveCooldown(ulong SteamID)
        {
            if (Database.nocooldownlist.TryGetValue(SteamID, out bool isNoCooldown_))
            {
                Database.nocooldownlist.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadNoCooldown() {
            string specificName = "nocooldown.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder,specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            string json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+specificName);
            try {
                Database.nocooldownlist = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                if (Database.nocooldownlist == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.nocooldownlist = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                }
                Plugin.Logger.LogWarning("NoCooldown DB Populated.");
            } catch {
                Database.nocooldownlist = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("NoCooldown DB Created.");
            }
        }
    }
}
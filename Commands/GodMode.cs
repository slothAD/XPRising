using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProjectM;
using RPGMods.Utils;
using VampireCommandFramework;

namespace RPGMods.Commands
{
    public static class GodMode
    {
        /*
        public static void Initialize(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isGodMode = Database.godmode.TryGetValue(SteamID, out bool isGodMode_);
            if (isGodMode) isGodMode = false;
            else isGodMode = true;
            UpdateGodMode(ctx, isGodMode);
            string s = isGodMode ? "Activated" : "Deactivated";
            ctx.Reply($"God mode <color=#ffff00>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.Buff.Buff_VBlood_Perk_Moose);
        }*/

        [Command("godmode","god", "godmode", "Toggles god mode.")]
        public static bool UpdateGodMode(ChatCommandContext ctx, bool isGodMode){
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.godmode.TryGetValue(SteamID, out bool isGodMode_);
            if (isExist || !isGodMode) RemoveGodMode(SteamID);
            else Database.godmode.Add(SteamID, isGodMode);
            return true;
        }

        public static void SaveGodMode(string saveFolder) {
            File.WriteAllText(saveFolder+"godmode.json", JsonSerializer.Serialize(Database.godmode, Database.JSON_options));
        }

        public static bool RemoveGodMode(ulong SteamID){
            if (Database.godmode.TryGetValue(SteamID, out bool isGodMode_)){
                Database.godmode.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadGodMode() {
            string specificName = "godmode.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder + specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder + specificName);
            string json = File.ReadAllText(AutoSaveSystem.mainSaveFolder + specificName);
            try {
                Database.godmode = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                if (Database.godmode == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.godmode = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                }
                Plugin.Logger.LogWarning("GodMode DB Populated.");
            } catch {
                Database.godmode = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("GodMode DB Created.");
            }
        }
    }
}

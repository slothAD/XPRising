using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenRPG.Utils;
using ProjectM;
using VampireCommandFramework;

namespace OpenRPG.Commands
{

    
    public static class GodMode
    {
        [Command("godmode", shortHand: "gm", adminOnly: false, usage: "", description: "Toggles god mode.")]
        public static void Initialize(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isGodMode = Database.godmode.TryGetValue(SteamID, out bool isGodMode_);
            if (isGodMode) isGodMode = false;
            else isGodMode = true;
            UpdateGodMode(ctx, isGodMode);
            string s = isGodMode ? "Activated" : "Deactivated";
            ctx.Reply($"God mode <color=#ffff00>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.Buff.Buff_VBlood_Perk_Moose);
        }

        public static bool UpdateGodMode(ChatCommandContext ctx, bool isGodMode)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.godmode.TryGetValue(SteamID, out bool isGodMode_);
            if (isExist || !isGodMode) RemoveGodMode(ctx);
            else Database.godmode.Add(SteamID, isGodMode);
            return true;
        }

        public static void SaveGodMode()
        {
            File.WriteAllText(Plugin.GodModeJson, JsonSerializer.Serialize(Database.godmode, Database.JSON_options));
        }

        public static bool RemoveGodMode(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Database.godmode.TryGetValue(SteamID, out bool isGodMode_))
            {
                Database.godmode.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadGodMode()
        {

            if (!Directory.Exists(Plugin.ConfigPath)) Directory.CreateDirectory(Plugin.ConfigPath);
            if (!Directory.Exists(Plugin.SavesPath)) Directory.CreateDirectory(Plugin.SavesPath);

            if (!File.Exists(Plugin.GodModeJson))
            {
                var stream = File.Create(Plugin.GodModeJson);
                stream.Dispose();
            }
            string json = File.ReadAllText(Plugin.GodModeJson);
            try
            {
                Database.godmode = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("GodMode DB Populated.");
            }
            catch
            {
                Database.godmode = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("GodMode DB Created.");
            }
        }
    }
}

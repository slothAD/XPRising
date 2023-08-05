using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenRPG.Utils;
using ProjectM;
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
            ctx.Reply( $"Sun Immunity <color=#ffff00>{s}</color>");
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
            File.WriteAllText(Plugin.SunImmunityJson, JsonSerializer.Serialize(Database.sunimmunity, Database.JSON_options));
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

        public static void LoadSunImmunity()
        {

            if (!Directory.Exists(Plugin.ConfigPath)) Directory.CreateDirectory(Plugin.ConfigPath);
            if (!Directory.Exists(Plugin.SavesPath)) Directory.CreateDirectory(Plugin.SavesPath);

            if (!File.Exists(Plugin.SunImmunityJson))
            {
                var stream = File.Create(Plugin.SunImmunityJson);
                stream.Dispose();
            }

            string json = File.ReadAllText(Plugin.SunImmunityJson);
            try
            {
                Database.sunimmunity = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("SunImmunity DB Populated.");
            }
            catch
            {
                Database.sunimmunity = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("SunImmunity DB Created.");
            }
        }
    }
}

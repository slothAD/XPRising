using OpenRPG.Utils;
using ProjectM;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    [CommandGroup("rpg")]
    public static class NoCooldown
    {
        [Command("nocooldown", usage:"nocooldown", description: "Toggles instant cooldown for all abilities.")]
        public static void NoCooldownCommand(ChatCommandContext ctx)
        {
            Entity PlayerCharacter = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isNoCD = Database.nocooldownlist.TryGetValue(SteamID, out bool isNoCooldown_);
            if (isNoCD) isNoCD = false;
            else isNoCD = true;
            UpdateCooldownList(ctx, isNoCD);
            string p = isNoCD ? "Activated" : "Deactivated";
            ctx.Reply($"No Cooldown is now <color=#ffff00>{p}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.Buff.Buff_VBlood_Perk_Moose);
        }

        public static bool UpdateCooldownList(ChatCommandContext ctx, bool isNoCooldown)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.nocooldownlist.TryGetValue(SteamID, out bool isNoCooldown_);
            if (isExist || !isNoCooldown) RemoveCooldown(ctx);
            else Database.nocooldownlist.Add(SteamID, isNoCooldown);
            return true;
        }

        public static void SaveCooldown()
        {
            File.WriteAllText(Plugin.NoCooldownJson, JsonSerializer.Serialize(Database.nocooldownlist, Database.JSON_options));
        }

        public static bool RemoveCooldown(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Database.nocooldownlist.TryGetValue(SteamID, out bool isNoCooldown_))
            {
                Database.nocooldownlist.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadNoCooldown()
        {
            if (!Directory.Exists(Plugin.ConfigPath)) Directory.CreateDirectory(Plugin.ConfigPath);
            if (!Directory.Exists(Plugin.SavesPath)) Directory.CreateDirectory(Plugin.SavesPath);

            if (!File.Exists(Plugin.NoCooldownJson))
            {
                var stream = File.Create(Plugin.NoCooldownJson);
                stream.Dispose();
            }
            string json = File.ReadAllText(Plugin.NoCooldownJson);
            try
            {
                Database.nocooldownlist = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("NoCooldown DB Populated.");
            }
            catch
            {
                Database.nocooldownlist = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("NoCooldown DB Created.");
            }
        }
    }
}
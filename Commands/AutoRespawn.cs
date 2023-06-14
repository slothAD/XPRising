using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using VampireCommandFramework;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;

namespace RPGMods.Commands
{
    // Disabling as auto-respawn does not yet work
    //[CommandGroup("autorespawn","ar")]
    public static class AutoRespawn
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        // Disabling as auto-respawn does not yet work
        //[Command("toggle", "t", "[playerName]", "Toggle auto respawn on the same position on death", adminOnly: true)]
        public static void Initialize(ChatCommandContext ctx, string targetName)
        {
            var user = ctx.Event.User;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            bool isServerWide = false;

            bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "autorespawn_args");
            if (isAllowed)
            {
                if (targetName.ToLower().Equals("all"))
                {
                    SteamID = 1;
                    isServerWide = true;
                }
                else
                {
                    if (Helper.FindPlayer(targetName, false, out Entity targetEntity, out Entity targetUserEntity))
                    {
                        var user_component = entityManager.GetComponentData<User>(targetUserEntity);
                        SteamID = user_component.PlatformId;
                    }
                    else
                    {
                        ctx.Reply($"Player \"{targetName}\" not found!");
                        return;
                    }
                }
            }
            bool isAutoRespawn = Database.autoRespawn.ContainsKey(SteamID);
            if (isAutoRespawn) isAutoRespawn = false;
            else isAutoRespawn = true;
            UpdateAutoRespawn(SteamID, isAutoRespawn);
            string s = isAutoRespawn ? "Activated" : "Deactivated";
            if (isServerWide)
            {
                ctx.Reply($"Server wide Auto Respawn <color=#ffff00>{s}</color>");
            }
            else
            {
                ctx.Reply($"Player \"{targetName}\" Auto Respawn <color=#ffff00>{s}</color>");
            }
        }

        public static bool UpdateAutoRespawn(ulong SteamID, bool isAutoRespawn)
        {
            bool isExist = Database.autoRespawn.ContainsKey(SteamID);
            if (isExist || !isAutoRespawn) RemoveAutoRespawn(SteamID);
            else Database.autoRespawn.Add(SteamID, isAutoRespawn);
            return true;
        }

        public static void SaveAutoRespawn()
        {
            File.WriteAllText(AutoSaveSystem.mainSaveFolder+"autorespawn.json", JsonSerializer.Serialize(Database.autoRespawn, Database.JSON_options));
        }

        public static bool RemoveAutoRespawn(ulong SteamID)
        {
            if (Database.autoRespawn.ContainsKey(SteamID))
            {
                Database.autoRespawn.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadAutoRespawn()
        {
            if (!File.Exists(AutoSaveSystem.mainSaveFolder+"autorespawn.json"))
            {
                var stream = File.Create(AutoSaveSystem.mainSaveFolder+"autorespawn.json");
                stream.Dispose();
            }
            string json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+"autorespawn.json");
            try
            {
                Database.autoRespawn = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("AutoRespawn DB Populated.");
            }
            catch
            {
                Database.autoRespawn = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("AutoRespawn DB Created.");
            }
        }
    }
}

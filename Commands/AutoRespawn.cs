using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using VampireCommandFramework;
using Bloodstone.API;
using static VCF.Core.Basics.RoleCommands;

namespace OpenRPG.Commands
{

    [CommandGroup("rpg")]
    public static class AutoRespawn
    {
        [Command("autorespawn", usage: "autorespawn [<PlayerName>]", description: "Toggle auto respawn on the same position on death.", adminOnly: true)]
        public static void AutoRespawnCommand(ChatCommandContext ctx, string playerName)
        {
            var entityManager = VWorld.Server.EntityManager;
            ulong SteamID = ctx.Event.User.PlatformId;
            string PlayerName = ctx.Event.User.CharacterName.ToString();
            bool isServerWide = false;

            bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "autorespawn_args");
            if (playerName.Length > 0)
            {
                if (playerName.ToLower().Equals("all"))
                {
                    SteamID = 1;
                    isServerWide = true;
                }
                else
                {
                    if (Helper.FindPlayer(playerName, false, out Entity targetEntity, out Entity targetUserEntity))
                    {
                        var user_component = entityManager.GetComponentData<ProjectM.Network.User>(targetUserEntity);
                        SteamID = user_component.PlatformId;
                        PlayerName = playerName;
                    }
                    else
                    {
                        throw ctx.Error($"Player \"{playerName}\" not found!");
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
                ctx.Reply($"Player \"{PlayerName}\" Auto Respawn <color=#ffff00>{s}</color>");
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
            File.WriteAllText("BepInEx/config/OpenRPG/Saves/autorespawn.json", JsonSerializer.Serialize(Database.autoRespawn, Database.JSON_options));
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
            if (!Directory.Exists(Plugin.ConfigPath)) Directory.CreateDirectory(Plugin.ConfigPath);
            if (!Directory.Exists(Plugin.SavesPath)) Directory.CreateDirectory(Plugin.SavesPath);

            if (!File.Exists(Plugin.AutorespawnJson))
            {
                var stream = File.Create(Plugin.AutorespawnJson);
                stream.Dispose();
            }
            string json = File.ReadAllText(Plugin.AutorespawnJson);
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

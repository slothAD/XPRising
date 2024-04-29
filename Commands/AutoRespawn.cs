using OpenRPG.Systems;
using OpenRPG.Utils;
using Unity.Entities;
using VampireCommandFramework;
using Bloodstone.API;

namespace OpenRPG.Commands
{

    public static class AutoRespawn
    {
        [Command(name: "autorespawn", shortHand: "aresp", adminOnly: false, usage: ".autorespawn [<PlayerName>]", description: "Toggle auto respawn on the same position on death.")]
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

        public static bool RemoveAutoRespawn(ulong SteamID)
        {
            if (Database.autoRespawn.ContainsKey(SteamID))
            {
                Database.autoRespawn.Remove(SteamID);
                return true;
            }
            return false;
        }
    }
}

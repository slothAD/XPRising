using System.Collections.Generic;
using OpenRPG.Utils;
using Unity.Entities;
using VampireCommandFramework;
using Bloodstone.API;

namespace OpenRPG.Commands
{
    public static class AutoRespawn
    {
        [Command(name: "autorespawn", shortHand: "autor", adminOnly: true, usage: "[PlayerName]", description: "Toggle auto respawn on the same position on death for yourself or a player.")]
        public static void AutoRespawnCommand(ChatCommandContext ctx, string playerName)
        {
            var entityManager = VWorld.Server.EntityManager;
            var steamID = ctx.Event.User.PlatformId;
            var PlayerName = ctx.Event.User.CharacterName.ToString(); // Default to current player name

            if (playerName.Length > 0)
            {
                if (Helper.FindPlayer(playerName, false, out Entity targetEntity, out Entity targetUserEntity))
                {
                    var user_component = entityManager.GetComponentData<ProjectM.Network.User>(targetUserEntity);
                    steamID = user_component.PlatformId;
                    PlayerName = playerName;
                }
                else
                {
                    throw ctx.Error($"Player \"{playerName}\" not found!");
                }
            }
            
            var s = ToggleAutoRespawn(steamID) ? "Activated" : "Deactivated";
            ctx.Reply($"Player \"{PlayerName}\" Auto Respawn <color=#ffff00>{s}</color>");
        }
        
        [Command(name: "autorespawn-all", shortHand: "autor-all", adminOnly: true, usage: "", description: "Toggle auto respawn on the same position on death for all players.")]
        public static void AutoRespawnAllCommand(ChatCommandContext ctx)
        {
            ulong SteamID = 1;

            var s = ToggleAutoRespawn(SteamID) ? "Activated" : "Deactivated";
            ctx.Reply($"Server wide Auto Respawn <color=#ffff00>{s}</color>");
        }

        private static bool ToggleAutoRespawn(ulong SteamID)
        {
            var currentValue = Database.autoRespawn.GetValueOrDefault(SteamID, false);
            if (currentValue) Database.autoRespawn.Remove(SteamID);
            else Database.autoRespawn.Add(SteamID, true);

            return !currentValue;
        }
    }
}

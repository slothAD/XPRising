using RPGMods.Systems;
using RPGMods.Utils;
using System;
using System.Linq;
using Unity.Entities;
using VampireCommandFramework;
using Color = RPGMods.Utils.Color;
using Faction = RPGMods.Utils.Prefabs.Faction;

namespace RPGMods.Commands
{
    [CommandGroup("wanted", "w")]
    public static class Wanted{
        private static void SendFactionWantedMessage(PlayerHeatData heatData, Entity userEntity, bool isDebug) {
            bool isWanted = false;
            foreach (Faction faction in FactionHeat.ActiveFactions) {
                var heat = heatData.heat[faction];
                // Don't display this faction's heat level if the wanted level is 0.
                if (heat.level < FactionHeat.HeatLevels[0] && !isDebug) continue;
                isWanted = true;
                
                Output.SendLore(userEntity, FactionHeat.GetFactionStatus(faction, heat.level));
                
                if (isDebug)
                {
                    var sinceAmbush = DateTime.Now - heat.lastAmbushed;
                    var nextAmbush = Math.Max((int)(HunterHuntedSystem.ambush_interval - sinceAmbush.TotalSeconds), 0);
                    Output.SendLore(userEntity, $"Level: {Color.White(heat.level.ToString())} Possible ambush in {Color.White(nextAmbush.ToString())}s Chance: {Color.White(HunterHuntedSystem.ambush_chance.ToString())}%");
                }
            }

            if (!isWanted) {
                Output.SendLore(userEntity, "No active wanted levels");
            }
        }
        
        [Command("get","g", "", "Shows your current wanted level", adminOnly: false)]
        public static void GetWanted(ChatCommandContext ctx){
            if (!HunterHuntedSystem.isActive){
                ctx.Reply("HunterHunted system is not enabled.");
                return;
            }
            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            
            var heatData = HunterHuntedSystem.GetPlayerHeat(userEntity);
            SendFactionWantedMessage(heatData, userEntity, user.IsAdmin && HunterHuntedSystem.factionLogging);
        }

        [Command("set","s", "[name, faction, value]", "Sets the current wanted level", adminOnly: true)]
        public static void SetWanted(ChatCommandContext ctx, string name, string faction, int value) {
            bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "wanted_args");
            if (isAllowed){
                var contextUserEntity = ctx.Event.SenderUserEntity;
                
                if (!Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                {
                    ctx.Reply($"Could not find specified player \"{name}\".");
                    return;
                }

                if (!Enum.TryParse(faction, true, out Faction heatFaction) || !FactionHeat.ActiveFactions.Contains(heatFaction)) {
                    var supportedFactions = String.Join(", ", FactionHeat.ActiveFactions);
                    ctx.Reply($"Faction not yet supported. Supported factions: {supportedFactions}");
                    return;
                }

                var heatLevel = value == 0
                    ? 0
                    : FactionHeat.HeatLevels[Math.Clamp(value - 1, 0, FactionHeat.HeatLevels.Length - 1)] + 10;

                // Set wanted level and reset last ambushed so the user can be ambushed from now (ie, greater than ambush_interval seconds ago) 
                var updatedHeatData = HunterHuntedSystem.SetPlayerHeat(
                    targetUserEntity,
                    heatFaction,
                    heatLevel,
                    DateTime.Now - TimeSpan.FromSeconds(HunterHuntedSystem.ambush_interval + 1));
                
                ctx.Reply($"Player \"{name}\" wanted value changed.");
                SendFactionWantedMessage(updatedHeatData, contextUserEntity, HunterHuntedSystem.factionLogging);
                if (!targetUserEntity.Equals(contextUserEntity)) {
                    SendFactionWantedMessage(updatedHeatData, targetUserEntity, false);
                }
            }
        }
        
        [Command("log", "l", "<On, Off>", "Turns on or off logging of heat data.", adminOnly: true)]
        public static void LogWanted(ChatCommandContext ctx, string flag){
            if (!HunterHuntedSystem.isActive){
                ctx.Reply("Wanted system is not enabled.");
                return;
            }

            var userEntity = ctx.Event.SenderUserEntity;
            switch (flag.ToLower()) {
                case "on":
                    HunterHuntedSystem.SetLogging(userEntity, true);
                    ctx.Reply("Heat levels now being logged.");
                    return;
                case "off":
                    HunterHuntedSystem.SetLogging(userEntity, true);
                    ctx.Reply($"Heat levels are no longer being logged.");
                    break;
            }
        }

        [Command("trigger","t", "[name]", "Triggers the ambush check for the given user", adminOnly: true)]
        public static void TriggerAmbush(ChatCommandContext ctx, string name) {
            var isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "wanted_args");
            if (!isAllowed) return;
                
            if (!Helper.FindPlayer(name, true, out var playerEntity, out var userEntity))
            {
                ctx.Reply($"Could not find specified player \"{name}\".");
                return;
            }
            
            HunterHuntedSystem.CheckForAmbush(userEntity, playerEntity);
            ctx.Reply($"Successfully triggered ambush check for \"{name}\"");
        }
    }
}

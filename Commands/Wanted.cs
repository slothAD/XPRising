using OpenRPG.Systems;
using OpenRPG.Utils;
using System;
using System.Linq;
using ProjectM;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using VampireCommandFramework;
using Color = OpenRPG.Utils.Color;
using Faction = OpenRPG.Utils.Prefabs.Faction;

namespace OpenRPG.Commands
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
        
        [Command("log", "l", "<On, Off>", "Turns on or off logging of heat data.", adminOnly: false)]
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
                
            if (!Helper.FindPlayer(name, true, out var playerEntity, out _))
            {
                ctx.Reply($"Could not find specified player \"{name}\".");
                return;
            }
            
            HunterHuntedSystem.CheckForAmbush(playerEntity);
            ctx.Reply($"Successfully triggered ambush check for \"{name}\"");
        }

        [Command("fixminions", "fm", "", "Remove broken gloomrot technician units", adminOnly: true)]
        public static void FixGloomrotMinions(ChatCommandContext ctx) {
            if (!ctx.Event.User.IsAdmin) return;

            var hasErrors = false;
            var removedCount = 0;

            var query = Plugin.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Minion>()
                },
                Options = EntityQueryOptions.IncludeDisabled
            });
            foreach (var entity in query.ToEntityArray(Allocator.Temp)) {
                try {
                    var unitName = Helper.GetPrefabName(Helper.GetPrefabGUID(entity));
                    // Note that the "broken" units differ from "working" units only by the broken ones missing the
                    // "PathRequestSolveDebugBuffer [B]" component. Ideally, we would only destroy minions missing the
                    // component, but we can't test for this case by any means other than checking the string generated
                    // by Plugin.Server.EntityManager.Debug.GetEntityInfo(entity). We can't test for this case as the
                    // GetBuffer or HasBuffer commands fail with an AOT code exception.
                    Plugin.Logger.LogInfo($"{DateTime.Now}: destroying minion {unitName}");
                    
                    DestroyUtility.CreateDestroyEvent(Plugin.Server.EntityManager, entity, DestroyReason.Default, DestroyDebugReason.None);
                    DestroyUtility.Destroy(Plugin.Server.EntityManager, entity);
                    removedCount++;
                }
                catch (Exception e) {
                    Plugin.Logger.LogInfo(DateTime.Now + ": error doing test other: " + e.Message);
                    hasErrors = true;
                }
            }

            if (hasErrors) {
                ctx.Reply($"Finished with errors (check logs). Removed {removedCount} units.");
            } else {
                ctx.Reply($"Finished successfully. Removed {removedCount} units.");
            }
        }
    }
}

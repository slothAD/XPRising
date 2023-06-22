using RPGMods.Systems;
using RPGMods.Utils;
using System;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;
using Color = RPGMods.Utils.Color;
using Faction = RPGMods.Utils.Prefabs.Faction;
using Random = System.Random;

namespace RPGMods.Commands
{
    [CommandGroup("wanted", "w")]
    public static class Wanted{
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        
        private static Random generate = new();
        
        [Command("get","g", "", "Shows your current wanted level", adminOnly: false)]
        public static void GetWanted(ChatCommandContext ctx){
            if (!HunterHuntedSystem.isActive){
                ctx.Reply("HunterHunted system is not enabled.");
                return;
            }
            var user = ctx.Event.User;
            var userEntity = ctx.Event.SenderUserEntity;
            
            var heatData = HunterHuntedSystem.GetPlayerHeat(userEntity);

            foreach (Faction faction in FactionHeat.ActiveFactions) {
                var heat = heatData.heat[faction];
                Output.SendLore(userEntity, FactionHeat.GetFactionStatus(faction, heat.level));
                
                if (user.IsAdmin)
                {
                    var sinceAmbush = DateTime.Now - heat.lastAmbushed;
                    var nextAmbush = Math.Max((int)(HunterHuntedSystem.ambush_interval - sinceAmbush.TotalSeconds), 0);
                    Output.SendLore(userEntity, $"Level: {Color.White(heat.level.ToString())} Possible ambush in {Color.White(nextAmbush.ToString())}s Chance: {Color.White(HunterHuntedSystem.ambush_chance.ToString())}%");
                }
            }
        }

        [Command("set","s", "[name, faction, value]", "Sets the current wanted level", adminOnly: true)]
        public static void SetWanted(ChatCommandContext ctx, string name, string faction, int value) {
            bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "wanted_args");
            if (isAllowed){
                var userEntity = ctx.Event.SenderUserEntity;
                
                if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                {
                    userEntity = targetUserEntity;
                }
                else
                {
                    ctx.Reply($"Could not find specified player \"{name}\".");
                    return;
                }

                if (!Enum.TryParse(faction, true, out Faction heatFaction)) {
                    ctx.Reply("Faction not yet supported");
                    return;
                }

                // Set wanted level and reset last ambushed so the user can be ambushed from now (ie, greater than ambush_interval seconds ago) 
                var updatedHeatData = HunterHuntedSystem.SetPlayerHeat(
                    userEntity,
                    heatFaction,
                    value,
                    DateTime.Now - TimeSpan.FromSeconds(HunterHuntedSystem.ambush_interval + 1));
                
                ctx.Reply($"Player \"{name}\" wanted value changed.");
                ctx.Reply(updatedHeatData.ToString());
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

        [Command("spawn","sp", "[name, faction]", "Spawns the current wanted level on the user", adminOnly: true)]
        public static void SpawnFaction(ChatCommandContext ctx, string name, string faction) {
            var isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "wanted_args");
            if (!isAllowed) return;
            
            var userEntity = ctx.Event.SenderUserEntity;
                
            if (!Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
            {
                ctx.Reply($"Could not find specified player \"{name}\".");
                return;
            }

            if (!Enum.TryParse(faction, true, out Faction heatFaction)) {
                ctx.Reply("Faction not yet supported");
                return;
            }

            var heatData = HunterHuntedSystem.GetPlayerHeat(userEntity);

            var heat = heatData.heat[heatFaction];
            var wantedLevel = FactionHeat.GetWantedLevel(heat.level);
            if (wantedLevel < 1) {
                ctx.Reply("User wanted level too low to spawn ambushers");
                return;
            }
            
            // Update faction spawn time (or else as soon as they are in combat it might spawn more)
            HunterHuntedSystem.SetPlayerHeat(userEntity, heatFaction, heat.level, DateTime.Now);
            
            FactionHeat.Ambush(userEntity, targetUserEntity, heatFaction, wantedLevel);
        }
    }
}

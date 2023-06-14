using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using BepInEx;
using Unity.Entities;
using VampireCommandFramework;
using Color = RPGMods.Utils.Color;

namespace RPGMods.Commands
{
    [CommandGroup("heat", "ht")]
    public static class Heat{
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        
        [Command("get","g", "", "Shows your current wanted level", adminOnly: false)]
        public static void GetHeat(ChatCommandContext ctx){
            if (!HunterHuntedSystem.isActive){
                ctx.Reply("HunterHunted system is not enabled.");
                return;
            }
            var user = ctx.Event.User;
            var SteamID = user.PlatformId;
            var userEntity = ctx.Event.SenderUserEntity;

            HunterHuntedSystem.HeatManager(userEntity);

            // Note that the HeatManager call above should initialise the heat data for us.
            if (!Cache.heatCache.TryGetValue(SteamID, out var heatData)) return;

            
            foreach (FactionHeat.Faction faction in Enum.GetValues<FactionHeat.Faction>()) {
                var heat = heatData.heat[faction];
                Output.SendLore(userEntity, FactionHeat.GetFactionStatus(faction, heat.level));
                
                if (user.IsAdmin)
                {
                    TimeSpan since_ambush = DateTime.Now - heat.lastAmbushed;
                    int NextAmbush = (int)(HunterHuntedSystem.ambush_interval - since_ambush.TotalSeconds);
                    if (NextAmbush < 0) NextAmbush = 0;

                    ctx.Reply($"Next Possible Ambush in {Color.White(NextAmbush.ToString())}s");
                    ctx.Reply($"Ambush Chance: {Color.White(HunterHuntedSystem.ambush_chance.ToString())}%");
                    ctx.Reply(heatData.ToString());
                }
            }
        }
        
        [Command("test","t", "", "Displays additional heat information", adminOnly: true)]
        public static void TestHeat(ChatCommandContext ctx){
            if (!HunterHuntedSystem.isActive){
                ctx.Reply("HunterHunted system is not enabled.");
                return;
            }
            var user = ctx.Event.User;
            var SteamID = user.PlatformId;
            var userEntity = ctx.Event.SenderUserEntity;

            
        }

        [Command("set","s", "[name, faction, value]", "Sets the current wanted level", adminOnly: true)]
        public static void SetHeat(ChatCommandContext ctx, string name, string faction, int value) {
            bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "heat_args");
            if (isAllowed && !name.IsNullOrWhiteSpace()){
                var user = ctx.Event.User;
                var SteamID = user.PlatformId;
                var userEntity = ctx.Event.SenderUserEntity;
                
                if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                {
                    SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                    userEntity = targetUserEntity;
                }
                else
                {
                    ctx.Reply($"Could not find specified player \"{name}\".");
                    return;
                }

                if (!Enum.TryParse(faction, true, out FactionHeat.Faction heatFaction)) {
                    ctx.Reply("Faction not yet supported");
                    return;
                }

                if (!Cache.heatCache.TryGetValue(SteamID, out PlayerHeatData heatData)) {
                    heatData = new PlayerHeatData();
                }

                // Update faction heat
                var heat = heatData.heat[heatFaction];
                heat.level = value;
                heatData.heat[heatFaction] = heat;
                Cache.heatCache[SteamID] = heatData;
                
                ctx.Reply($"Player \"{name}\" heat value changed.");
                ctx.Reply(heatData.ToString());
                HunterHuntedSystem.HeatManager(userEntity);
            }
        }
    }
}

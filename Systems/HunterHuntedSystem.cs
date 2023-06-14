using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using System;
using System.Threading.Tasks;
using Unity.Entities;

namespace RPGMods.Systems
{
    public class HunterHuntedSystem
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        public static bool isActive = true;
        public static int heat_cooldown = 35;
        public static int cooldown_timer = 60;
        public static int ambush_interval = 300;
        public static int ambush_chance = 50;
        public static float ambush_despawn_timer = 300;

        private static Random rand = new();

        public static void PlayerUpdateHeat(Entity killerEntity, Entity victimEntity)
        {
            var player = entityManager.GetComponentData<PlayerCharacter>(killerEntity);
            var userEntity = player.UserEntity;
            var user = entityManager.GetComponentData<User>(userEntity);
            var SteamID = user.PlatformId;

            var victim = entityManager.GetComponentData<FactionReference>(victimEntity);
            var victimFaction = victim.FactionGuid._Value;
            if (Database.faction_heatvalue.TryGetValue(victimFaction, out int heatValue)) {
                if (!Cache.heatCache.TryGetValue(SteamID, out var heatData)) return;
                
                if (victimFaction.GetHashCode() == -413163549) //-- Separate bandit heat level
                {
                    var banditHeatValue = rand.Next(1, 10);
                    var heatBandit = heatData.heat[FactionHeat.Faction.Bandit];
                    heatBandit.level += banditHeatValue;
                    heatData.heat[FactionHeat.Faction.Bandit] = heatBandit;
                }

                var heatHuman = heatData.heat[FactionHeat.Faction.Human];
                heatHuman.level += heatValue;
                heatData.heat[FactionHeat.Faction.Human] = heatHuman;
                Cache.heatCache[SteamID] = heatData;
            }
        }

        public static void CheckForAmbush(Entity userEntity, Entity playerEntity, bool inCombat) {
            var SteamID = entityManager.GetComponentData<User>(userEntity).PlatformId;

            Cache.heatCache.TryGetValue(SteamID, out var heatData);

            foreach (var faction in Enum.GetValues<FactionHeat.Faction>()) {
                var heat = heatData.heat[faction];
                TimeSpan timeSinceAmbush = DateTime.Now - heat.lastAmbushed;

                if (timeSinceAmbush.TotalSeconds > ambush_interval) {
                    if (rand.Next(0, 100) <= ambush_chance && inCombat) {
                        FactionHeat.Ambush(userEntity, playerEntity, faction, heat.level, rand);
                        heat.lastAmbushed = DateTime.Now;
                        heatData.heat[faction] = heat;
                    }
                }
            }
            
            Cache.heatCache[SteamID] = heatData;
        }

        public static void HeatManager(Entity userEntity)
        {
            var SteamID = entityManager.GetComponentData<User>(userEntity).PlatformId;

            if (!Cache.heatCache.TryGetValue(SteamID, out var heatData)) {
                Cache.heatCache[SteamID] = new PlayerHeatData();
            }

            var elapsedTime = DateTime.Now - heatData.lastUpdate;
            if (!(elapsedTime.TotalSeconds > cooldown_timer)) return;
            
            var heatTicks = (int)elapsedTime.TotalSeconds / cooldown_timer;
            if (heatTicks < 0) heatTicks = 0;

            // Update all heat levels
            foreach (var faction in Enum.GetValues<FactionHeat.Faction>()) {
                var heat = heatData.heat[faction];
                var newHeatLevel = Math.Max(heat.level - heat_cooldown * heatTicks, 0);
                heat.level = newHeatLevel;
                heatData.heat[faction] = heat;
            }

            heatData.lastUpdate = DateTime.Now;
                
            // Store updated heatData
            Cache.heatCache[SteamID] = heatData;
        }
    }
}

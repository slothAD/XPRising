using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using System;
using System.Linq;
using System.Text;
using Unity.Entities;
using Faction = RPGMods.Utils.Prefabs.Faction;

namespace RPGMods.Systems
{
    public static class HunterHuntedSystem {
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        public static bool isActive = true;
        public static int heat_cooldown = 10;
        public static int ambush_interval = 60;
        public static int ambush_chance = 50;
        public static float ambush_despawn_timer = 300;

        public static bool factionLogging = false;

        private static Random rand = new();

        public static void PlayerKillEntity(Entity killerEntity, Entity victimEntity) {
            var player = entityManager.GetComponentData<PlayerCharacter>(killerEntity);
            var userEntity = player.UserEntity;

            var victim = entityManager.GetComponentData<FactionReference>(victimEntity);
            var victimFaction = victim.FactionGuid._Value;

            var faction = Helper.ConvertGuidToFaction(victimFaction);
            if (factionLogging || faction == Faction.Unknown) {
                var factionString = $"{DateTime.Now}: Player killed: Entity: {Helper.GetPrefabGUID(victimEntity).GetHashCode()} Faction: {Enum.GetName(faction)}";
                Plugin.Logger.LogWarning(factionString);
            }

            bool isVBlood;
            if (entityManager.HasComponent<BloodConsumeSource>(victimEntity)) {
                BloodConsumeSource BloodSource = entityManager.GetComponentData<BloodConsumeSource>(victimEntity);
                isVBlood = BloodSource.UnitBloodType.Equals(Helper.vBloodType);
            }
            else {
                isVBlood = false;
            }

            FactionHeat.GetActiveFactionHeatValue(faction, isVBlood, out var heatValue, out var activeFaction);
            if (activeFaction == Faction.Unknown || heatValue == 0) return;
            
            HeatManager(userEntity, out var heatData, out var steamID);

            // If the faction is vampire hunters, reduce the heat level of all other active factions
            if (activeFaction == Faction.VampireHunters) {
                foreach (var (key, value) in heatData.heat) {
                    var heat = value;
                    var oldHeatLevel = FactionHeat.GetWantedLevel(heat.level);
                    heat.level = Math.Max(0, heat.level - heatValue);
                    var newHeatLevel = FactionHeat.GetWantedLevel(heat.level);
                    heatData.heat[key] = heat;

                    if (newHeatLevel < oldHeatLevel) {
                        // User has decreased in wanted level
                        Output.SendLore(userEntity,
                            $"Wanted level decreased ({FactionHeat.GetFactionStatus(key, heat.level)})");
                    }
                }
            }
            else {
                if (!heatData.heat.TryGetValue(activeFaction, out var heat)) {
                    Plugin.Logger.LogWarning(
                        $"{DateTime.Now}: Attempted to load non-active faction heat data: {Enum.GetName(activeFaction)}");
                    return;
                }

                // Update the heat value for this faction
                var randHeatValue = rand.Next(1, heatValue);
                var oldHeatLevel = FactionHeat.GetWantedLevel(heat.level);
                heat.level += randHeatValue;
                var newHeatLevel = FactionHeat.GetWantedLevel(heat.level);

                if (newHeatLevel > oldHeatLevel) {
                    // User has increased in wanted level, so send them an ominous message
                    Output.SendLore(userEntity,
                        $"Wanted level increased ({FactionHeat.GetFactionStatus(activeFaction, heat.level)})");
                    // and reset their last ambushed time so that they can be ambushed again
                    heat.lastAmbushed = DateTime.Now - TimeSpan.FromSeconds(ambush_interval);
                }
                
                heatData.heat[activeFaction] = heat;
            }

            // Update the heatCache with the new data
            Cache.heatCache[steamID] = heatData;

            LogHeatData(heatData, userEntity, "kill");
        }

        public static void PlayerDied(Entity victimEntity) {
            var player = entityManager.GetComponentData<PlayerCharacter>(victimEntity);
            var userEntity = player.UserEntity;
            var user = entityManager.GetComponentData<User>(userEntity);
            var steamID = user.PlatformId;

            // Reset player heat to 0
            var heatData = new PlayerHeatData();
            Cache.heatCache[steamID] = heatData;
            LogHeatData(heatData, userEntity, "died");
        }

        // This is expected to only be called at the start of combat
        public static void CheckForAmbush(Entity userEntity, Entity playerEntity) {
            HeatManager(userEntity, out var heatData, out var steamID);

            foreach (var faction in FactionHeat.ActiveFactions) {
                var heat = heatData.heat[faction];
                TimeSpan timeSinceAmbush = DateTime.Now - heat.lastAmbushed;
                var wantedLevel = FactionHeat.GetWantedLevel(heat.level);

                if (timeSinceAmbush.TotalSeconds > ambush_interval && wantedLevel > 0) {
                    if (factionLogging) Plugin.Logger.LogInfo($"{DateTime.Now}: {faction} can ambush");
                    if (rand.Next(0, 100) <= ambush_chance) {
                        if (heatData.isLogging) Output.SendLore(userEntity, $"{faction} is ambushing you!");
                        if (factionLogging) Plugin.Logger.LogInfo($"{DateTime.Now}: {faction} is ambushing");
                        FactionHeat.Ambush(userEntity, playerEntity, faction, wantedLevel);
                        heat.lastAmbushed = DateTime.Now;
                        heatData.heat[faction] = heat;
                    }
                }
            }

            Cache.heatCache[steamID] = heatData;
            LogHeatData(heatData, userEntity, "check");
        }

        public static PlayerHeatData GetPlayerHeat(Entity userEntity) {
            // Ensure that the user has the up-to-date heat data and return the value
            HeatManager(userEntity, out var heatData, out var steamID);
            LogHeatData(heatData, userEntity, "get");
            return heatData;
        }

        public static PlayerHeatData SetPlayerHeat(Entity userEntity, Faction heatFaction, int value, DateTime lastAmbushed) {
            HeatManager(userEntity, out var heatData, out var steamID);

            // Update faction heat
            var heat = heatData.heat[heatFaction];
            heat.level = value;
            heat.lastAmbushed = lastAmbushed;
            heatData.heat[heatFaction] = heat;

            Cache.heatCache[steamID] = heatData;
            LogHeatData(heatData, userEntity, "set");
            return heatData;
        }

        public static void SetLogging(Entity userEntity, bool on) {
            HeatManager(userEntity, out var heatData, out var steamID);
            heatData.isLogging = on;
            Cache.heatCache[steamID] = heatData;
            LogHeatData(heatData, userEntity, $"log({on})");
        }

        // Encodes the unit level into the lifetime
        // This uses the decimal portion of the float value to encode the value into the lifetime.
        public static float EncodeLifetime(int level) {
            level = Math.Clamp(level, 0, 99);
            // Adds level and level "checksum" - this assumes a level cap of 99
            return ambush_despawn_timer + (level / 100f) + (level / 10_000f);
        }

        // Decodes a unit level out of the lifetime
        // Returns whether true if the level decodes correctly
        public static bool DecodeLifetime(float lifetime, out int level) {
            if (lifetime >= ambush_despawn_timer && lifetime < ambush_despawn_timer + 1) {
                var encoded = (lifetime - ambush_despawn_timer) * 100;
                // This works better than rounding as this value will always be slightly above the expected level due to the checksum
                level = (int)encoded;
                var levelChecksum = (int)Math.Round((encoded - level) * 100);
                return level == levelChecksum;
            }
            level = 0;
            return false;
        }

        private static void HeatManager(Entity userEntity, out PlayerHeatData heatData, out ulong steamID) {
            steamID = entityManager.GetComponentData<User>(userEntity).PlatformId;
            var cooldownPerSecond = (double)heat_cooldown / 60;

            if (!Cache.heatCache.TryGetValue(steamID, out heatData)) {
                heatData = new PlayerHeatData();
            }
            
            // If there has been last than 5 seconds the last heat manager update, just skip calculations
            if ((DateTime.Now - heatData.lastCooldown).TotalSeconds < 5) return;

            var lastCombatStart = Cache.GetCombatStart(steamID);
            var lastCombatEnd = Cache.GetCombatEnd(steamID);

            var elapsedTime = CooldownPeriod(heatData.lastCooldown, lastCombatStart, lastCombatEnd);
            if (factionLogging) Plugin.Logger.LogInfo($"{DateTime.Now} Heat CD period: {elapsedTime:F1}s (L:{heatData.lastCooldown}|S:{lastCombatStart}|E:{lastCombatEnd})");

            if (elapsedTime > 0) {
                var cooldownValue = (int)Math.Floor(elapsedTime * cooldownPerSecond);
                if (factionLogging) Plugin.Logger.LogInfo($"{DateTime.Now} Heat cooldown: {cooldownValue} ({cooldownPerSecond:F1}c/s)");

                // Update all heat levels
                foreach (var faction in FactionHeat.ActiveFactions) {
                    var heat = heatData.heat[faction];
                    var newHeatLevel = Math.Max(heat.level - cooldownValue, 0);
                    heat.level = newHeatLevel;
                    heatData.heat[faction] = heat;
                }
            }

            heatData.lastCooldown = DateTime.Now;
                
            // Store updated heatData
            Cache.heatCache[steamID] = heatData;
        }

        private static double CooldownPeriod(DateTime lastCooldown, DateTime lastCombatStart, DateTime lastCombatEnd) {
            // By default, the cooldown period is from the lastCooldown to Now. 
            var cdPeriodStart = lastCooldown;
            var cdPeriodEnd = DateTime.Now;
            
            var inCombat = lastCombatStart > lastCombatEnd;
            if (factionLogging) Plugin.Logger.LogInfo($"{DateTime.Now} Heat CD period: combat: {inCombat}");
            if (inCombat) {
                // If we are in combat, cdPeriodEnd is the start of combat;
                cdPeriodEnd = lastCombatStart;
            }
            
            // cdPeriodStart is the max of (lastCooldown, lastCombatEnd + offset)
            var cdPeriodStartAfterCombat = lastCombatEnd + TimeSpan.FromSeconds(20);
            cdPeriodStart = lastCooldown > cdPeriodStartAfterCombat ? lastCooldown : cdPeriodStartAfterCombat;
            
            if (factionLogging) Plugin.Logger.LogInfo($"{DateTime.Now} Heat CD period: end before start: {cdPeriodEnd < cdPeriodStart}");
            // If cdPeriodEnd is earlier than cdPeriodStart, 0 seconds have elapsed in the cooldown period
            return cdPeriodEnd < cdPeriodStart ? 0 : (cdPeriodEnd - cdPeriodStart).TotalSeconds;
        }

        private static string HeatDataString(PlayerHeatData heatData, bool useColor) {
            var activeHeat =
                heatData.heat.Where(faction => faction.Value.level > 0)
                    .Select(faction =>
                        useColor ? $"{Enum.GetName(faction.Key)}: {Color.White(faction.Value.level.ToString())}" :
                            $"{Enum.GetName(faction.Key)}: {faction.Value.level.ToString()}"
                        )
                    .DefaultIfEmpty("All heat levels 0");
            var sb = new StringBuilder();
            sb.AppendJoin(" | ", activeHeat);
            return sb.ToString();
        }

        private static void LogHeatData(PlayerHeatData heatData, Entity userEntity, string origin) {
            if (heatData.isLogging) Output.SendLore(userEntity, HeatDataString(heatData, true));
            if (factionLogging) Plugin.Logger.LogInfo($"{DateTime.Now} Heat({origin}): {HeatDataString(heatData, false)}");
        }
    }
}

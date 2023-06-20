using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using System;
using System.Linq;
using System.Text;
using Unity.Entities;
using Faction = RPGMods.Utils.Faction;

namespace RPGMods.Systems
{
    public static class HunterHuntedSystem
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        public static bool isActive = true;
        public static int heat_cooldown = 10;
        public static int ambush_interval = 60;
        public static int ambush_chance = 50;
        public static float ambush_despawn_timer = 300;

        public static bool isLogging = false;
        public static bool IsDebugging = false;

        private static Random rand = new();
        // TODO put somewhere common
        private static readonly PrefabGUID vBloodType = new(1557174542);

        public static void PlayerKillEntity(Entity killerEntity, Entity victimEntity)
        {
            var player = entityManager.GetComponentData<PlayerCharacter>(killerEntity);
            var userEntity = player.UserEntity;
            var user = entityManager.GetComponentData<User>(userEntity);
            var SteamID = user.PlatformId;

            var victim = entityManager.GetComponentData<FactionReference>(victimEntity);
            var victimFaction = victim.FactionGuid._Value;

            var faction = Faction.ConvertGuidToFaction(victimFaction);
            if (IsDebugging) {
                var factionString = faction == Faction.Type.Unknown
                    ? $"Unknown faction: {victimFaction.GetHashCode()}"
                    : Enum.GetName(faction);
                Output.SendLore(userEntity, $"Entity: {Helper.GetPrefabGUID(victimEntity).GetHashCode()} Faction: {factionString}");
            }
            else if (faction == Faction.Type.Unknown) {
                // Log somewhere just in case
                Plugin.Logger.LogWarning($"Entity: {Helper.GetPrefabGUID(victimEntity).GetHashCode()} Unknown faction: {victimFaction.GetHashCode()}");
            }
            
            bool isVBlood;
            if (entityManager.HasComponent<BloodConsumeSource>(victimEntity))
            {
                BloodConsumeSource BloodSource = entityManager.GetComponentData<BloodConsumeSource>(victimEntity);
                isVBlood = BloodSource.UnitBloodType.Equals(vBloodType);
            }
            else
            {
                isVBlood = false;
            }
            
            FactionHeat.GetActiveFactionHeatValue(faction, isVBlood, out var heatValue, out var activeFaction);
            if (activeFaction == Faction.Type.Unknown || heatValue == 0) return;
            if (!Cache.heatCache.TryGetValue(SteamID, out var heatData)) return;

            // If the faction is vampire hunters, reduce the heat level of all other active factions
            if (activeFaction == Faction.Type.VampireHunters) {
                foreach (var (key, value) in heatData.heat) {
                    var heat = value;
                    var oldHeatLevel = FactionHeat.GetWantedLevel(heat.level);
                    heat.level = Math.Max(0, heat.level - heatValue);
                    var newHeatLevel = FactionHeat.GetWantedLevel(heat.level);
                    heatData.heat[key] = heat;
                    
                    if (newHeatLevel < oldHeatLevel) {
                        // User has decreased in wanted level
                        Output.SendLore(userEntity, $"Wanted level decreased ({FactionHeat.GetFactionStatus(key, heat.level)})");
                    }
                }
            }
            else {
                if (!heatData.heat.TryGetValue(activeFaction, out var heat)) {
                    Plugin.Logger.LogWarning($"Attempted to load non-active faction heat data: {Enum.GetName(activeFaction)}");
                    return;
                }

                // Update the heat value for this faction
                var randHeatValue = rand.Next(1, heatValue);
                var oldHeatLevel = FactionHeat.GetWantedLevel(heat.level);
                heat.level += randHeatValue;
                var newHeatLevel = FactionHeat.GetWantedLevel(heat.level);
                heatData.heat[activeFaction] = heat;
                
                if (newHeatLevel > oldHeatLevel) {
                    // User has increased in wanted level, so send them an ominous message
                    Output.SendLore(userEntity, $"Wanted level increased ({FactionHeat.GetFactionStatus(activeFaction, heat.level)})");
                }
            }

            // Update the heatCache with the new data
            Cache.heatCache[SteamID] = heatData;

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

        public static void CheckForAmbush(Entity userEntity, Entity playerEntity, bool inCombat) {
            var heatData = HeatManager(userEntity);
            
            var steamID = entityManager.GetComponentData<User>(userEntity).PlatformId;

            foreach (var faction in FactionHeat.ActiveFactions) {
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
            
            Cache.heatCache[steamID] = heatData;
            LogHeatData(heatData, userEntity, "check");
        }

        public static PlayerHeatData GetPlayerHeat(Entity userEntity) {
            // Ensure that the user has the up-to-date heat data and return the value
            var heatData = HeatManager(userEntity);
            LogHeatData(heatData, userEntity, "get");
            return heatData;
        }

        public static PlayerHeatData SetPlayerHeat(Entity userEntity, Faction.Type heatFaction, int value, DateTime lastAmbushed) {
            var heatData = HeatManager(userEntity);
            var steamID = entityManager.GetComponentData<User>(userEntity).PlatformId;
            
            // Update faction heat
            var heat = heatData.heat[heatFaction];
            heat.level = value;
            heat.lastAmbushed = lastAmbushed;
            heatData.heat[heatFaction] = heat;
            
            Cache.heatCache[steamID] = heatData;
            LogHeatData(heatData, userEntity, "set");
            return heatData;
        }

        private static PlayerHeatData HeatManager(Entity userEntity) {
            var cooldownPerSecond = (double)heat_cooldown / 60;
            var steamID = entityManager.GetComponentData<User>(userEntity).PlatformId;

            if (!Cache.heatCache.TryGetValue(steamID, out var heatData)) {
                heatData = new PlayerHeatData();
            }

            var elapsedTime = DateTime.Now - heatData.lastCooldown;
            // Just return heat data without triggering faction cooldown if we elapsed cooldown time < 5 seconds
            if (!(elapsedTime.TotalSeconds > 5)) return heatData;
            
            var cooldownValue = (int)Math.Floor(elapsedTime.TotalSeconds * cooldownPerSecond);
            if (IsDebugging) Plugin.Logger.LogInfo($"{DateTime.Now} Heat cooldown: {cooldownValue} ({cooldownPerSecond:F1}c/s)");

            // Update all heat levels
            foreach (var faction in FactionHeat.ActiveFactions) {
                var heat = heatData.heat[faction];
                var newHeatLevel = Math.Max(heat.level - cooldownValue, 0);
                heat.level = newHeatLevel;
                heatData.heat[faction] = heat;
            }

            heatData.lastCooldown = DateTime.Now;
                
            // Store updated heatData
            Cache.heatCache[steamID] = heatData;
            return heatData;
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
            if (isLogging) Output.SendLore(userEntity, HeatDataString(heatData, true));
            if (IsDebugging) Plugin.Logger.LogInfo($"{DateTime.Now} Heat: {origin} {HeatDataString(heatData, false)}");
        }
    }
}

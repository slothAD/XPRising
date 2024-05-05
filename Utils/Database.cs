using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenRPG.Models;
using OpenRPG.Systems;
using Unity.Collections;
using Unity.Entities;

namespace OpenRPG.Utils
{
    using WeaponMasteryData = LazyDictionary<WeaponMasterySystem.MasteryType, MasteryData>;
    using BloodlineMasteryData = LazyDictionary<BloodlineSystem.BloodType, MasteryData>;
    public static class Cache
    {
        //-- Cache (Wiped on plugin reload, server restart, and shutdown.)

        //-- -- Player Cache
        public static Dictionary<FixedString64, PlayerData> NamePlayerCache = new();
        public static Dictionary<ulong, PlayerData> SteamPlayerCache = new();
        public static Dictionary<Entity, PlayerGroup> PlayerAllies = new();
        public static Dictionary<ulong, List<BuffData>> buffData = new();

        //-- -- Commands
        public static Dictionary<ulong, float> command_Cooldown = new();
        
        //-- -- Combat
        public static Dictionary<ulong, DateTime> playerCombatStart = new();
        public static Dictionary<ulong, DateTime> playerCombatEnd = new();

        //-- -- HunterHunted System
        public static Dictionary<ulong, PlayerHeatData> heatCache = new();

        //-- -- Mastery System
        public static Dictionary<ulong, DateTime> player_last_combat = new();
        public static Dictionary<ulong, int> player_combat_ticks = new();

        //-- -- Experience System
        public static Dictionary<ulong, float> player_level = new();
        public static Dictionary <ulong,Dictionary<UnitStatType, float>> player_geartypedonned = new();

        //-- -- CustomNPC Spawner
        public static SizedDictionaryAsync<float, SpawnNPCListen> spawnNPC_Listen = new(500);
        
        public static DateTime GetCombatStart(ulong steamID) {
            if (!playerCombatStart.TryGetValue(steamID, out var start)) {
                start = DateTime.MinValue;
            }

            return start;
        }
        public static DateTime GetCombatEnd(ulong steamID) {
            if (!playerCombatEnd.TryGetValue(steamID, out var start)) {
                start = DateTime.MinValue;
            }

            return start;
        }

        public static bool PlayerInCombat(ulong steamID)
        {
            return GetCombatStart(steamID) > GetCombatEnd(steamID);
        }
    }

    public static class Database
    {
        //-- Dynamic Database (Saved on a JSON file on plugin reload, server restart, and shutdown.)
        //-- Initialization for the data loading is on each command or related CS file.

        public static HashSet<ApplyBuffDebugEvent> playerBuffs = new();
        //-- -- Commands
        public static Dictionary<ulong, bool> sunimmunity { get; set; }
        public static Dictionary<ulong, bool> nocooldownlist { get; set; }
        public static Dictionary<ulong, bool> godmode { get; set; }
        public static Dictionary<ulong, bool> speeding { get; set; }
        public static Dictionary<string, WaypointData> waypoints { get; set; }
        public static Dictionary<ulong, int> waypoints_owned { get; set; }
        public static Dictionary<ulong, int> user_permission { get; set; }
        public static Dictionary<string, int> command_permission { get; set; }
        public static Dictionary<ulong, PowerUpData> PowerUpList { get; set; }
        public static List<ItemKit> kits { get; set; }

        //-- -- Ban System
        public static Dictionary<ulong, BanData> user_banlist { get; set; }

        //-- -- EXP System
        public static Dictionary<ulong, int> player_experience { get; set; }
        /// <summary>
        /// Ability points awarded per level.
        /// </summary>
        public static Dictionary<ulong, int> player_abilityIncrease { get; set; }
        /// <summary>
        /// Buff stat bonuses from leveling
        /// </summary>
        public static LazyDictionary<ulong, LazyDictionary<UnitStatType,float>> player_level_stats { get; set; }   
        /// <summary>
        /// A configuration database of class stats per ability point spent.
        /// </summary>
        public static Dictionary<string, Dictionary<UnitStatType, float>> experience_class_stats { get; set; }

        public static Dictionary<ulong, bool> player_log_exp { get; set; }
        
        public static Dictionary<ulong, DateTime> player_logout { get; set; }

        //-- -- Mastery System
        public static LazyDictionary<ulong, WeaponMasteryData> player_weaponmastery { get; set; }
        public static Dictionary<ulong, bool> player_log_mastery { get; set; }
        public static Dictionary<WeaponMasterySystem.MasteryType, List<StatConfig>> masteryStatConfig { get; set; }

        //-- -- Bloodline System
        public static LazyDictionary<ulong, BloodlineMasteryData> playerBloodline { get; set; }
        public static Dictionary<ulong, bool> playerLogBloodline { get; set; }
        public static Dictionary<BloodlineSystem.BloodType, List<StatConfig>> bloodlineStatConfig { get; set; }

        //-- -- World Event System
        public static ConcurrentDictionary<int, FactionData> FactionStats { get; set; }
        public static HashSet<PrefabGUID> IgnoredMonstersGUID { get; set; }
    }
}

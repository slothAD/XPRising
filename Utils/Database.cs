using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace RPGMods.Utils
{
    public static class Cache
    {
        //-- Cache (Wiped on plugin reload, server restart, and shutdown.)

        //-- -- Player Cache
        public static Dictionary<FixedString64, PlayerData> NamePlayerCache = new();
        public static Dictionary<ulong, PlayerData> SteamPlayerCache = new();
        public static Dictionary<Entity, PlayerGroup> PlayerAllies = new();
        public static Dictionary<Entity, LocalToWorld> PlayerLocations = new();
        public static Dictionary<ulong, List<BuffData>> buffData = new();

        //-- -- Commands
        public static Dictionary<ulong, float> command_Cooldown = new();
        
        //-- -- Combat
        // Note: These are currently only used in the HunterHunted system, but are able to be used generically
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

        //-- -- PvP System
        public static Dictionary<Entity, LevelData> PlayerLevelCache = new();
        public static Dictionary<ulong, PvPOffenseLog> OffenseLog = new();
        public static Dictionary<ulong, ReputationLog> ReputationLog = new();
        public static Dictionary<Entity, StateData> HostilityState = new();

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
    }

    public static class Database
    {
        public static JsonSerializerOptions JSON_options = new()
        {
            WriteIndented = false,
            IncludeFields = false
        };
        public static JsonSerializerOptions Pretty_JSON_options = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };
        //-- Dynamic Database (Saved on a JSON file on plugin reload, server restart, and shutdown.)
        //-- Initialization for the data loading is on each command or related CS file.

        //public static Dictionary<ulong, ApplyBuffDebugEvent> playerBuffs = new();
        public static HashSet<ApplyBuffDebugEvent> playerBuffs = new();
        //-- -- Commands
        public static Dictionary<ulong, bool> sunimmunity { get; set; }
        public static Dictionary<ulong, bool> nocooldownlist { get; set; }
        public static Dictionary<ulong, bool> godmode { get; set; }
        public static Dictionary<ulong, bool> speeding { get; set; }
        public static Dictionary<ulong, bool> autoRespawn { get; set; }
        public static Dictionary<string, Tuple<float,float,float>> waypointDBNew { get; set; }

        public static Dictionary<string, WaypointData> globalWaypoint { get; set; }
        public static Dictionary<string, WaypointData> waypoints { get; set; }
        public static Dictionary<ulong, int> waypoints_owned { get; set; }
        public static Dictionary<ulong, int> user_permission { get; set; }
        public static Dictionary<string, int> command_permission { get; set; }
        public static Dictionary<ulong, PowerUpData> PowerUpList { get; set; }

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
        public static bool ErrorOnLoadingExperienceClasses = false;

        public static Dictionary<ulong, bool> player_log_exp { get; set; }

        //-- -- PvP System
        //-- -- -- NEW Database
        public static ConcurrentDictionary<ulong, PvPData> PvPStats { get; set; }
        public static Dictionary<ulong, SiegeData> SiegeState = new();
        public static Dictionary<Entity, Entity> killMap { get; set; }
        //-- -- -- OLD Database (To be removed)
        public static Dictionary<ulong, int> pvpkills { get; set; }
        public static Dictionary<ulong, int> pvpdeath { get; set; }
        public static Dictionary<ulong, double> pvpkd { get; set; }

        //-- -- Mastery System
        public static Dictionary<ulong, WeaponMasterData> player_weaponmastery { get; set; }
        public static Dictionary<ulong, DateTime> player_decaymastery_logout { get; set; }
        public static Dictionary<ulong, bool> player_log_mastery { get; set; }

        //-- -- Bloodline System
        public static Dictionary<ulong, BloodlineData> playerBloodline { get; set; }
        public static Dictionary<ulong, DateTime> playerDecayBloodlineLogout { get; set; }
        public static Dictionary<ulong, bool> playerLogBloodline { get; set; }

        //-- -- World Event System
        public static ConcurrentDictionary<int, FactionData> FactionStats { get; set; }
        public static HashSet<string> IgnoredMonsters { get; set; }
        public static HashSet<PrefabGUID> IgnoredMonstersGUID { get; set; }

        public static class Buff
        {
            public static PrefabGUID EquipBuff = new PrefabGUID(343359674);
            public static PrefabGUID WolfStygian = new PrefabGUID(-1158884666);
            public static PrefabGUID WolfNormal = new PrefabGUID(-351718282);
            public static PrefabGUID BatForm = new PrefabGUID(1205505492);
            public static PrefabGUID NormalForm = new PrefabGUID(1352541204);
            public static PrefabGUID RatForm = new PrefabGUID(902394170);

            public static PrefabGUID DownedBuff = new PrefabGUID(-1992158531);
            public static PrefabGUID BloodSight = new PrefabGUID(1199823151);

            public static PrefabGUID InCombat = new PrefabGUID(581443919);
            public static PrefabGUID InCombat_PvP = new PrefabGUID(697095869);
            public static PrefabGUID OutofCombat = new PrefabGUID(897325455);
            public static PrefabGUID BloodMoon = new PrefabGUID(-560523291);

            public static PrefabGUID Severe_GarlicDebuff = new PrefabGUID(1582196539);          //-- Using this for PvP Punishment debuff
            public static PrefabGUID General_GarlicDebuff = new PrefabGUID(-1701323826);

            public static PrefabGUID Buff_VBlood_Perk_Moose = new PrefabGUID(-1464851863);      //-- Using this for commands & mastery buff
            public static PrefabGUID PerkMoose = new PrefabGUID(-1464851863);
            //public static PrefabGUID NPCInvul = new PrefabGUID(544892542);

            /*
            541307027		BloodQualityUnitBuff_Brute Entity(3611:3)  - Entity  - PrefabGUID  - Prefab  - PrefabCollectionPrefabTag
1708373727		BloodQualityUnitBuff_Creature Entity(3612:3)  - Entity  - PrefabGUID  - Prefab  - PrefabCollectionPrefabTag
-798998623		BloodQualityUnitBuff_Mutant Entity(3613:3)  - Entity  - PrefabGUID  - Prefab  - PrefabCollectionPrefabTag
1064495467		BloodQualityUnitBuff_Rogue Entity(3614:3)  - Entity  - PrefabGUID  - Prefab  - PrefabCollectionPrefabTag
-1959246041		BloodQualityUnitBuff_Scholar Entity(3615:3)  - Entity  - PrefabGUID  - Prefab  - PrefabCollectionPrefabTag
1243275376		BloodQualityUnitBuff_Warrior Entity(3616:3)  - Entity  - PrefabGUID  - Prefab  - PrefabCollectionPrefabTag
2089547928		BloodQualityUnitBuff_Worker Entity(3617:3)  - Entity  - PrefabGUID  - Prefab  - PrefabCollectionPrefabTag
                */
            // 1409441911
            public static PrefabGUID cloakBuff = new PrefabGUID(1409441911);

            public static PrefabGUID SiegeGolem_T01 = new PrefabGUID(-148535031);
            public static PrefabGUID SiegeGolem_T02 = new PrefabGUID(914043867);

            //-- Coffin Buff
            public static PrefabGUID AB_Interact_GetInside_Owner_Buff_Stone = new PrefabGUID(569692162); //-- Inside Stone Coffin
            public static PrefabGUID AB_Interact_GetInside_Owner_Buff_Base = new PrefabGUID(381160212); //-- Inside Base/Wooden Coffin

            public static PrefabGUID AB_ExitCoffin_Travel_Phase_Stone = new PrefabGUID(-162820429);
            public static PrefabGUID AB_ExitCoffin_Travel_Phase_Base = new PrefabGUID(-997204628);
            public static PrefabGUID AB_Interact_TombCoffinSpawn_Travel = new PrefabGUID(722466953);

            public static PrefabGUID AB_Interact_WaypointSpawn_Travel = new PrefabGUID(-66432447);
            public static PrefabGUID AB_Interact_WoodenCoffinSpawn_Travel = new PrefabGUID(-1705977973);
            public static PrefabGUID AB_Interact_StoneCoffinSpawn_Travel = new PrefabGUID(-1276482574);

            //-- LevelUp Buff
            public static PrefabGUID LevelUp_Buff = new PrefabGUID(-1133938228);

            //-- Nice Effect...
            public static PrefabGUID AB_Undead_BishopOfShadows_ShadowSoldier_Minion_Buff = new PrefabGUID(450215391);   //-- Impair cast & movement

            //-- The Only Potential Buff we can use for hostile mark
            //Buff_Cultist_BloodFrenzy_Buff - PrefabGuid(-106492795)

            //-- Relic Buff
            //[-238197495]          AB_Interact_UseRelic_Manticore_Buff
            //[-1161197991]		    AB_Interact_UseRelic_Paladin_Buff
            //[-1703886455]		    AB_Interact_UseRelic_Behemoth_Buff

            //-- Fun
            public static PrefabGUID HolyNuke = new PrefabGUID(-1807398295);
            public static PrefabGUID AB_Manticore_Flame_Buff_UNUSED = new PrefabGUID(1502566434); //-- And Dangerous~
            public static PrefabGUID Pig_Transform_Debuff = new PrefabGUID(1356064917);

            //[505018388]		    AB_Nightlurker_Rush_Buff


            //-- Possible Buff use
            public static PrefabGUID EquipBuff_Chest_Base = new PrefabGUID(1872694456);         //-- Hmm... not sure what to do with this right now...
            public static PrefabGUID Buff_VBlood_Perk_ProgTest = new PrefabGUID(1614409699);    //-- What does this do??
            public static PrefabGUID AB_BloodBuff_VBlood_0 = new PrefabGUID(20081801);          //-- Does it do anything negative...? How can i check for this, seems like it's a total blank o.o

            //-- Just putting it here for no reason at all...
            //public static PrefabGUID Admin_Observe_Ghost_Buff = new PrefabGUID(77473184);       //-- Not sure what to do with it
            //[1258181143]		    AB_Undead_Priest_Elite_RaiseHorde_Minion_Buff
            //[1502566434]		    AB_Manticore_Flame_Buff_UNUSED
            //[-1133938228]		    AB_Town_Priest_HealBomb_Buff        //-- Good Heal Effect
            //[-225445080]          AB_Nun_AoE_ApplyLight_Buff          //-- Low Healing Effect
            //[-2115732274]		    AB_Manticore_Flying_Buff

            //[-474441982]		    Buff_General_Teleport_Travel        //-- Usefull for imprissoning someone?


        }
    }
}

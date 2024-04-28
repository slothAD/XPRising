using ProjectM;
using RPGMods.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Unity.Entities;

namespace RPGMods.Systems
{
    public static class WorldDynamicsSystem
    {
        public static EntityManager em = Plugin.Server.EntityManager;
        public static bool isFactionDynamic = false;

        private static bool loopInProgress = false;
        public static bool growOnKill = false;
        public static void OnDayCycle()
        {
            if (Plugin.isInitialized == false || loopInProgress == true) return;

            loopInProgress = true;
            if (Object.ReferenceEquals(Database.FactionStats, null)){ Plugin.Logger.LogWarning("Faction Stats loaded as Null, check the json file."); return;}
            foreach (var faction in Database.FactionStats)
            {
                if (faction.Value.Active == false) continue;

                var factionStats = faction.Value;
                if(factionStats.RequiredPower < 0) {
                    Plugin.Logger.LogWarning(factionStats.Name + "'s required power to levelup is negative, fixing it now.");
                    factionStats.RequiredPower *= -1;
                    Database.FactionStats[faction.Key] = factionStats;
                }
                if (growOnKill){
                    factionStats.StoredPower -= factionStats.DailyPower;
                }
                else{
                    //-- Calculate total stored power.
                    factionStats.StoredPower += factionStats.ActivePower;
                    //-- Reset the active power.
                    factionStats.ActivePower = factionStats.DailyPower;
                }

                //-- Calculate if faction level should change.
                checkForLevelup(factionStats, faction.Key);

                //Database.FactionStats[faction.Key] = factionStats;
            }
            loopInProgress = false;
        }

        public static void checkForLevelup(FactionData factionStats, int factionKey){
            if (factionStats.StoredPower >= factionStats.RequiredPower){
                factionStats.Level += 1;
                factionStats.StoredPower = 0;
                if (factionStats.Level > factionStats.MaxLevel) factionStats.Level = factionStats.MaxLevel;
            }
            else if (factionStats.StoredPower < 0){
                factionStats.Level -= 1;
                if (growOnKill){
                    factionStats.StoredPower = factionStats.RequiredPower + factionStats.StoredPower;
                }
                else{
                    factionStats.StoredPower = 0;
                }
                if (factionStats.Level < factionStats.MinLevel) factionStats.Level = factionStats.MinLevel;
            }
            Database.FactionStats[factionKey] = factionStats;

        }

        public static void MobKillMonitor(Entity entity){
            if (!em.HasComponent<FactionReference>(entity)) return;

            var factionID = em.GetComponentData<FactionReference>(entity).FactionGuid._Value.GetHashCode();
            if (Object.ReferenceEquals(Database.FactionStats, null)){ Plugin.Logger.LogWarning("Faction Stats loaded as Null, check the json file."); return;}
            if (!Database.FactionStats.TryGetValue(factionID, out FactionData factionStats)) return;

            if (factionStats.Active == false) return;

            if (growOnKill) {
                factionStats.StoredPower += 1;
            }
            else {
                factionStats.ActivePower -= 1;
            }
            checkForLevelup(factionStats, factionID);
            //Database.FactionStats[factionID] = factionStats;
        }

        public static void MobReceiver(Entity entity)
        {
            if (!em.HasComponent<UnitLevel>(entity) && !em.HasComponent<UnitStats>(entity)) return;

            if (Database.IgnoredMonstersGUID.Contains(em.GetComponentData<PrefabGUID>(entity))) return;

            var factionID = em.GetComponentData<FactionReference>(entity).FactionGuid._Value.GetHashCode();
            if (Object.ReferenceEquals(Database.FactionStats, null)){ Plugin.Logger.LogWarning("Faction Stats loaded as Null, check the json file."); return;}
            if (!Database.FactionStats.TryGetValue(factionID, out var factionStats)) return;

            if (factionStats.Active == false) return;

            factionStats.Level = Math.Clamp(factionStats.Level, factionStats.MinLevel, factionStats.MaxLevel);
            if (factionStats.Level <= 0) return;

            //-- Unit Buffers for stats modification
            var floatBuffer = em.GetBuffer<FloatModificationBuffer>(entity);
            var boolBuffer = em.GetBuffer<BoolModificationBuffer>(entity);
            var intBuffer = em.GetBuffer<IntModificationBuffer>(entity);

            //-- Unit Stats
            var unitHealth = em.GetComponentData<Health>(entity);
            var unitStats = em.GetComponentData<UnitStats>(entity);
            var unitLevel = em.GetComponentData<UnitLevel>(entity);

            //-- Calculate Modifications
            int Level = (int) Math.Ceiling((float)factionStats.FactionBonus.Level_Int * factionStats.Level);
            float HP = (float) Math.Ceiling(factionStats.FactionBonus.HP_Float * factionStats.Level);
            float PhysicalPower = (float)Math.Ceiling(factionStats.FactionBonus.PhysicalPower_Float * factionStats.Level);
            float PhysicalResistance = (float)Math.Ceiling(factionStats.FactionBonus.PhysicalResistance_Float * factionStats.Level);
            float PhysicalCriticalStrikeChance = (float)Math.Ceiling(factionStats.FactionBonus.PhysicalCriticalStrikeChance_Float * factionStats.Level);
            float PhysicalCriticalStrikeDamage = (float)Math.Ceiling(factionStats.FactionBonus.PhysicalCriticalStrikeDamage_Float * factionStats.Level);
            float SpellPower = (float)Math.Ceiling(factionStats.FactionBonus.SpellPower_Float * factionStats.Level);
            float SpellResistance = (float)Math.Ceiling(factionStats.FactionBonus.SpellResistance_Float * factionStats.Level);
            float SpellCriticalStrikeChance = (float)Math.Ceiling(factionStats.FactionBonus.SpellCriticalStrikeChance_Float * factionStats.Level);
            float SpellCriticalStrikeDamage = (float)Math.Ceiling(factionStats.FactionBonus.SpellCriticalStrikeDamage_Float * factionStats.Level);
            float DamageVsPlayerVampires = (float)Math.Ceiling(factionStats.FactionBonus.DamageVsPlayerVampires_Float * factionStats.Level);
            float ResistVsPlayerVampires = (float)Math.Ceiling(factionStats.FactionBonus.ResistVsPlayerVampires_Float * factionStats.Level);
            int FireResistance = (int)Math.Ceiling((float)factionStats.FactionBonus.FireResistance_Int * factionStats.Level);

            //-- Do Modifications
            if (Level > 0)
            {
                unitLevel.Level += Level;
                em.SetComponentData(entity, unitLevel);
            }

            if (HP > 0)
            {
                unitHealth.MaxHealth.SetBaseValue(unitHealth.MaxHealth._Value + HP, floatBuffer);
                unitHealth.Value = unitHealth.MaxHealth._Value + HP;
                em.SetComponentData(entity, unitHealth);
            }

            if (PhysicalPower > 0) unitStats.PhysicalPower.SetBaseValue(unitStats.PhysicalPower._Value + PhysicalPower, floatBuffer);
            if (PhysicalResistance > 0) unitStats.PhysicalResistance.SetBaseValue(PhysicalResistance, floatBuffer);
            if (PhysicalCriticalStrikeChance > 0) unitStats.PhysicalCriticalStrikeChance.SetBaseValue(unitStats.PhysicalCriticalStrikeChance._Value + PhysicalCriticalStrikeChance, floatBuffer);
            if (PhysicalCriticalStrikeDamage > 0) unitStats.PhysicalCriticalStrikeDamage.SetBaseValue(unitStats.PhysicalCriticalStrikeDamage._Value + PhysicalCriticalStrikeDamage, floatBuffer);
            if (SpellPower > 0) unitStats.SpellPower.SetBaseValue(unitStats.SpellPower._Value + SpellPower, floatBuffer);
            if (SpellResistance > 0) unitStats.SpellResistance.SetBaseValue(SpellResistance, floatBuffer);
            if (SpellCriticalStrikeChance > 0) unitStats.SpellCriticalStrikeChance.SetBaseValue(unitStats.SpellCriticalStrikeChance._Value + SpellCriticalStrikeChance, floatBuffer);
            if (SpellCriticalStrikeDamage > 0) unitStats.SpellCriticalStrikeDamage.SetBaseValue(unitStats.SpellCriticalStrikeDamage._Value + SpellCriticalStrikeDamage, floatBuffer);
            //if (DamageVsPlayerVampires > 0) unitStats.DamageVsPlayerVampires.Set(DamageVsPlayerVampires, floatBuffer);
            //if (ResistVsPlayerVampires > 0) unitStats.ResistVsPlayerVampires.Set(ResistVsPlayerVampires, floatBuffer);
            if (FireResistance > 0) unitStats.FireResistance.SetBaseValue(unitStats.FireResistance._Value + FireResistance, intBuffer);
            unitStats.PvPProtected.SetBaseValue(false, boolBuffer);
            em.SetComponentData(entity, unitStats);
        }

        public static void SaveFactionStats()
        {
            File.WriteAllText(AutoSaveSystem.mainSaveFolder+"factionstats.json", JsonSerializer.Serialize(Database.FactionStats, Database.Pretty_JSON_options));
        }

        public static void SaveIgnoredMobs()
        {
            File.WriteAllText(AutoSaveSystem.mainSaveFolder+"ignoredmonsters.json", JsonSerializer.Serialize(Database.IgnoredMonsters, Database.Pretty_JSON_options));
        }

        public static void LoadIgnoredMobs()
        {
            if (!File.Exists(AutoSaveSystem.mainSaveFolder+"ignoredmonsters.json"))
            {
                var stream = File.Create(AutoSaveSystem.mainSaveFolder+"ignoredmonsters.json");
                stream.Dispose();
            }
            string content = File.ReadAllText(AutoSaveSystem.mainSaveFolder+"ignoredmonsters.json");
            try
            {
                Database.IgnoredMonsters = JsonSerializer.Deserialize<HashSet<string>>(content);
                Database.IgnoredMonstersGUID = new HashSet<PrefabGUID>();
                foreach (var item in Database.IgnoredMonsters)
                {
                    if (Enum.TryParse(item, true, out Prefabs.Units unit))
                    {
                        Database.IgnoredMonstersGUID.Add(new PrefabGUID((int)unit));
                    }
                }
                Plugin.Logger.LogWarning("IgnoredMonsters DB Populated.");
            }
            catch
            {
                Database.IgnoredMonsters = new HashSet<string>();
                Database.IgnoredMonstersGUID = new HashSet<PrefabGUID>();

                Database.IgnoredMonsters.Add(Enum.GetName(Prefabs.Units.CHAR_Undead_GhostBanshee));
                Database.IgnoredMonstersGUID.Add(new PrefabGUID((int)Prefabs.Units.CHAR_Undead_GhostBanshee));

                SaveIgnoredMobs();

                Plugin.Logger.LogWarning("IgnoredMonsters DB Created.");
            }
        }

        private static Prefabs.Faction[] IgnoredFactions = new Prefabs.Faction[]
        {
            Prefabs.Faction.ChurchOfLum_Slaves,
            Prefabs.Faction.ChurchOfLum_Slaves_Rioters,
            Prefabs.Faction.Ignored,
            Prefabs.Faction.Players,
            Prefabs.Faction.Players_Mutant,
            Prefabs.Faction.Players_Castle_Prisoners,
            Prefabs.Faction.Players_Shapeshift_Human,
            Prefabs.Faction.Traders_T01,
            Prefabs.Faction.Traders_T02,
            Prefabs.Faction.WerewolfHuman,
            Prefabs.Faction.World_Prisoners,
            Prefabs.Faction.Unknown
        };

        public static void LoadFactionStats()
        {
            if (!File.Exists(AutoSaveSystem.mainSaveFolder+"factionstats.json"))
            {
                var stream = File.Create(AutoSaveSystem.mainSaveFolder+"factionstats.json");
                stream.Dispose();
            }
            string content = File.ReadAllText(AutoSaveSystem.mainSaveFolder+"factionstats.json");
            try
            {
                Database.FactionStats = JsonSerializer.Deserialize<ConcurrentDictionary<int, FactionData>>(content);
                Plugin.Logger.LogWarning("FactionStats DB Populated.");
            }
            catch
            {
                Database.FactionStats = new ConcurrentDictionary<int, FactionData>();

                foreach (var faction in Enum.GetValues<Prefabs.Faction>())
                {
                    if (!IgnoredFactions.Contains(faction))
                    {
                        Database.FactionStats.TryAdd((int)faction, new FactionData(faction));
                    }
                }
                SaveFactionStats();
                Plugin.Logger.LogWarning("FactionStats DB Created.");
            }
        }
    }
}

using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using BepInEx.Logging;
using OpenRPG.Models;
using Unity.Entities;
using OpenRPG.Utils;

namespace OpenRPG.Systems
{
    using WeaponMasteryData = LazyDictionary<WeaponMasterySystem.MasteryType,MasteryData>;
    public static class WeaponMasterySystem
    {
        private static EntityManager _em = Plugin.Server.EntityManager;

        public static bool IsMasteryEnabled = true;
        public static int MasteryCombatTick = 5;
        public static int MaxCombatTick = 12;
        public static double MasteryMultiplier = 1;
        public static double MaxMastery = 100;
        public static double VBloodMultiplier = 15;
        
        // TODO online decay
        public static bool IsDecaySystemEnabled = true;
        public static int DecayInterval = 60;
        public static double OnlineDecayValue = 0;
        public static double OfflineDecayValue = 1;

        // Shou Change - make options for spell mastery with weapons active.
        public static bool SpellMasteryNeedsUnarmedToUse = true;
        public static bool SpellMasteryNeedsUnarmedToLearn = true;
        public static bool LinearCdr = false;
        public static bool CdrStacks = false;

        public static double MaxEffectiveness = 10;
        public static bool EffectivenessSubSystemEnabled = false;
        public static bool GrowthSubSystemEnabled = false;
        public static double MinGrowth = 0.1f;
        public static double MaxGrowth = 10;
        public static double GrowthPerEffectiveness = 1;

        private static readonly Random Rand = new Random();
        
        public enum MasteryType
        {
            Unarmed,
            Spear,
            Sword,
            Scythe,
            Crossbow,
            Mace,
            Slasher,
            Axe,
            FishingPole,
            Rapier,
            Pistol,
            GreatSword,
            Spell
        }
        
        // This is a "potential" name to mastery map. Multiple keywords map to the same mastery
        public static Dictionary<string, MasteryType> KeywordToMasteryMap = new()
        {
            { "spell", MasteryType.Spell },
            { "magic", MasteryType.Spell },
            { "unarmed", MasteryType.Unarmed },
            { "none", MasteryType.Unarmed },
            { "spear", MasteryType.Spear },
            { "crossbow", MasteryType.Crossbow },
            { "slashers", MasteryType.Slasher },
            { "slasher", MasteryType.Slasher },
            { "scythe", MasteryType.Scythe },
            { "reaper", MasteryType.Scythe },
            { "sword", MasteryType.Sword },
            { "fishingpole", MasteryType.FishingPole },
            { "mace", MasteryType.Mace },
            { "axe", MasteryType.Axe },
            { "greatsword", MasteryType.GreatSword },
            { "rapier", MasteryType.Rapier },
            { "pistol", MasteryType.Pistol },
            { "dagger", MasteryType.Sword },
            { "longbow", MasteryType.Crossbow },
            { "xbow", MasteryType.Crossbow }
        };

        private static double CalculateMasteryChange(double value, double growth)
        {
            return (value * (Rand.Next(1, 10) * 0.1) * growth)/1000.0;
        }

        public static void UpdateMastery(Entity killer, Entity victim)
        {
            if (killer == victim) return;
            if (_em.HasComponent<Minion>(victim)) return;
            
            Entity userEntity = _em.GetComponentData<PlayerCharacter>(killer).UserEntity;
            ulong steamID = _em.GetComponentData<User>(userEntity).PlatformId;
            
            Plugin.Log(Plugin.LogSystem.Mastery, LogLevel.Info, $"Updating weapon mastery for {steamID}");
            
            var masteryType = WeaponToMasteryType(GetWeaponType(killer));
            
            var victimStats = _em.GetComponentData<UnitStats>(victim);
            double masteryValue = victimStats.PhysicalPower;
            double spellMasteryValue = victimStats.SpellPower;
            
            var wd = Database.player_weaponmastery[steamID];

            double weaponGrowth = 1;
            double spellGrowth = 1;

            if (GrowthSubSystemEnabled) {
                weaponGrowth = wd[masteryType].Growth;
                spellGrowth = wd.GetValueOrDefault(MasteryType.Spell).Growth;
            }
            
            var isVBlood = false;
            if (_em.HasComponent<BloodConsumeSource>(victim))
            {
                var bloodSource = _em.GetComponentData<BloodConsumeSource>(victim);
                isVBlood = bloodSource.UnitBloodType.Equals(Helper.vBloodType);
            }
            
            if (_em.HasComponent<PlayerCharacter>(victim) && _em.TryGetComponentData<Equipment>(victim, out var victimGear))
            {
                var bonusMastery = victimGear.ArmorLevel + victimGear.WeaponLevel + victimGear.SpellLevel;
                masteryValue *= (1 + (bonusMastery * 0.01));
                spellMasteryValue *= (1 + (bonusMastery * 0.01));
                Plugin.Log(Plugin.LogSystem.Mastery, LogLevel.Info, $"Bonus mastery {bonusMastery:F3}]");
            }

            var vBloodMultiplier = isVBlood ? VBloodMultiplier : 1;
            var changeInMastery = CalculateMasteryChange(masteryValue, weaponGrowth) * vBloodMultiplier * MasteryMultiplier;
            var changeInSpellMastery = CalculateMasteryChange(spellMasteryValue, spellGrowth) * vBloodMultiplier * MasteryMultiplier;
            
            wd = ModMastery(steamID, wd, masteryType, changeInMastery);
            var updateSpellMastery = !SpellMasteryNeedsUnarmedToLearn || masteryType == MasteryType.Unarmed;
            if (updateSpellMastery) {
                wd = ModMastery(steamID, wd, MasteryType.Spell, changeInSpellMastery);
            }

            Database.player_weaponmastery[steamID] = wd;

            if (Database.player_log_mastery.TryGetValue(steamID, out bool isLogging) && isLogging)
            {
                var currentMastery = wd[masteryType].Mastery;
                Output.SendLore(userEntity, $"<color=#ffb700>Weapon mastery has increased by {changeInMastery:F3}% [ {Enum.GetName(masteryType)}: {currentMastery:F2}% ]</color>");
                
                if (updateSpellMastery)
                {
                    var currentSpellMastery = wd.GetValueOrDefault(MasteryType.Spell).Mastery;
                    Output.SendLore(userEntity, $"<color=#ffb700>Weapon mastery has increased by {changeInSpellMastery:F3}% [ Spell: {currentSpellMastery:F2}% ]</color>");
                }
            }
        }

        public static void LoopMastery(Entity user, Entity player)
        {
            var steamID = _em.GetComponentData<User>(user).PlatformId;

            Cache.player_last_combat.TryGetValue(steamID, out var lastCombat);
            Cache.player_combat_ticks.TryGetValue(steamID, out var combatTicks);
            var elapsedTime = DateTime.Now - lastCombat;
            if (elapsedTime.TotalSeconds >= 10) combatTicks = 0;
            if (elapsedTime.TotalSeconds * 0.2 < 1) return;

            Cache.player_last_combat[steamID] = DateTime.Now;

            if (combatTicks > MaxCombatTick) return;
            var masteryType = WeaponToMasteryType(GetWeaponType(player));
            
            double weaponGrowth = 1;
            double spellGrowth = 1;
            
            var wd = Database.player_weaponmastery[steamID];

            if (GrowthSubSystemEnabled) {
                weaponGrowth = wd[masteryType].Growth;
                spellGrowth = wd.GetValueOrDefault(MasteryType.Spell).Growth;
            }

            var changeInMastery = (MasteryCombatTick * MasteryMultiplier * weaponGrowth)/1000.0;
            var changeInSpellMastery = (MasteryCombatTick * MasteryMultiplier * spellGrowth)/1000.0;
            Cache.player_combat_ticks[steamID] = combatTicks + 1;
            
            wd = ModMastery(steamID, wd, masteryType, changeInMastery);
            if (!SpellMasteryNeedsUnarmedToLearn || masteryType == MasteryType.Unarmed) {
                wd = ModMastery(steamID, wd, MasteryType.Spell, changeInSpellMastery);
            }

            Database.player_weaponmastery[steamID] = wd;
        }

        public static void DecayMastery(Entity userEntity, DateTime lastDecay)
        {
            var steamID = _em.GetComponentData<User>(userEntity).PlatformId;
            var elapsedTime = DateTime.Now - lastDecay;
            if (elapsedTime.TotalSeconds < DecayInterval) return;

            var decayTicks = (int)Math.Floor(elapsedTime.TotalSeconds / DecayInterval);
            if (decayTicks > 0)
            {
                var decayValue = OfflineDecayValue * decayTicks * -1;

                Output.SendLore(userEntity, $"You've been offline for {elapsedTime.TotalMinutes} minute(s). Your weapon mastery has decayed by {decayValue * 0.001:F3}%");
                
                var wd = Database.player_weaponmastery[steamID];

                foreach (var type in Enum.GetValues<MasteryType>())
                {
                    wd = ModMastery(steamID, wd, type, decayValue);
                }

                Database.player_weaponmastery[steamID] = wd;
            }
        }

        public static void BuffReceiver(DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer, Entity owner, ulong steamID)
        {
            var masteryType = WeaponToMasteryType(GetWeaponType(owner));
            var hasMastery = CalcBuffValue(steamID, masteryType, out var masteryValue, out var masterySpellValue);
            
            if (hasMastery)
            {
                ApplyBuff(buffer, masteryType, masteryValue, steamID);
                if(!SpellMasteryNeedsUnarmedToUse || masteryType == MasteryType.Unarmed){
                    ApplyBuff(buffer, MasteryType.Spell, masterySpellValue, steamID);
                }                 
            }
        }

        private static void ApplyBuff(DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer, MasteryType type, double mastery, ulong steamID)
        {
            var wd = Database.player_weaponmastery[steamID];
            var config = Database.masteryStatConfig[type];
            foreach (var statConfig in config)
            {
                var effectiveness = EffectivenessSubSystemEnabled ? wd[type].Effectiveness : 1;
                buffer.Add(Helper.MakeBuff(statConfig.type, Helper.CalcBuffValue(mastery, effectiveness, statConfig.rate, statConfig.type)));
            }
        }

        private static bool CalcBuffValue(ulong steamID, MasteryType masteryType, out double masteryValue, out double masterySpellValue){
            masteryValue = 0;
            masterySpellValue = 0;

            bool isFound = Database.player_weaponmastery.TryGetValue(steamID, out var mastery);
            if (!isFound) return false;
            
            masteryValue = Math.Max(mastery[masteryType].Mastery, 0);
            masterySpellValue = Math.Max(mastery.GetValueOrDefault(MasteryType.Spell).Mastery, 0);

            return true;
        }

        public static void ModMastery(ulong steamID, MasteryType type, double changeInMastery)
        {
            var wd = Database.player_weaponmastery[steamID];
            wd = ModMastery(steamID, wd, type, changeInMastery);
            Database.player_weaponmastery[steamID] = wd;
        }

        private static WeaponMasteryData ModMastery(ulong steamID, WeaponMasteryData wd, MasteryType type, double changeInMastery)
        {
            if (type == MasteryType.Unarmed){
                if (changeInMastery > 0) changeInMastery *= 2;
            }
            
            var mastery = wd[type];
            mastery.Mastery = Math.Clamp(mastery.Mastery + changeInMastery, 0, MaxMastery);
            Plugin.Log(Plugin.LogSystem.Mastery, LogLevel.Info, $"Mastery changed: {steamID}: {Enum.GetName(type)}: {mastery.Mastery}");
            wd[type] = mastery;
            return wd;
        }

        public static void ResetMastery(ulong steamID, MasteryType type) {
            if (!EffectivenessSubSystemEnabled) {
                if (Helper.FindPlayer(steamID, true, out _, out var targetUserEntity)) {
                    Output.SendLore(targetUserEntity, $"Effectiveness Subsystem disabled, not resetting mastery.");
                }
                return;
            }
            if (Database.player_weaponmastery.TryGetValue(steamID, out var wd))
            {
                var mastery = wd[type];
                // If it is already 0, then this won't have much effect.
                if (mastery.Mastery > 0)
                {
                    wd[type] = mastery.ResetMastery(MaxMastery, MaxEffectiveness, GrowthPerEffectiveness, MaxGrowth, MinGrowth);
                    Plugin.Log(Plugin.LogSystem.Mastery, LogLevel.Info, $"Mastery reset: {Enum.GetName(type)}: {mastery}");
                }
                Database.player_weaponmastery[steamID] = wd;
            }
        }

        public static WeaponType GetWeaponType(Entity player)
        {
            var weaponEntity = _em.GetComponentData<Equipment>(player).WeaponSlotEntity._Entity;
            var weaponType = WeaponType.None;
            if (_em.HasComponent<EquippableData>(weaponEntity))
            {
                var weaponData = _em.GetComponentData<EquippableData>(weaponEntity);
                weaponType = weaponData.WeaponType;
            }
            return weaponType;
        }
        
        public static MasteryType WeaponToMasteryType(WeaponType weapon)
        {
            // Note: we are not just simply casting the int value of weapon to a MasteryType to help ensure forwards compatibility.
            switch (weapon)
            {
                case WeaponType.None:
                    return MasteryType.Unarmed;
                case WeaponType.Spear:
                    return MasteryType.Spear;
                case WeaponType.Sword:
                    return MasteryType.Sword;
                case WeaponType.Scythe:
                    return MasteryType.Scythe;
                case WeaponType.Crossbow:
                    return MasteryType.Crossbow;
                case WeaponType.Mace:
                    return MasteryType.Mace;
                case WeaponType.Slashers:
                    return MasteryType.Slasher;
                case WeaponType.Axes:
                    return MasteryType.Axe;
                case WeaponType.FishingPole:
                    return MasteryType.FishingPole;
                case WeaponType.Rapier:
                    return MasteryType.Rapier;
                case WeaponType.Pistols:
                    return MasteryType.Pistol;
                case WeaponType.GreatSword:
                    return MasteryType.GreatSword;
                default:
                    Plugin.Log(Plugin.LogSystem.Mastery, LogLevel.Error, $"Cannot convert new weapon to mastery: {Enum.GetName(weapon)}. Defaulting to Spell.");
                    return MasteryType.Spell;
            }
        }

        public static Dictionary<MasteryType, List<StatConfig>> DefaultMasteryConfig()
        {
            return new Dictionary<MasteryType, List<StatConfig>>
            {
                { MasteryType.Unarmed, new List<StatConfig>() { new(UnitStatType.PhysicalPower, 0, 0.25f), new(UnitStatType.MovementSpeed, 0, 0.01f) } },
                { MasteryType.Spear, new List<StatConfig>() { new(UnitStatType.PhysicalPower, 0, 0.25f) } },
                { MasteryType.Sword, new List<StatConfig>() { new(UnitStatType.PhysicalPower, 0,  0.125f ), new(UnitStatType.SpellPower, 0,  0.125f ) } },
                { MasteryType.Scythe, new List<StatConfig>() { new(UnitStatType.PhysicalPower, 0,  0.125f ), new(UnitStatType.PhysicalCriticalStrikeChance, 0,  0.00125f ) } },
                { MasteryType.Crossbow, new List<StatConfig>() { new(UnitStatType.PhysicalCriticalStrikeChance, 0,  0.0025f ) } },
                { MasteryType.Mace, new List<StatConfig>() { new(UnitStatType.MaxHealth, 0,  1f ) } },
                { MasteryType.Slasher, new List<StatConfig>() { new(UnitStatType.PhysicalCriticalStrikeChance, 0,  0.00125f ), new(UnitStatType.MovementSpeed, 0,  0.005f ) } },
                { MasteryType.Axe, new List<StatConfig>() { new(UnitStatType.PhysicalPower, 0,  0.125f ), new(UnitStatType.MaxHealth, 0,  0.5f ) } },
                { MasteryType.FishingPole, new List<StatConfig>() },
                { MasteryType.Rapier, new List<StatConfig>() { new(UnitStatType.PhysicalCriticalStrikeChance, 0,  0.00125f ), new(UnitStatType.PhysicalCriticalStrikeDamage, 0,  0.00125f ) } },
                { MasteryType.Pistol, new List<StatConfig>() { new(UnitStatType.PhysicalCriticalStrikeChance, 0,  0.00125f ), new(UnitStatType.PhysicalCriticalStrikeDamage, 0,  0.00125f ) } },
                { MasteryType.GreatSword, new List<StatConfig>() { new(UnitStatType.PhysicalPower, 0,  0.125f ), new(UnitStatType.PhysicalCriticalStrikeDamage, 0,  0.00125f ) } },
                { MasteryType.Spell, new List<StatConfig>() { new(UnitStatType.CooldownModifier, 0,  100f )} }
            };
        }
    }
}

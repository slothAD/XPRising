using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Unity.Entities;
using OpenRPG.Utils;
using OpenRPG.Utils.Prefabs;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Systems
{
    public class Bloodlines
    {
        public static EntityManager em = Plugin.Server.EntityManager;

        public static bool bloodAmountModification = true;

        public static bool isDecaySystemEnabled = false;
        public static bool draculaGetsAll = true;
        public static double growthMultiplier = 1;
        public static int DecayInterval = 60;
        public static int Online_DecayValue = 0;
        public static int Offline_DecayValue = 1;
        public static double VBloodMultiplier = 15;

        public static bool areBloodlinesEnabled = true;
        public static bool mercilessBloodlines = false;
        public static bool effectivenessSubSystemEnabled = true;
        public static bool growthSubsystemEnabled = true;
        public static double growthPerEfficency = 1.0;
        public static double MaxBloodlineStrength = 100;
        public static double maxBloodlineEfficency = 5;
        public static double maxBloodlineGrowth = 10;
        public static double minBloodlineGrowth = 0.1;

        // Idk how to do this elegantly and allow it to be bound to a config.
        public static int[] draculaStats = { };
        public static double[] draculaMinStrength = { };
        public static double[] draculaRates = { };

        public static int[] arwenStats = { (int)UnitStatType.HolyResistance, (int)UnitStatType.MovementSpeed, (int)UnitStatType.DamageVsHumans};
        public static double[] arwenMinStrength = {0, 50, 100 };
        public static double[] arwenRates = { 0.25, 0.005, 0.0025 };

        public static int[] ilvrisStats = { (int)UnitStatType.FireResistance, (int)UnitStatType.PhysicalPower, (int)UnitStatType.DamageVsBeasts};
        public static double[] ilvrisMinStrength = { 0, 50, 100 };
        public static double[] ilvrisRates = { 0.25, 0.1, 0.0025 };

        public static int[] ayaStats = { (int)UnitStatType.SunResistance, (int)UnitStatType.PhysicalCriticalStrikeChance, (int)UnitStatType.DamageVsPlayerVampires};
        public static double[] ayaMinStrength = { 0, 50, 100 };
        public static double[] ayaRates = { 0.25, 0.001, 0.0025 };

        public static int[] nytheriaStats = { (int)UnitStatType.SilverResistance, (int)UnitStatType.PhysicalCriticalStrikeDamage, (int)UnitStatType.DamageVsUndeads };
        public static double[] nytheriaMinStrength = { 0, 50, 100 };
        public static double[] nytheriaRates = { 0.25, 0.01, 0.0025};

        public static int[] hadubertStats = { (int)UnitStatType.SpellPower, (int)UnitStatType.CooldownModifier, (int)UnitStatType.DamageVsDemons };
        public static double[] hadubertMinStrength = { 0, 50, 100};
        public static double[] hadubertRates = { 0.1, 200, 0.0025};

        public static int[] reiStats = { (int)UnitStatType.GarlicResistance, (int)UnitStatType.ResourceYield, (int)UnitStatType.DamageVsMineral, (int)UnitStatType.DamageVsVegetation, (int)UnitStatType.DamageVsWood };
        public static double[] reiMinStrength = { 0, 50, 100, 100, 100 };
        public static double[] reiRates = { 0.25, 0.01, 0.0025, 0.0025, 0.0025 };

        public static int[] semikaStats = { (int)UnitStatType.SpellCriticalStrikeChance, (int)UnitStatType.MovementSpeed, (int)UnitStatType.DamageVsHumans};
        public static double[] semikaMinStrength = { 0, 50, 100 };
        public static double[] semikaRates = { 0.005, 0.005, 0.0025 };


        public static int[][] stats = { draculaStats, arwenStats, ilvrisStats, ayaStats, nytheriaStats, hadubertStats, reiStats, semikaStats };
        public static double[][] minStrengths = { draculaMinStrength, arwenMinStrength, ilvrisMinStrength, ayaMinStrength, nytheriaMinStrength, hadubertMinStrength, reiMinStrength, semikaMinStrength };
        public static double[][] rates = { draculaRates, arwenRates, ilvrisRates, ayaRates, nytheriaRates, hadubertRates, reiRates, semikaRates };

        public static Dictionary<PrefabGUID, int> bloodlineMap = new Dictionary<PrefabGUID, int> {
            {new PrefabGUID((int)Remainders.BloodType_None), 0 },
            {new PrefabGUID((int)Remainders.BloodType_Creature), 1 },
            {new PrefabGUID((int)Remainders.BloodType_Warrior), 2 },
            {new PrefabGUID((int)Remainders.BloodType_Rogue), 3 },
            {new PrefabGUID((int)Remainders.BloodType_Brute), 4 },
            {new PrefabGUID((int)Remainders.BloodType_Scholar), 5 },
            {new PrefabGUID((int)Remainders.BloodType_Worker), 6 },
            {new PrefabGUID((int)Remainders.BloodType_Mutant), 7 }
        };

        public static string[] names = { "Dracula, Vampire Progenitor", "Arwen the Godeater", "Ilvris Dragonblood", "Aya the Shadowlord", "Nytheria the Destroyer", "Hadubert the Inferno", "Rei the Binder", "Semika the Ever-shifting" };

        public static Dictionary<string, int> nameMap = new Dictionary<string, int> {
            { "dracula", 0 },
            { "arwen", 1 },
            { "ilvris", 2 },
            { "aya", 3 },
            { "nytheria", 4 },
            { "hadubert", 5 },
            { "rei", 6 },
            { "semika", 7 },
            { "semi", 7 },
            { "mutant", 7 },
            { "frail", 0 },
            { "creature", 1 },
            { "warrior", 2 },
            { "rogue", 3 },
            { "brute", 4 },
            { "scholar", 5 },
            { "worker", 6 }
        };
        public static Dictionary<int, string> typeToName = new Dictionary<int, string> {
            { 0, "dracula" },
            { 1, "arwen" },
            { 2, "ilvris" },
            { 3, "aya" },
            { 4, "nytheria" },
            { 5, "hadubert" },
            { 6, "rei" },
            { 7, "Semika" },
        };

        private static readonly Random rand = new Random();

        public static void UpdateBloodline(Entity Killer, Entity Victim)
        {
            if (Killer == Victim) return;
            if (em.HasComponent<Minion>(Victim)) return;

            //var VictimStats = em.GetComponentData<UnitStats>(Victim);
            UnitLevel UnitLevel = em.GetComponentData<UnitLevel>(Victim);
            Entity userEntity = em.GetComponentData<PlayerCharacter>(Killer).UserEntity;
            User User = em.GetComponentData<User>(userEntity);
            ulong SteamID = User.PlatformId;

            
                

            Blood bloodline;
            int bloodlineIndex = -1;
            BloodConsumeSource victimBlood;
            double growthVal = 0;
            if (em.HasComponent<Blood>(Killer)){
                bloodline = em.GetComponentData<Blood>(Killer);
                growthVal = UnitLevel.Level;
                if(!bloodlineMap.TryGetValue(bloodline.BloodType, out bloodlineIndex)) {
                    if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity)){

                        Plugin.Log(LogSystem.Bloodline, LogLevel.Warning, "Bloodline DB Populated.");
                        Output.SendLore(targetUserEntity, "Bloodline not found for guid of " + bloodline.BloodType.GuidHash);
                    }
                    return;
                }
            }
            else { 
                return; 
            }


            bool isVBlood;
            if (em.HasComponent<BloodConsumeSource>(Victim)) {
                BloodConsumeSource BloodSource = em.GetComponentData<BloodConsumeSource>(Victim);
                isVBlood = BloodSource.UnitBloodType.Equals(Helper.vBloodType);
            }
            else {
                isVBlood = false;
            }

            if (isVBlood /*&& !mercilessBloodlines*/) growthVal = (growthVal * VBloodMultiplier);

            if (mercilessBloodlines){
                if (em.HasComponent<BloodConsumeSource>(Victim)){
                    victimBlood = em.GetComponentData<BloodConsumeSource>(Victim);
                    if (!(victimBlood.UnitBloodType.GuidHash == bloodline.BloodType.GuidHash|| isVBlood)){
                        Plugin.Log(LogSystem.Bloodline, LogLevel.Info, "Player blood of " + bloodline.BloodType.ToString() + " - " + bloodline.BloodType.GuidHash + " Not equals victim blood of " + victimBlood.UnitBloodType.ToString() + " - " + victimBlood.UnitBloodType.GuidHash);
                        return;
                    }
                    if (!(isVBlood || victimBlood.BloodQuality > getBloodlineData(SteamID).strength[bloodlineIndex])) {
                        Plugin.Log(LogSystem.Bloodline, LogLevel.Info, "Victim Blood Quality " + victimBlood.BloodQuality + " less than strength for bloodline " + names[bloodlineIndex] + " ("+bloodlineIndex+") of " + getBloodlineData(SteamID).strength[bloodlineIndex]);
                        return;
                    }
                    if (!(isVBlood || bloodline.Quality > getBloodlineData(SteamID).strength[bloodlineIndex])){
                        Plugin.Log(LogSystem.Bloodline, LogLevel.Info, "Current Blood Quality " + bloodline.Quality + " less than strength for bloodline " + names[bloodlineIndex] + " (" + bloodlineIndex + ") of " + getBloodlineData(SteamID).strength[bloodlineIndex]);
                        return;
                    }

                    if (bloodAmountModification) {
                        bloodline.MaxBlood.SetBaseValue(50000, bloodline.MaxBlood.GetOrAddModificationsBuffer(em, Killer));
                    }

                    growthVal *= 1 + ((victimBlood.BloodQuality+bloodline.Quality) - (getBloodlineData(SteamID).strength[bloodlineIndex]*2))/100;
                }
                else{
                    return;
                }
            }



            growthVal *= Math.Max(0.1, rand.NextDouble());

            if (em.HasComponent<PlayerCharacter>(Victim))
            {
                Equipment VictimGear = em.GetComponentData<Equipment>(Victim);
                var BonusMastery = VictimGear.ArmorLevel + VictimGear.WeaponLevel + VictimGear.SpellLevel;
                growthVal *= (1 + (BonusMastery * 0.01));
            }

            growthVal = (int)(growthVal * growthMultiplier);

            growthVal /= 1000;

            modBloodline(SteamID, bloodlineIndex, growthVal);

            if (Database.playerLogBloodline.TryGetValue(SteamID, out bool isLogging))
            {
                if (isLogging)
                {
                    double updatedValue = getBloodlineData(SteamID).strength[bloodlineIndex];
                    Output.SendLore(userEntity, $"<color=#ffb700>{names[bloodlineIndex]}'s bloodline has increased by {growthVal:F3}% [ {updatedValue:F3}%]</color>");
                }
            }
        }
        public static BloodlineData getBloodlineData(ulong SteamID) {
            BloodlineData bld;
            if (!Database.playerBloodline.TryGetValue(SteamID, out bld)) {
                bld = new BloodlineData();
                bld.strength = new double[rates.Length];
                bld.efficency = new double[rates.Length];
                bld.growth = new double[rates.Length];
                for (int i = 0; i < bld.growth.Length; i++) {
                    bld.strength[i] = 0;
                    bld.efficency[i] = 1;
                    bld.growth[i] = 1;
                }
                Database.playerBloodline[SteamID] = bld;
            }
            return bld;
        }
        public static void resetBloodline(ulong SteamID, int type) {
            if (!effectivenessSubSystemEnabled) {
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity)) {
                    Output.SendLore(targetUserEntity, $"Effectiveness Subsystem disabled, not resetting bloodline.");
                }
                return;
            }

            BloodlineData bld = getBloodlineData(SteamID);

            if (type < 0) {
                for (int i = 0; i < bld.strength.Length; i++) {
                    addEffectiveness(SteamID, i, bld.strength[i] / MaxBloodlineStrength);
                    adjustGrowth(SteamID, i, bld.strength[i] / MaxBloodlineStrength);
                    bld.strength[i] = 0;
                }

            }
            else {
                int i = type;
                addEffectiveness(SteamID, i, bld.strength[i] / MaxBloodlineStrength);
                adjustGrowth(SteamID, i, bld.strength[i] / MaxBloodlineStrength);
                bld.strength[i] = 0;
            }
            Database.playerBloodline[SteamID] = bld;
            return;
        }
        public static void adjustGrowth(ulong SteamID, int type, double value) {
            BloodlineData bld = getBloodlineData(SteamID);
            if (type >= 0 && type < bld.growth.Length) {
                if (growthPerEfficency >= 0) {
                    bld.growth[type] = Math.Min(maxBloodlineGrowth, bld.growth[type] + (value * growthPerEfficency));
                }
                else {
                    double gpe = -1 * growthPerEfficency;
                    value = value / (value + gpe);
                    bld.growth[type] = Math.Max(minBloodlineGrowth, bld.growth[type] * (1-value));
                }
            }
            Database.playerBloodline[SteamID] = bld;
            return;
        }
        public static void addEffectiveness(ulong SteamID, int type, double value) {

            BloodlineData bld = getBloodlineData(SteamID);
            
            if (type >= 0 && type < bld.efficency.Length) {
                bld.efficency[type] = Math.Min(maxBloodlineEfficency, bld.efficency[type] + value);
            }
            Database.playerBloodline[SteamID] = bld;
            return;
        }
        public static void BuffReceiver(DynamicBuffer<ModifyUnitStatBuff_DOTS> Buffer, Entity Owner, ulong SteamID) {
            Blood bloodline;
            int bloodlineIndex = -1;
            if (em.HasComponent<Blood>(Owner)) {
                bloodline = em.GetComponentData<Blood>(Owner);
                if (!bloodlineMap.TryGetValue(bloodline.BloodType, out bloodlineIndex)) {
                    if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity)) {
                        Output.SendLore(targetUserEntity, "Bloodline not found for guid of " + bloodline.BloodType.GuidHash);
                    }
                    return;
                }
            }
            else {
                return;
            }
            applyBloodlineBuffs(Buffer, bloodlineIndex, SteamID);
        }
        private static void applyBloodlineBuffs(DynamicBuffer<ModifyUnitStatBuff_DOTS> Buffer, int type, ulong SteamID) {
            int end = type + 1;
            bool isDracula = (type == 0 && draculaGetsAll);
            if (isDracula) { end = stats.Length; }
            for (; type < end; type++) {
                for (int i = 0; i < stats[type].Length; i++) {
                    double value = isDracula ? calcDraculaBuffValue(type, SteamID, i) : calcBuffValue(type, SteamID, i);
                    Buffer.Add(Helper.makeBuff(stats[type][i], value));
                }
            }
        }

        public static double calcDraculaBuffValue(int type, ulong SteamID, int stat) {
            BloodlineData bld = getBloodlineData(SteamID);
            double effectiveness;
            effectiveness = bld.efficency[type];
            effectiveness = Math.Max(1.0f, effectiveness);
            double value;
            double strength = bld.strength[type];
            if (strength < minStrengths[type][stat] || bld.strength[0] < minStrengths[type][stat]) {
                return stats[type][stat] == (int)UnitStatType.CooldownModifier ? 1.0 :  0.0;
            }
            strength *= (bld.strength[0] / 100) * bld.efficency[0] / stats.Length;

            value = Helper.calcBuffValue(strength, effectiveness, rates[type][stat], stats[type][stat]);
            return value;
        }

        public static double calcBuffValue(int type, ulong SteamID, int stat) {
            BloodlineData bld = getBloodlineData(SteamID);
            double effectiveness;
            effectiveness = bld.efficency[type];
            effectiveness = Math.Max(1.0f, effectiveness);
            double value;
            double strength = bld.strength[type];
            if(strength < minStrengths[type][stat]) {
                return stats[type][stat] == (int)UnitStatType.CooldownModifier ? 1.0 : 0.0;
            }
            value = Helper.calcBuffValue(strength, effectiveness, rates[type][stat], stats[type][stat]);
            return value;
        }

        public static void modBloodline(ulong SteamID, int Type, double value)
        {

            BloodlineData bld = getBloodlineData(SteamID);

            bld.strength[Type] += value;
            bld.strength[Type] = Math.Min(MaxBloodlineStrength, bld.strength[Type]);
            bld.strength[Type] = Math.Max(0, bld.strength[Type]);

            Database.playerBloodline[SteamID] = bld;
            return;
        }
    }
}

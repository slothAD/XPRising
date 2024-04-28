using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using RPGMods.Utils;

namespace RPGMods.Systems
{
    public class WeaponMasterSystem
    {
        public static EntityManager em = Plugin.Server.EntityManager;

        public static bool isMasteryEnabled = true;
        public static bool isDecaySystemEnabled = true;
        public static int MasteryCombatTick = 5;
        public static int MaxCombatTick = 12;
        public static double MasteryMultiplier = 1;
        public static int DecayInterval = 60;
        public static int Online_DecayValue = 0;
        public static int Offline_DecayValue = 1;
        public static double MaxMastery = 100;
        public static double VBloodMultiplier = 15;
        // Shou Change - make options for spell mastery with weapons active.
        public static Boolean spellMasteryNeedsNoneToUse = true;
        public static Boolean spellMasteryNeedsNoneToLearn = true;
        public static Boolean linearCDR = false;
        public static Boolean CDRStacks = false;

        private static readonly Random rand = new Random();

        // Idk how to do this elegantly and allow it to be bound to a config.
        public static int[] UnarmedStats = { 0, 5 };
        public static double[] UnarmedRates = { 0.25f, 0.01f };

        public static int[] SpearStats = { 0 };
        public static double[] SpearRates = { 0.25 };

        public static int[] SwordStats = { 0, 25 };
        public static double[] SwordRates = { 0.125, 0.125 };

        public static int[] ScytheStats = { 0 , 29 };
        public static double[] ScytheRates = { 0.125, 0.00125 };

        public static int[] CrossbowStats = { 29 };
        public static double[] CrossbowRates = { 0.0025 };

        public static int[] MaceStats = { 4 };
        public static double[] MaceRates = { 1f };

        public static int[] SlasherStats = { 29, 5 };
        public static double[] SlasherRates = { 0.00125, 0.005 };

        public static int[] AxeStats = { 0, 4 };
        public static double[] AxeRates = { 0.125f, 0.5f };

        public static int[] FishingPoleStats = { };
        public static double[] FishingPoleRates = { };

        public static int[] SpellStats = { 7 };
        public static double[] SpellRates = { 100 };

        public static int[] RapierStats = { 29, 30 };
        public static double[] RapierRates = { 0.00125, 0.00125 };

        public static int[] PistolStats = { 29, 30 };
        public static double[] PistolRates = { 0.00125, 0.0125 };

        public static int[] GreatSwordStats = { 0, 30 };
        public static double[] GreatSwordRates = { 0.125, 0.0125 };


        public static int[][] masteryStats = { SpellStats, UnarmedStats, SpearStats, SwordStats, ScytheStats, CrossbowStats, MaceStats, SlasherStats, AxeStats, FishingPoleStats, RapierStats, PistolStats, GreatSwordStats };
        public static double[][] masteryRates = { SpellRates, UnarmedRates, SpearRates, SwordRates, ScytheRates, CrossbowRates, MaceRates, SlasherRates, AxeRates, FishingPoleRates, RapierRates, PistolRates, GreatSwordRates };

        public static double maxEffectiveness = 10;
        public static bool effectivenessSubSystemEnabled = false;
        public static bool growthSubSystemEnabled = false;
        public static double minGrowth = 0.1f;
        public static double maxGrowth = 10;
        public static double growthPerEfficency = 1;
        
        public static Dictionary<string, int> nameMap = new Dictionary<string, int> {
            { "spell", 0 },
            { "magic", 0 },
            { "unarmed", (int) WeaponType.None + 1 },
            { "none", (int) WeaponType.None + 1 },
            { "spear", (int)WeaponType.Spear+1 },
            { "crossbow", (int) WeaponType.Crossbow + 1 },
            { "slashers", (int) WeaponType.Slashers + 1 },
            { "scythe", (int) WeaponType.Scythe + 1 },
            { "reaper", (int) WeaponType.Scythe + 1 },
            { "sword", (int) WeaponType.Sword + 1 },
            { "fishingpole", (int) WeaponType.FishingPole + 1 },
            { "mace", (int) WeaponType.Mace + 1 },
            { "axe", (int) WeaponType.Axes + 1 },
            { "greatsword", (int) WeaponType.GreatSword + 1 },
            { "rapier", (int) WeaponType.Rapier + 1 },
            { "pistol", (int) WeaponType.Pistols + 1 },
            { "dagger", (int) WeaponType.Sword + 1 },
            { "longbow", (int) WeaponType.Crossbow + 1 },
            { "xbow", (int) WeaponType.Crossbow + 1 }
        };
        public static Dictionary<int, string> typeToNameMap = new Dictionary<int, string> {
            { 0, "spell" },
            { (int) WeaponType.None + 1, "unarmed" },
            { (int) WeaponType.Spear + 1, "spear" },
            { (int) WeaponType.Crossbow + 1 , "crossbow" },
            { (int) WeaponType.Slashers + 1, "slashers" },
            { (int) WeaponType.Scythe + 1, "reaper" },
            { (int) WeaponType.Sword + 1, "sword" },
            { (int) WeaponType.FishingPole + 1, "fishingpole" },
            { (int) WeaponType.Mace + 1, "mace" },
            { (int) WeaponType.Axes + 1, "axe" },
            { (int) WeaponType.GreatSword + 1, "greatsword" },
            { (int) WeaponType.Rapier + 1, "rapier" },
            { (int) WeaponType.Pistols + 1, "pistol" }
        };

        public static void UpdateMastery(Entity Killer, Entity Victim)
        {
            if (Killer == Victim) return;
            if (em.HasComponent<Minion>(Victim)) return;

            Entity userEntity = em.GetComponentData<PlayerCharacter>(Killer).UserEntity;
            User User = em.GetComponentData<User>(userEntity);
            ulong SteamID = User.PlatformId;
            int weapon = (int)GetWeaponType(Killer)+1;

            double MasteryValue;
            double spellMasteryValue;
            var VictimStats = em.GetComponentData<UnitStats>(Victim);
            MasteryValue = VictimStats.PhysicalPower;
            spellMasteryValue = VictimStats.SpellPower;

            double weaponGrowth = 1;
            double spellGrowth = 1;

            if (growthSubSystemEnabled) {
                bool isGrowthFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData wd);
                if (!isGrowthFound)
                {
                    wd = new WeaponMasterData();
                    wd.mastery = new double[masteryStats.Length];
                    wd.efficency = new double[masteryStats.Length];
                    wd.growth = new double[masteryStats.Length];
                    for (int i = 0; i < masteryStats.Length; i++)
                    {
                        wd.mastery[i] = 0.0;
                        wd.efficency[i] = 1.0;
                        wd.growth[i] = 1.0;
                    }
                }
                weaponGrowth = wd.growth[weapon];
                spellGrowth = wd.growth[0];
            }

            MasteryValue = (MasteryValue * (rand.Next(10, 100) * 0.01) * weaponGrowth)/1000.0;
            spellMasteryValue = (MasteryValue * (rand.Next(10, 100) * 0.01) * spellGrowth)/1000.0;

            bool isVBlood;
            if (em.HasComponent<BloodConsumeSource>(Victim))
            {
                BloodConsumeSource BloodSource = em.GetComponentData<BloodConsumeSource>(Victim);
                isVBlood = BloodSource.UnitBloodType.Equals(Helper.vBloodType);
            }
            else
            {
                isVBlood = false;
            }

            if (isVBlood) {
                MasteryValue = (MasteryValue * VBloodMultiplier);
                spellMasteryValue = (spellMasteryValue * VBloodMultiplier);
            }

            if (em.HasComponent<PlayerCharacter>(Victim))
            {
                Equipment VictimGear = em.GetComponentData<Equipment>(Victim);
                var BonusMastery = VictimGear.ArmorLevel + VictimGear.WeaponLevel + VictimGear.SpellLevel;
                MasteryValue *= (1 + (BonusMastery * 0.01));
                spellMasteryValue *= (1 + (BonusMastery * 0.01));
            }

            MasteryValue = (MasteryValue * MasteryMultiplier);
            spellMasteryValue = (spellMasteryValue * MasteryMultiplier);
            modMastery(SteamID, weapon, MasteryValue);
            if (weapon == (((int)WeaponType.None) + 1) || !spellMasteryNeedsNoneToLearn) {
                modMastery(SteamID, 0, spellMasteryValue);
            }

            if (Database.player_log_mastery.TryGetValue(SteamID, out bool isLogging))
            {
                if (isLogging) {
                    string weaponTypeName = typeToName(weapon);
                    double currentMastery = masteryDataByType(weapon, SteamID);
                    Output.SendLore(userEntity, $"<color=#ffb700>Weapon mastery has increased by {MasteryValue:#.###}% [ {weaponTypeName}: {currentMastery:F2}% ]</color>");
                }
            }
        }

        public static void LoopMastery(Entity User, Entity Player)
        {
            User userData = em.GetComponentData<User>(User);
            ulong SteamID = userData.PlatformId;

            Cache.player_last_combat.TryGetValue(SteamID, out var LastCombat);
            TimeSpan elapsed_time = DateTime.Now - LastCombat;
            if (elapsed_time.TotalSeconds >= 10) Cache.player_combat_ticks[SteamID] = 0;
            if (elapsed_time.TotalSeconds * 0.2 < 1) return;

            Cache.player_last_combat[SteamID] = DateTime.Now;

            if (Cache.player_combat_ticks[SteamID] > MaxCombatTick) return;
            WeaponType WeaponType = GetWeaponType(Player);



            double weaponGrowth = 1;
            double spellGrowth = 1;

            if (growthSubSystemEnabled) {
                bool isGrowthFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData wd);
                if (!isGrowthFound) {
                    wd = new WeaponMasterData();
                    wd.mastery = new double[masteryStats.Length];
                    wd.efficency = new double[masteryStats.Length];
                    wd.growth = new double[masteryStats.Length];
                    for (int i = 0; i < masteryStats.Length; i++)
                    {
                        wd.mastery[i] = 0.0;
                        wd.efficency[i] = 1.0;
                        wd.growth[i] = 1.0;
                    }
                    Database.player_weaponmastery[SteamID] = wd;
                }
                weaponGrowth = wd.growth[(int)WeaponType+1];
                spellGrowth = wd.growth[0];
            }

            double MasteryValue = (MasteryCombatTick * MasteryMultiplier * weaponGrowth)/1000.0;
            double spellMasteryValue = (MasteryCombatTick * MasteryMultiplier * spellGrowth)/1000.0;
            Cache.player_combat_ticks[SteamID] += 1;
            
            modMastery(SteamID, (int)WeaponType+1, MasteryValue);
            if(WeaponType == WeaponType.None || !spellMasteryNeedsNoneToLearn) {
                modMastery(SteamID, 0, spellMasteryValue);

            }
        }

        public static void DecayMastery(Entity userEntity)
        {
            User Data = em.GetComponentData<User>(userEntity);
            var SteamID = Data.PlatformId;
            if (Database.player_decaymastery_logout.TryGetValue(SteamID, out var LastDecay)) {
                TimeSpan elapsed_time = DateTime.Now - LastDecay;
                if (elapsed_time.TotalSeconds < DecayInterval) return;

                int DecayTicks = (int)Math.Floor(elapsed_time.TotalSeconds / DecayInterval);
                if (DecayTicks > 0)
                {
                    int DecayValue = Offline_DecayValue * DecayTicks *-1;

                    Output.SendLore(userEntity, $"You've been sleeping for {(int)elapsed_time.TotalMinutes} minute(s). Your mastery has decayed by {DecayValue * 0.001:F3}%");

                    for(int i = 0; i < masteryStats.Length; i++){
                        modMastery(SteamID, i, DecayValue);
                    }
                }
            }
        }

        public static void BuffReceiver(Entity buffEntity, PrefabGUID GUID)
        {
            if (!GUID.Equals(Database.Buff.OutofCombat) && !GUID.Equals(Database.Buff.InCombat) && !GUID.Equals(Database.Buff.InCombat_PvP)) return;

            var Owner = em.GetComponentData<EntityOwner>(buffEntity).Owner;
            if (!em.HasComponent<PlayerCharacter>(Owner)) return;

            var userEntity = em.GetComponentData<PlayerCharacter>(Owner).UserEntity;
            var SteamID = em.GetComponentData<User>(userEntity).PlatformId;

            var Buffer = em.GetBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
            BuffReceiver(Buffer, Owner, SteamID);
        }

        public static void BuffReceiver(DynamicBuffer<ModifyUnitStatBuff_DOTS> Buffer, Entity Owner, ulong SteamID)
        {
            WeaponType WeaponType = GetWeaponType(Owner);
            if(WeaponType < 0 || (int)WeaponType >= masteryStats.Length-1){
                return;
            }
            var isMastered = ConvertMastery(SteamID, WeaponType, out var PMastery, out var SMastery);
            
            if (isMastered)
            {
                applyBuff(Buffer, (int)WeaponType+1, PMastery, SteamID);
                if(WeaponType == WeaponType.None || !spellMasteryNeedsNoneToUse){
                    applyBuff(Buffer, 0, SMastery, SteamID);
                }                 
            }
        }

        private static void applyBuff(DynamicBuffer<ModifyUnitStatBuff_DOTS> Buffer, int type, double mastery, ulong SteamID){
            for (int i = 0; i < masteryStats[type].Length; i++){
                double effectiveness = 1;
                if (Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData wd) && effectivenessSubSystemEnabled)
                    effectiveness = wd.efficency[type];
                effectiveness = Math.Max(1.0f, effectiveness);
                Buffer.Add(Helper.makeBuff(masteryStats[type][i], Helper.calcBuffValue(mastery, effectiveness, masteryRates[type][i], masteryStats[type][i])));/*
                var modType = ModificationType.Add;
                if ((UnitStatType)masteryStats[type][i] == UnitStatType.CooldownModifier)
                {
                    //value = 1.0f - value;
                    modType = ModificationType.Set;
                    if (CDRStacks)
                    {
                        modType = ModificationType.Multiply;
                    }
                }
                Buffer.Add(new ModifyUnitStatBuff_DOTS()
                {
                    StatType = (UnitStatType)masteryStats[type][i],
                    Value = (float)value,
                    ModificationType = modType,
                    Id = ModificationId.NewId(0)
                });*/
            }
        }

        public static double calcBuffValue(int type, double mastery, ulong SteamID, int stat){
            double effectiveness = 1;
            if (Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData wd) && effectivenessSubSystemEnabled)
                effectiveness = wd.efficency[type];
            effectiveness = Math.Max(1.0f, effectiveness);
            if (type >= masteryRates.Length){
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity))
                    Output.SendLore(targetUserEntity, $"Type {type} out of bounds for masteryRates, max of {masteryRates.Length - 1}");
                return 0.0f;
            }
            if (stat >= masteryRates[type].Length){
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity))
                    Output.SendLore(targetUserEntity, $"stat {stat} out of bounds for masteryRates for type {type}, max of {masteryRates[type].Length - 1}");
                return 0.0f;
            }
            if (type >= masteryStats.Length){
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity))
                    Output.SendLore(targetUserEntity, $"Type {type} out of bounds for masteryStats, max of {masteryStats.Length - 1}");
                return 0.0f;
            }
            if (stat >= masteryStats[type].Length){
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity))
                    Output.SendLore(targetUserEntity, $"stat {stat} out of bounds for masteryStats for type {type}, max of {masteryStats[type].Length - 1}");
                return 0.0f;
            }
            double value = mastery * masteryRates[type][stat] * effectiveness;
            /*if ((UnitStatType)masteryStats[type][stat] == UnitStatType.CooldownModifier){
                if (linearCDR){
                    value = mastery * effectiveness;
                    value = value / (value + masteryRates[type][stat]);
                }
                else{
                    value = (mastery*effectiveness)/(masteryRates[type][stat]*2);
                }
            }*/
            value = Helper.calcBuffValue(mastery, effectiveness, masteryRates[type][stat], masteryStats[type][stat]);
            return value;
        }

        public static bool ConvertMastery(ulong SteamID, WeaponType weaponType, out double MasteryValue, out double MasterySpellValue){
            MasteryValue = 0;
            MasterySpellValue = 0;

            bool isFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData Mastery);
            if (!isFound) return false;


            MasteryValue = Mastery.mastery[(int)weaponType + 1];
            MasterySpellValue = Mastery.mastery[0];

            if (MasteryValue < 0) MasteryValue = 0;
            if (MasterySpellValue < 0) MasterySpellValue = 0;
            return true;
        }

        public static void modMastery(ulong SteamID, int Type, double Value)
        {
            int NoneExpertise = 0;
            if (Type == (int)WeaponType.None+1){
                if (Value > 0) Value = Value * 2;
            }
            WeaponMasterData Mastery;
            try {
                bool isPlayerFound = Database.player_weaponmastery.TryGetValue(SteamID, out Mastery);
                Mastery.mastery[Type] += Value;                
                Mastery.mastery[Type] = Math.Min(Mastery.mastery[Type], MaxMastery);
            }
            catch (NullReferenceException nre) {
                Mastery = new WeaponMasterData();
                Mastery.mastery = new double[masteryStats.Length];
                Mastery.efficency = new double[masteryStats.Length];
                Mastery.growth = new double[masteryStats.Length];
                for (int i = 0; i < masteryStats.Length; i++)
                {
                    Mastery.mastery[i] = 0.0;
                    Mastery.efficency[i] = 1.0;
                    Mastery.growth[i] = 1.0;
                }
                if (NoneExpertise < 0) NoneExpertise = 0;
                if (Value < 0) Value = 0;
                Mastery.mastery[Type] += Value;
                Plugin.Logger.LogInfo(DateTime.Now + ": Null Ref trying to get mastery, reset it instead: " + nre.Message);
            }
            if (Mastery.mastery[Type] < 0) Mastery.mastery[Type] = 0;
            Database.player_weaponmastery[SteamID] = Mastery;
            return;
        }

        public static void resetMastery(ulong SteamID, int type) {
            if (!effectivenessSubSystemEnabled) {
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity)) {
                    Output.SendLore(targetUserEntity, $"Effectiveness Subsystem disabled, not resetting mastery.");
                }
                return;
            }
            bool isPlayerFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData Mastery);
            if (isPlayerFound) {
                if (type < 0) {
                    for (int i = 0; i < Mastery.mastery.Length; i++) {
                        addMasteryEffectiveness(SteamID, i, Mastery.mastery[i] / MaxMastery);
                        adjustGrowth(SteamID, i, Mastery.mastery[i] / MaxMastery);
                        Mastery.mastery[i] = 0;
                    }
                    
                }
                else {
                    addMasteryEffectiveness(SteamID, type, Mastery.mastery[type]/MaxMastery);
                    adjustGrowth(SteamID, type, Mastery.mastery[type] / MaxMastery);
                    Mastery.mastery[type] = 0;
                }
                Database.player_weaponmastery[SteamID] = Mastery;
            }
            return;
        }

        public static void adjustGrowth(ulong SteamID, int type, double value) {
            bool isPlayerFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData wd);
            if (isPlayerFound) {
                if (type >= 0 && type < wd.growth.Length) {
                    if(growthPerEfficency >= 0) {
                        wd.growth[type] = Math.Min(maxGrowth, wd.growth[type] + (value*growthPerEfficency));
                    }
                    else {
                        double gpe = -1 * growthPerEfficency;
                        value = value / (value + gpe);
                        wd.growth[type] = Math.Max(minGrowth, wd.growth[type] * (1-value) );
                    }
                }
            }
            else {

                wd = new WeaponMasterData();
                wd.mastery = new double[masteryStats.Length];
                wd.efficency = new double[masteryStats.Length];
                wd.growth = new double[masteryStats.Length];
                for (int i = 0; i < masteryStats.Length; i++)
                {
                    wd.mastery[i] = 0.0;
                    wd.efficency[i] = 1.0;
                    wd.growth[i] = 1.0;
                }

                if (type >= 0 && type < wd.growth.Length){
                    if (growthPerEfficency >= 0){
                        wd.growth[type] = Math.Min(maxGrowth, wd.growth[type] + (value * growthPerEfficency));
                    }
                    else{
                        double gpe = -1 * growthPerEfficency;
                        value = value / (value + growthPerEfficency);
                        wd.growth[type] = Math.Max(minGrowth, wd.growth[type] * value);
                    }
                }

            }
            Database.player_weaponmastery[SteamID] = wd;
            return;
        }

        public static void addMasteryEffectiveness(ulong SteamID, int type, double value) {
            bool isPlayerFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData wd);
            if (isPlayerFound) {
                if (type >= 0 && type < wd.efficency.Length) {
                    wd.efficency[type] = Math.Min(maxEffectiveness, wd.efficency[type] + value);
                }
            }
            else{
                wd = new WeaponMasterData();
                wd.mastery = new double[masteryStats.Length];
                wd.efficency = new double[masteryStats.Length];
                wd.growth = new double[masteryStats.Length];
                for (int i = 0; i < masteryStats.Length; i++)
                {
                    wd.mastery[i] = 0.0;
                    wd.efficency[i] = 1.0;
                    wd.growth[i] = 1.0;
                }
                if (type >= 0 && type < wd.efficency.Length) {
                    wd.efficency[type] = Math.Min(maxEffectiveness, wd.efficency[type] + value);
                }

            }
            Database.player_weaponmastery[SteamID] = wd;
            return;
        }

        public static WeaponType GetWeaponType(Entity Player)
        {
            Entity WeaponEntity = em.GetComponentData<Equipment>(Player).WeaponSlotEntity._Entity;
            WeaponType WeaponType = WeaponType.None;
            if (em.HasComponent<EquippableData>(WeaponEntity))
            {
                EquippableData WeaponData = em.GetComponentData<EquippableData>(WeaponEntity);
                WeaponType = WeaponData.WeaponType;
            }
            return WeaponType;
        }

        public static string typeToName(int type)
        {
            if(type == -1) {
                return "All";
            }
            bool nameFound = typeToNameMap.TryGetValue(type, out string weaponName);
            if (!nameFound) {
                weaponName = "Unknown";
            }
            return weaponName;
        }

        public static double masteryDataByType(int type, ulong SteamID)
        {
            bool isFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData Mastery);
            if (!isFound) return (MaxMastery*-10);
            return Mastery.mastery[type];
        }

        public static void SaveWeaponMastery(string saveFolder)
        {
            File.WriteAllText(saveFolder+"weaponMastery.json", JsonSerializer.Serialize(Database.player_weaponmastery, Database.JSON_options));
            File.WriteAllText(saveFolder+"mastery_decay.json", JsonSerializer.Serialize(Database.player_decaymastery_logout, Database.JSON_options));
            File.WriteAllText(saveFolder +"player_log_mastery.json", JsonSerializer.Serialize(Database.player_log_mastery, Database.JSON_options));
        }

        public static void LoadWeaponMastery() {

            string specificName = "weaponMastery.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder,specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            string json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+"weaponMastery.json");
            try {
                Database.player_weaponmastery = JsonSerializer.Deserialize<Dictionary<ulong, WeaponMasterData>>(json);
                if (Database.player_weaponmastery == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.player_weaponmastery = JsonSerializer.Deserialize<Dictionary<ulong, WeaponMasterData>>(json);
                }
                Plugin.Logger.LogWarning("WeaponMastery DB Populated.");
            } catch {
                Database.player_weaponmastery = new Dictionary<ulong, WeaponMasterData>();
                Plugin.Logger.LogWarning("WeaponMastery DB Created.");
            }

            specificName = "weaponMastery.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder,specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+ specificName);
            try{
                Database.player_decaymastery_logout = JsonSerializer.Deserialize<Dictionary<ulong, DateTime>>(json);
                if (Database.player_decaymastery_logout == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.player_decaymastery_logout = JsonSerializer.Deserialize<Dictionary<ulong, DateTime>>(json);
                }
                Plugin.Logger.LogWarning("WeaponMasteryDecay DB Populated.");
            }
            catch{
                Database.player_decaymastery_logout = new Dictionary<ulong, DateTime>();
                Plugin.Logger.LogWarning("WeaponMasteryDecay DB Created.");
            }

            specificName = "player_log_mastery.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder,specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+ specificName);
            try{
                Database.player_log_mastery = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                if (Database.player_log_mastery == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.player_log_mastery = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                }
                Plugin.Logger.LogWarning("Player_LogMastery_Switch DB Populated.");
            }
            catch{
                Database.player_log_mastery = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("Player_LogMastery_Switch DB Created.");
            }


        }
    }
}

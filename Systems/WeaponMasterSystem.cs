using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using RPGMods.Utils;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace RPGMods.Systems
{
    public class WeaponMasterSystem
    {
        public static EntityManager em = Plugin.Server.EntityManager;

        public static bool isMasteryEnabled = true;
        public static bool isDecaySystemEnabled = true;
        public static int MasteryCombatTick = 5;
        public static int MaxCombatTick = 12;
        public static float MasteryMultiplier = 1;
        public static int DecayInterval = 60;
        public static int Online_DecayValue = 0;
        public static int Offline_DecayValue = 1;
        public static int MaxMastery = 100000;
        public static float VBloodMultiplier = 15;
        // Shou Change - make options for spell mastery with weapons active.
        public static Boolean spellMasteryNeedsNoneToUse = true;
        public static Boolean spellMasteryNeedsNoneToLearn = true;
        public static Boolean linearCDR = false;
        public static Boolean CDRStacks = false;

        private static PrefabGUID vBloodType = new PrefabGUID(1557174542);

        private static readonly Random rand = new Random();

        // Idk how to do this elegantly and allow it to be bound to a config.
        public static int[] UnarmedStats = { 0, 5 };
        public static float[] UnarmedRates = { 0.25f, 0.01f };

        public static int[] SpearStats = { 0 };
        public static float[] SpearRates = { 0.25f };

        public static int[] SwordStats = { 0, 25 };
        public static float[] SwordRates = { 0.125f, 0.125f };

        public static int[] ScytheStats = { 0 , 29 };
        public static float[] ScytheRates = { 0.125f, 0.00125f };

        public static int[] CrossbowStats = { 29 };
        public static float[] CrossbowRates = { 0.0025f };

        public static int[] MaceStats = { 4 };
        public static float[] MaceRates = { 1f };

        public static int[] SlasherStats = { 29, 5 };
        public static float[] SlasherRates = { 0.00125f, 0.005f };

        public static int[] AxeStats = { 0, 4 };
        public static float[] AxeRates = { 0.125f, 0.5f };

        public static int[] FishingPoleStats = {  };
        public static float[] FishingPoleRates = {  };

        public static int[][] masteryStats = { UnarmedStats, SpearStats, SwordStats, ScytheStats, CrossbowStats, MaceStats, SlasherStats, AxeStats, FishingPoleStats };
        public static float[][] masteryRates = { UnarmedRates, SpearRates, SwordRates, ScytheRates, CrossbowRates, MaceRates, SlasherRates, AxeRates, FishingPoleRates };


        public static void UpdateMastery(Entity Killer, Entity Victim)
        {
            if (Killer == Victim) return;
            if (em.HasComponent<Minion>(Victim)) return;

            Entity userEntity = em.GetComponentData<PlayerCharacter>(Killer).UserEntity._Entity;
            User User = em.GetComponentData<User>(userEntity);
            ulong SteamID = User.PlatformId;
            WeaponType WeaponType = GetWeaponType(Killer);

            int MasteryValue;
            var VictimStats = em.GetComponentData<UnitStats>(Victim);
            if (WeaponType == WeaponType.None) MasteryValue = (int)VictimStats.SpellPower;
            else MasteryValue = (int)VictimStats.PhysicalPower;

            MasteryValue = (int)(MasteryValue * (rand.Next(10, 100) * 0.01));

            bool isVBlood;
            if (em.HasComponent<BloodConsumeSource>(Victim))
            {
                BloodConsumeSource BloodSource = em.GetComponentData<BloodConsumeSource>(Victim);
                isVBlood = BloodSource.UnitBloodType.Equals(vBloodType);
            }
            else
            {
                isVBlood = false;
            }

            if (isVBlood) MasteryValue = (int)(MasteryValue * VBloodMultiplier);

            if (em.HasComponent<PlayerCharacter>(Victim))
            {
                Equipment VictimGear = em.GetComponentData<Equipment>(Victim);
                var BonusMastery = VictimGear.ArmorLevel + VictimGear.WeaponLevel + VictimGear.SpellLevel;
                MasteryValue *= (int)(1 + (BonusMastery * 0.01));
            }

            MasteryValue = (int)(MasteryValue * MasteryMultiplier);
            SetMastery(SteamID, WeaponType, MasteryValue);

            if (Database.player_log_mastery.TryGetValue(SteamID, out bool isLogging))
            {
                if (isLogging)
                {
                    Output.SendLore(userEntity, $"<color=#ffb700>Weapon mastery has increased by {MasteryValue * 0.001}%</color>");
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

            int MasteryValue = (int)(MasteryCombatTick * MasteryMultiplier);
            Cache.player_combat_ticks[SteamID] += 1;
            
            SetMastery(SteamID, WeaponType, MasteryValue);
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

                    Output.SendLore(userEntity, $"You've been sleeping for {(int)elapsed_time.TotalMinutes} minute(s). Your mastery has decayed by {DecayValue * 0.001}%");

                    foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
                    {
                        SetMastery(SteamID, type, DecayValue);
                    }
                }
            }
        }

        public static void BuffReceiver(Entity buffEntity, PrefabGUID GUID)
        {
            if (!GUID.Equals(Database.Buff.OutofCombat) && !GUID.Equals(Database.Buff.InCombat) && !GUID.Equals(Database.Buff.InCombat_PvP)) return;

            var Owner = em.GetComponentData<EntityOwner>(buffEntity).Owner;
            if (!em.HasComponent<PlayerCharacter>(Owner)) return;

            var userEntity = em.GetComponentData<PlayerCharacter>(Owner).UserEntity._Entity;
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
                applyBuff(Buffer, (int)WeaponType, PMastery, SteamID);
                if((spellMasteryNeedsNoneToUse && WeaponType == WeaponType.None) || !spellMasteryNeedsNoneToUse)
                {
                    applyBuff(Buffer, masteryStats.Length-1, SMastery, SteamID);
                }                 
            }
        }

        private static void applyBuff(DynamicBuffer<ModifyUnitStatBuff_DOTS> Buffer, int type, float mastery, ulong SteamID){
            for (int i = 0; i < masteryStats[type].Length; i++)
            {
                float value = calcBuffValue(type, mastery, SteamID, i);
                var modType = ModificationType.Add;
                if ((UnitStatType)masteryStats[type][i] == UnitStatType.CooldownModifier)
                {
                    value = 1.0f - value;
                    modType = ModificationType.Set;
                    if (CDRStacks)
                    {
                        modType = ModificationType.Multiply;
                    }
                }
                Buffer.Add(new ModifyUnitStatBuff_DOTS()
                {
                    StatType = (UnitStatType)masteryStats[type][i],
                    Value = value,
                    ModificationType = modType,
                    Id = ModificationId.NewId(0)
                }); ;
            }
        }

        public static float calcBuffValue(int type, float mastery, ulong SteamID, int stat)
        {
            float effectiveness = 1;
            if (Database.playerWeaponEffectiveness.TryGetValue(SteamID, out WeaponMasterEffectivenessData effectivenessData))
                effectiveness = effectivenessData.data[type];
            if (type >= masteryRates.Length)
            {
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity))
                    Output.SendLore(targetUserEntity, $"Type {type} out of bounds for masteryRates, max of {masteryRates.Length - 1}");
                return 0.0f;
            }
            if (stat >= masteryRates[type].Length)
            {
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity))
                    Output.SendLore(targetUserEntity, $"stat {stat} out of bounds for masteryRates for type {type}, max of {masteryRates[type].Length - 1}");
                return 0.0f;
            }
            if (type >= masteryStats.Length)
            {
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity))
                    Output.SendLore(targetUserEntity, $"Type {type} out of bounds for masteryStats, max of {masteryStats.Length - 1}");
                return 0.0f;
            }
            if (stat >= masteryStats[type].Length)
            {
                if (Helper.FindPlayer(SteamID, true, out var targetEntity, out var targetUserEntity))
                    Output.SendLore(targetUserEntity, $"stat {stat} out of bounds for masteryStats for type {type}, max of {masteryStats[type].Length - 1}");
                return 0.0f;
            }
            float value = mastery * masteryRates[type][stat] * effectiveness;
            if ((UnitStatType)masteryStats[type][stat] == UnitStatType.CooldownModifier)
            {
                if (linearCDR)
                {
                    value = mastery * effectiveness;
                    value = value / (value + masteryRates[type][stat]);
                }
            }
            return value;
        }

        public static bool ConvertMastery(ulong SteamID, WeaponType weaponType, out float MasteryValue, out float MasterySpellValue)
        {
            MasteryValue = 0;
            MasterySpellValue = 0;

            bool isFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData Mastery);
            if (!isFound) return false;

            switch (weaponType)
            {
                case WeaponType.Sword:
                    MasteryValue = Mastery.Sword; break;
                case WeaponType.Spear:
                    MasteryValue = Mastery.Spear; break;
                case WeaponType.None:
                    MasteryValue = Mastery.None;
                    if (spellMasteryNeedsNoneToUse){
                        MasterySpellValue = Mastery.Spell;
                    }
                    break;
                case WeaponType.Scythe:
                    MasteryValue = Mastery.Scythe; break;
                case WeaponType.Axes:
                    MasteryValue = Mastery.Axes; break;
                case WeaponType.Mace:
                    MasteryValue = Mastery.Mace; break;
                case WeaponType.Crossbow:
                    MasteryValue = Mastery.Crossbow; break;
                case WeaponType.Slashers:
                    MasteryValue = Mastery.Slashers; break;
                case WeaponType.FishingPole:
                    MasteryValue = Mastery.FishingPole; break;
            }
            if (!spellMasteryNeedsNoneToUse){
                MasterySpellValue = Mastery.Spell;
            }
            if (MasteryValue > 0) MasteryValue = (float)(MasteryValue * 0.001);
            if (MasterySpellValue > 0) MasterySpellValue = (float)(MasterySpellValue * 0.001);
            return true;
        }

        public static void SetMastery(ulong SteamID, WeaponType Type, int Value)
        {
            int NoneExpertise = 0;
            if (Type == WeaponType.None)
            {
                if (Value > 0) NoneExpertise = Value * 2;
                else NoneExpertise = Value;
            }
            bool isPlayerFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData Mastery);
            if (isPlayerFound)
            {
                switch (Type)
                {
                    case WeaponType.Sword:
                        if (Mastery.Sword + Value > MaxMastery) Mastery.Sword = MaxMastery;
                        else if (Mastery.Sword + Value < 0) Mastery.Sword = 0;
                        else Mastery.Sword += Value;
                        break;
                    case WeaponType.Spear:
                        if (Mastery.Spear + Value >= MaxMastery) Mastery.Spear = MaxMastery;
                        else if (Mastery.Spear + Value < 0) Mastery.Spear = 0;
                        else Mastery.Spear += Value;
                        break;
                    case WeaponType.None:
                        if (Mastery.None + NoneExpertise > MaxMastery) Mastery.None = MaxMastery;
                        else if (Mastery.None + NoneExpertise < 0) Mastery.None = 0;
                        else Mastery.None += NoneExpertise;
                        if (spellMasteryNeedsNoneToLearn){
                            if (Mastery.Spell + Value > MaxMastery) Mastery.Spell = MaxMastery;
                            else if (Mastery.Spell + Value < 0) Mastery.Spell = 0;
                            else Mastery.Spell += Value;
                        }
                        break;
                    case WeaponType.Scythe:
                        if (Mastery.Scythe + Value >= MaxMastery) Mastery.Scythe = MaxMastery;
                        else if (Mastery.Scythe + Value < 0) Mastery.Scythe = 0;
                        else Mastery.Scythe += Value;
                        break;
                    case WeaponType.Axes:
                        if (Mastery.Axes + Value >= MaxMastery) Mastery.Axes = MaxMastery;
                        else if (Mastery.Axes + Value < 0) Mastery.Axes = 0;
                        else Mastery.Axes += Value;
                        break;
                    case WeaponType.Mace:
                        if (Mastery.Mace + Value >= MaxMastery) Mastery.Mace = MaxMastery;
                        else if (Mastery.Mace + Value < 0) Mastery.Mace = 0;
                        else Mastery.Mace += Value;
                        break;
                    case WeaponType.Crossbow:
                        if (Mastery.Crossbow + Value >= MaxMastery) Mastery.Crossbow = MaxMastery;
                        else if (Mastery.Crossbow + Value < 0) Mastery.Crossbow = 0;
                        else Mastery.Crossbow += Value;
                        break;
                    case WeaponType.Slashers:
                        if (Mastery.Slashers + Value >= MaxMastery) Mastery.Slashers = MaxMastery;
                        else if (Mastery.Slashers + Value < 0) Mastery.Slashers = 0;
                        else Mastery.Slashers += Value;
                        break;
                    case WeaponType.FishingPole:
                        if (Mastery.FishingPole + Value >= MaxMastery) Mastery.FishingPole = MaxMastery;
                        else if (Mastery.FishingPole + Value < 0) Mastery.FishingPole = 0;
                        else Mastery.FishingPole += Value;
                        break;
                }
                if (!spellMasteryNeedsNoneToLearn){
                    if (Mastery.Spell + Value > MaxMastery) Mastery.Spell = MaxMastery;
                    else if (Mastery.Spell + Value < 0) Mastery.Spell = 0;
                    else Mastery.Spell += Value;
                }
            }
            else
            {
                Mastery = new WeaponMasterData();

                if (NoneExpertise < 0) NoneExpertise = 0;
                if (Value < 0) Value = 0;

                switch (Type)
                {
                    case WeaponType.Sword:
                        Mastery.Sword += Value; break;
                    case WeaponType.Spear:
                        Mastery.Spear += Value; break;
                    case WeaponType.None:
                        Mastery.None += NoneExpertise;
                        if (spellMasteryNeedsNoneToLearn){
                            Mastery.Spell += Value;
                        }
                        break;
                    case WeaponType.Scythe:
                        Mastery.Scythe += Value; break;
                    case WeaponType.Axes:
                        Mastery.Axes += Value; break;
                    case WeaponType.Mace:
                        Mastery.Mace += Value; break;
                    case WeaponType.Crossbow:
                        Mastery.Crossbow += Value; break;
                    case WeaponType.Slashers:
                        Mastery.Slashers += Value; break;
                    case WeaponType.FishingPole:
                        Mastery.FishingPole += Value; break;
                }
                if (!spellMasteryNeedsNoneToLearn){
                    Mastery.Spell += Value;
                }
            }
            Database.player_weaponmastery[SteamID] = Mastery;
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

        public static string typeToName(WeaponType type)
        {

            string weaponName = "Unknown";
            switch (type)
            {
                case WeaponType.None:
                    weaponName = "Unarmed";
                    break;
                case WeaponType.Spear:
                    weaponName = "Spear";
                    break;
                case WeaponType.Sword:
                    weaponName = "Sword";
                    break;
                case WeaponType.Scythe:
                    weaponName = "Scythe";
                    break;
                case WeaponType.Crossbow:
                    weaponName = "Crossbow";
                    break;
                case WeaponType.Mace:
                    weaponName = "Mace";
                    break;
                case WeaponType.Slashers:
                    weaponName = "Slashers";
                    break;
                case WeaponType.Axes:
                    weaponName = "Axes";
                    break;
                case WeaponType.FishingPole:
                    weaponName = "Fishing Rod";
                    break;
            }
            return weaponName;
        }

        public static int masteryDataByType(WeaponType type, ulong SteamID)
        {
            int mastery = 0;
            bool isFound = Database.player_weaponmastery.TryGetValue(SteamID, out WeaponMasterData Mastery);
            if (!isFound) return (MaxMastery*-10);
            switch (type)
            {
                case WeaponType.None:
                    mastery = Mastery.None;
                    break;
                case WeaponType.Spear:
                    mastery = Mastery.Spear;
                    break;
                case WeaponType.Sword:
                    mastery = Mastery.Sword;
                    break;
                case WeaponType.Scythe:
                    mastery = Mastery.Scythe;
                    break;
                case WeaponType.Crossbow:
                    mastery = Mastery.Crossbow;
                    break;
                case WeaponType.Mace:
                    mastery = Mastery.Mace;
                    break;
                case WeaponType.Slashers:
                    mastery = Mastery.Slashers;
                    break;
                case WeaponType.Axes:
                    mastery = Mastery.Axes;
                    break;
                case WeaponType.FishingPole:
                    mastery = Mastery.FishingPole;
                    break;
            }
            return mastery;
        }

        public static void SaveWeaponMastery()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/weapon_mastery.json", JsonSerializer.Serialize(Database.player_weaponmastery, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/mastery_decay.json", JsonSerializer.Serialize(Database.player_decaymastery_logout, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/player_log_mastery.json", JsonSerializer.Serialize(Database.player_log_mastery, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/weapon_Mastery_Effectiveness.json", JsonSerializer.Serialize(Database.playerWeaponEffectiveness, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/weapon_Mastery_Growth.json", JsonSerializer.Serialize(Database.playerWeaponGrowth, Database.JSON_options));
        }

        public static void LoadWeaponMastery()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/weapon_mastery.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/weapon_mastery.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/weapon_mastery.json");
            try
            {
                Database.player_weaponmastery = JsonSerializer.Deserialize<Dictionary<ulong, WeaponMasterData>>(json);
                Plugin.Logger.LogWarning("WeaponMastery DB Populated.");
            }
            catch
            {
                Database.player_weaponmastery = new Dictionary<ulong, WeaponMasterData>();
                Plugin.Logger.LogWarning("WeaponMastery DB Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/mastery_decay.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/mastery_decay.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/mastery_decay.json");
            try
            {
                Database.player_decaymastery_logout = JsonSerializer.Deserialize<Dictionary<ulong, DateTime>>(json);
                Plugin.Logger.LogWarning("WeaponMasteryDecay DB Populated.");
            }
            catch
            {
                Database.player_decaymastery_logout = new Dictionary<ulong, DateTime>();
                Plugin.Logger.LogWarning("WeaponMasteryDecay DB Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/player_log_mastery.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/player_log_mastery.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/player_log_mastery.json");
            try
            {
                Database.player_log_mastery = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("Player_LogMastery_Switch DB Populated.");
            }
            catch
            {
                Database.player_log_mastery = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("Player_LogMastery_Switch DB Created.");
            }


            if (!File.Exists("BepInEx/config/RPGMods/Saves/weapon_Mastery_Effectiveness.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/weapon_Mastery_Effectiveness.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/weapon_Mastery_Effectiveness.json");
            try
            {
                Database.playerWeaponEffectiveness = JsonSerializer.Deserialize<Dictionary<ulong, WeaponMasterEffectivenessData>>(json);
                Plugin.Logger.LogWarning("WeaponMasteryEffectiveness DB Populated.");
            }
            catch
            {
                Database.playerWeaponEffectiveness = new Dictionary<ulong, WeaponMasterEffectivenessData>();
                Plugin.Logger.LogWarning("WeaponMasteryEffectiveness DB Created.");
            }


            if (!File.Exists("BepInEx/config/RPGMods/Saves/weapon_Mastery_Growth.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/weapon_Mastery_Growth.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/weapon_Mastery_Growth.json");
            try
            {
                Database.playerWeaponGrowth = JsonSerializer.Deserialize<Dictionary<ulong, WeaponMasterGrowthData>>(json);
                Plugin.Logger.LogWarning("WeaponMasteryGrowth DB Populated.");
            }
            catch
            {
                Database.playerWeaponGrowth = new Dictionary<ulong, WeaponMasterGrowthData>();
                Plugin.Logger.LogWarning("WeaponMasteryGrowth DB Created.");
            }

        }
    }
}

using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using RPGMods.Utils;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;
using Cache = RPGMods.Utils.Cache;
using Unity.Entities.UniversalDelegates;

namespace RPGMods.Systems
{
    public class ExperienceSystem
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        public static bool isEXPActive = true;
        public static float EXPMultiplier = 1;
        public static float VBloodMultiplier = 15;
        public static float EXPConstant = 0.1f;
        public static int EXPPower = 2;
        public static int MaxLevel = 80;
        public static double GroupModifier = 0.75;
        public static float GroupMaxDistance = 50;
        public static bool ShouldAllowGearLevel = true;
        public static bool LevelRewardsOn = true;
        public static bool easyLvl15 = true;

        public static double EXPLostOnDeath = 0.10;

        private static readonly PrefabGUID vBloodType = new PrefabGUID(1557174542);
        public static bool xpLogging = true;
        public static void EXPMonitor(Entity killerEntity, Entity victimEntity)
        {
            //-- Check victim is not a summon
            if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Kill Found, Handling XP");
            if (entityManager.HasComponent<Minion>(victimEntity)) return;

            if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Not a Minion, checking level");
            //-- Check victim has a level
            if (!entityManager.HasComponent<UnitLevel>(victimEntity)) return;

            if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Has Level, fetching allies");
            //-- Must be executed from main thread
            Helper.GetAllies(killerEntity, out var PlayerGroup);
            //-- ---------------------------------
            if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Allies Fetched, player group is " + PlayerGroup + ", Total ally count of " + PlayerGroup.AllyCount);
            UpdateEXP(killerEntity, victimEntity, PlayerGroup);
        }

        public static void UpdateEXP(Entity killerEntity, Entity victimEntity, PlayerGroup PlayerGroup) {
            if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Began XP Update, player group is size " + PlayerGroup.AllyCount);
            PlayerCharacter player = entityManager.GetComponentData<PlayerCharacter>(killerEntity);
            Entity userEntity = player.UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong SteamID = user.PlatformId;
            if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": SteamID of killer is " + SteamID);

            int player_level = 0;
            if (Database.player_experience.TryGetValue(SteamID, out int exp))
            {
                player_level = convertXpToLevel(exp);
                if (exp >= convertLevelToXp(MaxLevel)) return;
            }

            UnitLevel UnitLevel = entityManager.GetComponentData<UnitLevel>(victimEntity);

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

            int EXPGained;
            if (isVBlood) EXPGained = (int)(UnitLevel.Level * VBloodMultiplier);
            else EXPGained = UnitLevel.Level;

            int level_diff = UnitLevel.Level - player_level;
            //if (level_diff > 10) level_diff = 10;

            if (level_diff >= 0) EXPGained = (int)(EXPGained * (1 + level_diff * 0.1) * EXPMultiplier);
            else{
                float xpMult = level_diff * -1.0f;
                xpMult = xpMult / (xpMult + 10.0f);
                EXPGained = (int)(EXPGained * (1-xpMult));
            }
            /*
            else if (level_diff <= -20) EXPGained = (int) Math.Ceiling(EXPGained * 0.10 * EXPMultiplier);
            else if (level_diff <= -15) EXPGained = (int) Math.Ceiling(EXPGained * 0.25 * EXPMultiplier);
            else if (level_diff <= -10) EXPGained = (int) Math.Ceiling(EXPGained * 0.50 * EXPMultiplier);
            else if (level_diff <= -5) EXPGained = (int) Math.Ceiling(EXPGained * 0.75 * EXPMultiplier);
            else EXPGained = (int)(EXPGained * EXPMultiplier);*/

            if (PlayerGroup.AllyCount > 0) {
                if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Beginning XP Sharing");
                List<Entity> CloseAllies = new();
                if (Cache.PlayerLocations.TryGetValue(killerEntity, out var playerPos)) {
                    if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Got Player Position from Cache");
                    foreach (var ally in PlayerGroup.Allies) {
                        if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Iterating over allies, ally is " + ally.GetHashCode());
                        if (Cache.PlayerLocations.TryGetValue(ally.Value, out var allyPos)) {
                            if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Got Ally Position");
                            var Distance = math.distance(playerPos.Position.xz, allyPos.Position.xz);
                            if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Distance is " + Distance + ", Max Distance is " + GroupMaxDistance);
                            if (Distance <= GroupMaxDistance) {
                                if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Adjusting XP Gain and adding ally to the close allies list");
                                EXPGained = (int)(EXPGained * GroupModifier);
                                CloseAllies.Add(ally.Key);
                            }
                        }
                    }
                }

                if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Running Share EXP");
                foreach (var teammate in CloseAllies) {
                    ShareEXP(teammate, EXPGained);
                }
            }

            if (exp <= 0) Database.player_experience[SteamID] = EXPGained;
            else Database.player_experience[SteamID] = exp + EXPGained;

            SetLevel(killerEntity, userEntity, SteamID);
            if (Database.player_log_exp.TryGetValue(SteamID, out bool isLogging))
            {
                if (isLogging)
                {
                    Output.SendLore(userEntity, $"<color=#ffdd00>You gain {EXPGained} experience points by slaying a Lv.{UnitLevel.Level} enemy.</color>");
                }
            }
        }

        public static void ShareEXP(Entity user, int EXPGain) {
            if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Getting User Component from Ally");
            var user_component = entityManager.GetComponentData<User>(user);
            if (EXPGain > 0) {
                if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Trying to get allies exp");
                Database.player_experience.TryGetValue(user_component.PlatformId, out var exp);
                if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Trying to set allies exp");
                Database.player_experience[user_component.PlatformId] = exp + EXPGain;
                if (xpLogging) Plugin.Logger.LogInfo(System.DateTime.Now + ": Running Set Level for Ally");
                SetLevel(user_component.LocalCharacter._Entity, user, user_component.PlatformId);
            }
        }

        public static void LoseEXP(Entity playerEntity)
        {
            PlayerCharacter player = entityManager.GetComponentData<PlayerCharacter>(playerEntity);
            Entity userEntity = player.UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong SteamID = user.PlatformId;

            int EXPLost;
            Database.player_experience.TryGetValue(SteamID, out int exp);
            if (exp <= 0) EXPLost = 0;
            else
            {
                int variableEXP = convertLevelToXp(convertXpToLevel(exp) + 1) - convertLevelToXp(convertXpToLevel(exp));
                EXPLost = (int)(variableEXP * EXPLostOnDeath);
            }

            Database.player_experience[SteamID] = exp - EXPLost;

            SetLevel(playerEntity, userEntity, SteamID);
            Output.SendLore(userEntity, $"You've been defeated,<color=#fffffffe> {EXPLostOnDeath * 100}%</color> experience is lost.");
        }

        public static void BuffReceiver(Entity buffEntity)
        {
            PrefabGUID GUID = entityManager.GetComponentData<PrefabGUID>(buffEntity);
            if (GUID.Equals(Database.Buff.LevelUp_Buff)) {
                Entity Owner = entityManager.GetComponentData<EntityOwner>(buffEntity).Owner;
                if (entityManager.HasComponent<PlayerCharacter>(Owner))
                {
                    LifeTime lifetime = entityManager.GetComponentData<LifeTime>(buffEntity);
                    lifetime.Duration = 0.0001f;
                    entityManager.SetComponentData(buffEntity, lifetime);
                }
            }
        }

        public static void SetLevel(Entity entity, Entity user, ulong SteamID)
        {
            if (!Database.player_experience.ContainsKey(SteamID)) Database.player_experience[SteamID] = 0;
            if (!Database.player_abilityIncrease.ContainsKey(SteamID)) Database.player_abilityIncrease[SteamID] = 0;

            float level = convertXpToLevel(Database.player_experience[SteamID]);
            if (level < 0) return;
            if (level > MaxLevel){
                level = MaxLevel;
                Database.player_experience[SteamID] = convertLevelToXp(MaxLevel);
            }

            bool isLastLevel = Cache.player_level.TryGetValue(SteamID, out var level_);
            if (isLastLevel){
                if (level_ < level) 
                {
                    Cache.player_level[SteamID] = level;
                    Helper.ApplyBuff(user, entity, Database.Buff.LevelUp_Buff);

                    if (LevelRewardsOn)
                    {
                        //increases by level
                        for (var i = level_+1; i <= level; i++)
                        {
                            //default rewards for leveling up
                            Database.player_abilityIncrease[SteamID] += 1;
                            Database.player_level_stats[SteamID][UnitStatType.MaxHealth] += .5f;

                            Helper.ApplyBuff(user, entity, Database.Buff.Buff_VBlood_Perk_Moose);

                            //extra ability point rewards to spend for achieve certain level milestones
                            switch (i)
                            {
                                case 1:
                                    Database.player_abilityIncrease[SteamID] += 1;
                                    break;
                            }
                        }
                    }

                    if (Database.player_log_exp.TryGetValue(SteamID, out bool isLogging))
                    {
                        if (isLogging) 
                        {
                            var userData = entityManager.GetComponentData<User>(user);
                            Output.SendLore(user, $"<color=#ffdd00>Level up! You're now level</color><color=#fffffffe> {level}</color><color=#ffdd00ff>!</color>");
                        }
                    }
                    
                }
            }
            else
            {
                Cache.player_level[SteamID] = level;
            }
            Equipment eq_comp = entityManager.GetComponentData<Equipment>(entity);
            
            eq_comp.SpellLevel._Value = level;

            entityManager.SetComponentData(entity, eq_comp);
        }

        /// <summary>
        /// For use with the LevelUpRewards buffing system.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Owner"></param>
        /// <param name="SteamID"></param>
        public static void BuffReceiver(DynamicBuffer<ModifyUnitStatBuff_DOTS> Buffer, Entity Owner, ulong SteamID)
        {
            if (!LevelRewardsOn) return;
            float multiplier = 1;
            try
            {
                foreach (var gearType in Database.player_level_stats[SteamID])
                {
                    //we have to hack unequipped players and give them double bonus because the buffer array does not contain the buff, but they get an additional 
                    //buff of the same type when they are equipped! This will make them effectively the same effect, equipped or not.
                    //Maybe im just dumb, but I checked the array and tried that approach thinking i was double buffing due to logical error                    
                    if (WeaponMasterSystem.GetWeaponType(Owner) == WeaponType.None) multiplier = 2; 

                    Buffer.Add(new ModifyUnitStatBuff_DOTS()
                    {
                        StatType = gearType.Key,
                        Value = gearType.Value * multiplier,
                        ModificationType = ModificationType.AddToBase,
                        Id = ModificationId.NewId(0)
                    });
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Could not apply buff, I'm sad for you! {ex.ToString()} ");
            }
        }

        public static int convertXpToLevel(int xp)
        {
            // Level = 0.05 * sqrt(xp)
            int lvl = (int)Math.Floor(EXPConstant * Math.Sqrt(xp));
            if(lvl < 15 && easyLvl15){
                lvl = Math.Min(15, (int)Math.Sqrt(xp));
            }
            return lvl;
        }

        public static int convertLevelToXp(int level)
        {
            // XP = (Level / 0.05) ^ 2
            int xp = (int)Math.Pow(level / EXPConstant, EXPPower);
            if (level <= 15 && easyLvl15)
            {
                xp = level * level;
            }
            return xp;
        }

        public static int getXp(ulong SteamID)
        {
            if (Database.player_experience.TryGetValue(SteamID, out int exp)) return exp;
            return 0;
        }

        public static int getLevel(ulong SteamID)
        {
            return convertXpToLevel(getXp(SteamID));
        }

        public static int getLevelProgress(ulong SteamID)
        {
            int currentXP = getXp(SteamID);
            int currentLevelXP = convertLevelToXp(getLevel(SteamID));
            int nextLevelXP = convertLevelToXp(getLevel(SteamID) + 1);

            double neededXP = nextLevelXP - currentLevelXP;
            double earnedXP = nextLevelXP - currentXP;

            return 100 - (int)Math.Ceiling(earnedXP / neededXP * 100);
        }

        public static void SaveEXPData()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/player_experience.json", JsonSerializer.Serialize(Database.player_experience, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/player_log_exp.json", JsonSerializer.Serialize(Database.player_log_exp, Database.JSON_options));
            File.WriteAllText("BepInEx/config/RPGMods/Saves/player_abilitypoints.json", JsonSerializer.Serialize(Database.player_abilityIncrease, Database.JSON_options));
            File.WriteAllText($"BepInEx/config/RPGMods/Saves/player_level_stats.json", JsonSerializer.Serialize(Database.player_level_stats, Database.JSON_options));
            if (!Database.ErrorOnLoadingExperienceClasses) File.WriteAllText($"BepInEx/config/RPGMods/Saves/experience_class_stats.json", JsonSerializer.Serialize(Database.experience_class_stats, Database.JSON_options));
        }

        public static void LoadEXPData()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/player_experience.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/player_experience.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/player_experience.json");
            try
            {
                Database.player_experience = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("PlayerEXP DB Populated.");
            }
            catch
            {
                Database.player_experience = new Dictionary<ulong, int>();
                Plugin.Logger.LogWarning("PlayerEXP DB Created.");
            }

            //we have to know the difference here between a deserialization failure or initialization since if the file is there we don't 
            //want to overwrite it in case we typoed a unitstattype or some other typo in the experience class config file.
            var wasExperienceClassesCreated = false;
            if (!File.Exists("BepInEx/config/RPGMods/Saves/experience_class_stats.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/experience_class_stats.json");
                wasExperienceClassesCreated = true;
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/experience_class_stats.json");
            try
            {
                Database.experience_class_stats = JsonSerializer.Deserialize<Dictionary<string, Dictionary<UnitStatType, float>>>(json);
                Plugin.Logger.LogWarning("Experience class stats DB Populated.");
                Database.ErrorOnLoadingExperienceClasses = false;
            }
            catch (Exception ex)
            {
                initializeClassData();
                if (wasExperienceClassesCreated) Plugin.Logger.LogWarning("Experience class stats DB Created.");
                else
                {
                    Plugin.Logger.LogError($"Problem loading experience classes from file. {ex.Message}");
                    Database.ErrorOnLoadingExperienceClasses = true;
                }
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/player_abilitypoints.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/player_abilitypoints.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/player_abilitypoints.json");
            try
            {
                Database.player_abilityIncrease = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                Plugin.Logger.LogWarning("PlayerAbilities DB Populated.");
            }
            catch
            {
                Database.player_abilityIncrease = new Dictionary<ulong, int>();                
                Plugin.Logger.LogWarning("PlayerAbilities DB Created.");
            }

            if (!File.Exists($"BepInEx/config/RPGMods/Saves/player_level_stats.json"))
            {
                FileStream stream = File.Create($"BepInEx/config/RPGMods/Saves/player_level_stats.json");
                stream.Dispose();
            }
            json = File.ReadAllText($"BepInEx/config/RPGMods/Saves/player_level_stats.json");
            try
            {
                Database.player_level_stats = JsonSerializer.Deserialize<LazyDictionary<ulong, LazyDictionary<UnitStatType,float>>>(json);
                Plugin.Logger.LogWarning("Player level Stats DB Populated.");
            }
            catch
            {
                Database.player_level_stats = new LazyDictionary<ulong, LazyDictionary<UnitStatType, float>>();
                Plugin.Logger.LogWarning("Player level stats DB Created.");
            }

            if (!File.Exists("BepInEx/config/RPGMods/Saves/player_log_exp.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/Saves/player_log_exp.json");
                stream.Dispose();
            }
            json = File.ReadAllText("BepInEx/config/RPGMods/Saves/player_log_exp.json");
            try
            {
                Database.player_log_exp = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("PlayerEXP_Log_Switch DB Populated.");
            }
            catch
            {
                Database.player_log_exp = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("PlayerEXP_Log_Switch DB Created.");
            }
        }

        private static void initializeClassData()
        {
            Database.experience_class_stats = new Dictionary<string, Dictionary<UnitStatType, float>>();
            //maybe someday we'll have a default
        }
    }
}
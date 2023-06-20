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
using MS.Internal.Xml.XPath;

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
        public static bool xpLogging = false;
        public static void EXPMonitor(Entity killerEntity, Entity victimEntity)
        {
            if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Kill Found, Handling XP");
            
            //-- Check victim is not a summon
            if (entityManager.HasComponent<Minion>(victimEntity)) {
                if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Minion killed, exiting XP");
                return;
            }
            
            //-- Check victim has a level
            if (!entityManager.HasComponent<UnitLevel>(victimEntity)) {
                if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Has no level, exiting XP");
                return;
            }
            
            //-- Must be executed from main thread
            if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Fetching allies...");
            Helper.GetAllies(killerEntity, out var playerGroup);
            //-- ---------------------------------
            if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Allies Fetched, Total ally count of " + playerGroup.AllyCount);
            UpdateEXP(killerEntity, victimEntity, playerGroup);
        }

        public static void UpdateEXP(Entity killerEntity, Entity victimEntity, PlayerGroup playerGroup) {
            if (xpLogging) {
                Plugin.Logger.LogInfo($"{DateTime.Now}: Killer entity: {killerEntity}");
                Plugin.Logger.LogInfo($"{DateTime.Now}: Began XP Update, current ally count: {playerGroup.AllyCount}");
            }

            var unitLevel = entityManager.GetComponentData<UnitLevel>(victimEntity);

            bool isVBlood;
            if (entityManager.HasComponent<BloodConsumeSource>(victimEntity))
            {
                BloodConsumeSource bloodSource = entityManager.GetComponentData<BloodConsumeSource>(victimEntity);
                isVBlood = bloodSource.UnitBloodType.Equals(vBloodType);
            }
            else
            {
                isVBlood = false;
            }

            var killerLocation = entityManager.GetComponentData<LocalToWorld>(killerEntity);
            
            List<Entity> closeAllies = new();
            foreach (var ally in playerGroup.Allies) {
                if (xpLogging) Plugin.Logger.LogInfo($"{DateTime.Now}: Getting close allies");
                if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Iterating over allies, ally is " + ally.GetHashCode());
                if (Cache.PlayerLocations.TryGetValue(ally.Value, out var allyPos)) {
                    if (xpLogging) Plugin.Logger.LogInfo(   DateTime.Now + ": Got Ally Position");
                    var distance = math.distance(killerLocation.Position.xz, allyPos.Position.xz);
                    if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Distance is " + distance + ", Max Distance is " + GroupMaxDistance);
                    if (distance <= GroupMaxDistance) {
                        if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Adjusting XP Gain and adding ally to the close allies list");
                        closeAllies.Add(ally.Key);
                    }
                } else {
                    if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Could not get position, update the cache!");
                }
            }

            if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Running Assign EXP for all close allied players");
            var isGroup = closeAllies.Count > 1;
            foreach (var teammate in closeAllies) {
                if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Assigning EXP to " + teammate.GetHashCode());
                AssignExp(teammate, unitLevel.Level, isVBlood, isGroup);
            }
        }

        public static void AssignExp(Entity entity, int mobLevel, bool isVBlood, bool isGroup) {
            if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Getting User Component from entity");

            if (!entityManager.TryGetComponentData(entity, out PlayerCharacter pc)) {
                if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": Player Character Component unavailable, available components are: " + Plugin.Server.EntityManager.Debug.GetEntityInfo(entity));
                return;
            } 
            var user = pc.UserEntity;
            if (!entityManager.TryGetComponentData(user, out User userComponent)) {
                if (xpLogging) Plugin.Logger.LogInfo(DateTime.Now + ": User Component unavailable, available components from pc.UserEntity are: " + Plugin.Server.EntityManager.Debug.GetEntityInfo(user));
                // Can't really do anything at this point
                return;
            }

            var steamID = userComponent.PlatformId;
            int playerLevel = 0;
            if (Database.player_experience.TryGetValue(steamID, out int currentXp))
            {
                playerLevel = convertXpToLevel(currentXp);
                if (currentXp >= convertLevelToXp(MaxLevel)) return;
            }

            var xpGained = isVBlood ? (int)(mobLevel * VBloodMultiplier) : mobLevel;

            var levelDiff = mobLevel - playerLevel;
            
            if (levelDiff >= 0) xpGained = (int)(xpGained * (1 + levelDiff * 0.1) * EXPMultiplier);
            else{
                float xpMult = levelDiff * -1.0f;
                xpMult = xpMult / (xpMult + 10.0f);
                xpGained = (int)(xpGained * (1-xpMult));
            }

            if (isGroup) {
                xpGained = (int)(xpGained * GroupModifier);
            }
            
            Database.player_experience[steamID] = Math.Max(currentXp, 0) + xpGained;;

            if (Database.player_log_exp.TryGetValue(steamID, out bool isLogging))
            {
                if (isLogging) {
                    GetLevelAndProgress(currentXp, out int progress, out int earned, out int needed);
                    Output.SendLore(user, $"<color=#ffdd00>You gain {xpGained} XP by slaying a Lv.{mobLevel} enemy.</color> [ XP: <color=#fffffffe> {earned}</color>/<color=#fffffffe>{needed}</color> ]");
                }
            }
            
            SetLevel(userComponent.LocalCharacter._Entity, user, steamID);
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

            int currentXp = exp - EXPLost;
            Database.player_experience[SteamID] = currentXp;

            SetLevel(playerEntity, userEntity, SteamID);
            GetLevelAndProgress(currentXp, out int progress, out int earned, out int needed);
            Output.SendLore(userEntity, $"You've been defeated,<color=#fffffffe> {EXPLostOnDeath * 100}%</color> XP is lost. [ XP: <color=#fffffffe> {earned}</color>/<color=#fffffffe>{needed}</color> ]");
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
            Database.player_experience.TryAdd(SteamID, 0);
            Database.player_abilityIncrease.TryAdd(SteamID, 0);

            float level = convertXpToLevel(Database.player_experience[SteamID]);
            if (level < 0) return;
            if (level > MaxLevel){
                level = MaxLevel;
                Database.player_experience[SteamID] = convertLevelToXp(MaxLevel);
            }

            bool levelDataExists = Cache.player_level.TryGetValue(SteamID, out var storedLevel);
            if (levelDataExists){
                if (storedLevel < level) 
                {
                    Cache.player_level[SteamID] = level;
                    Helper.ApplyBuff(user, entity, Database.Buff.LevelUp_Buff);

                    if (LevelRewardsOn)
                    {
                        //increases by level
                        for (var i = storedLevel+1; i <= level; i++)
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

        public static void GetLevelAndProgress(int currentXp, out int progressPercent, out int earnedXp, out int neededXp) {
            var currentLevel = convertXpToLevel(currentXp);
            var currentLevelXp = convertLevelToXp(currentLevel);
            var nextLevelXp = convertLevelToXp(currentLevel + 1);

            neededXp = nextLevelXp - currentLevelXp;
            earnedXp = currentXp - currentLevelXp;
            
            progressPercent = (int)Math.Floor((double)earnedXp / neededXp * 100.0);
        }

        public static void SaveEXPData(string saveFolder)
        {
            File.WriteAllText(saveFolder + "player_experience.json", JsonSerializer.Serialize(Database.player_experience, Database.JSON_options));
            File.WriteAllText(saveFolder+"player_log_exp.json", JsonSerializer.Serialize(Database.player_log_exp, Database.JSON_options));
            File.WriteAllText(saveFolder+"player_abilitypoints.json", JsonSerializer.Serialize(Database.player_abilityIncrease, Database.JSON_options));
            File.WriteAllText(saveFolder+"player_level_stats.json", JsonSerializer.Serialize(Database.player_level_stats, Database.JSON_options));
            if (!Database.ErrorOnLoadingExperienceClasses) File.WriteAllText(saveFolder+"experience_class_stats.json", JsonSerializer.Serialize(Database.experience_class_stats, Database.JSON_options));
        }

        public static void LoadEXPData() {
            string specificName = "player_experience.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder,specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            string json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+ specificName);
            try{
                Database.player_experience = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                if (Database.player_experience == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.player_experience = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                }
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
            specificName = "experience_class_stats.json";
            if (!File.Exists(AutoSaveSystem.mainSaveFolder+"experience_class_stats.json")){
                FileStream stream = File.Create(AutoSaveSystem.mainSaveFolder+"experience_class_stats.json");
                wasExperienceClassesCreated = true;
                stream.Dispose();
            }
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+"experience_class_stats.json");
            try{
                Database.experience_class_stats = JsonSerializer.Deserialize<Dictionary<string, Dictionary<UnitStatType, float>>>(json);
                if (Database.experience_class_stats == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.experience_class_stats = JsonSerializer.Deserialize<Dictionary<string, Dictionary<UnitStatType, float>>>(json);
                }
                Plugin.Logger.LogWarning("Experience class stats DB Populated.");
                Database.ErrorOnLoadingExperienceClasses = false;
            }
            catch (Exception ex){
                initializeClassData();
                if (wasExperienceClassesCreated) Plugin.Logger.LogWarning("Experience class stats DB Created.");
                else{
                    Plugin.Logger.LogError($"Problem loading experience classes from file. {ex.Message}");
                    Database.ErrorOnLoadingExperienceClasses = true;
                }
            }

            specificName = "player_abilitypoints.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder,specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+ specificName);
            try{
                Database.player_abilityIncrease = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                if (Database.player_abilityIncrease == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.player_abilityIncrease = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json);
                }
                Plugin.Logger.LogWarning("PlayerAbilities DB Populated.");
            }
            catch{
                Database.player_abilityIncrease = new Dictionary<ulong, int>();                
                Plugin.Logger.LogWarning("PlayerAbilities DB Created.");
            }


            specificName = "player_level_stats.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder,specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+ specificName);
            try{
                Database.player_level_stats = JsonSerializer.Deserialize<LazyDictionary<ulong, LazyDictionary<UnitStatType,float>>>(json);
                if (Database.player_level_stats == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.player_level_stats = JsonSerializer.Deserialize<LazyDictionary<ulong, LazyDictionary<UnitStatType, float>>>(json);
                }
                Plugin.Logger.LogWarning("Player level Stats DB Populated.");
            }
            catch{
                Database.player_level_stats = new LazyDictionary<ulong, LazyDictionary<UnitStatType, float>>();
                Plugin.Logger.LogWarning("Player level stats DB Created.");
            }

            specificName = "player_log_exp.json";
            Helper.confirmFile(AutoSaveSystem.mainSaveFolder,specificName);
            Helper.confirmFile(AutoSaveSystem.backupSaveFolder,specificName);
            json = File.ReadAllText(AutoSaveSystem.mainSaveFolder+ specificName);
            try{
                Database.player_log_exp = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                if (Database.player_log_exp == null) {
                    json = File.ReadAllText(AutoSaveSystem.backupSaveFolder + specificName);
                    Database.player_log_exp = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                }
                Plugin.Logger.LogWarning("PlayerEXP_Log_Switch DB Populated.");
            }
            catch{
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
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using Unity.Entities;
using OpenRPG.Utils;
using System.Linq;
using BepInEx.Logging;
using Cache = OpenRPG.Utils.Cache;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Systems
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

        public static float pvpXPLossPercent = 0;
        public static float pveXPLossPercent = 10;
        
        public enum GroupLevelScheme {
            None = 0,
            Average = 1,
            Max = 2,
            EachPlayer = 3,
            Killer = 4,
        }

        public static GroupLevelScheme groupLevelScheme = GroupLevelScheme.Max;

        public static bool IsPlayerLoggingExperience(ulong steamId)
        {
            return Database.player_log_exp.GetValueOrDefault(steamId, false);
        }
        
        public static bool EntityProvidesExperience(Entity victim) {
            //-- Check victim is not a summon
            if (Plugin.Server.EntityManager.HasComponent<Minion>(victim)) {
                Plugin.Log(LogSystem.Death, LogLevel.Info, "Minion killed, ignoring");
                return false;
            }

            //-- Check victim has a level
            if (!Plugin.Server.EntityManager.HasComponent<UnitLevel>(victim)) {
                Plugin.Log(LogSystem.Death, LogLevel.Info, "Has no level, ignoring");
                return false;
            }

            return true;
        }

        public static void EXPMonitor(List<Alliance.ClosePlayer> closeAllies, Entity victimEntity, bool isVBlood)
        {
            var unitLevel = entityManager.GetComponentData<UnitLevel>(victimEntity);
            UpdateEXP(unitLevel.Level, isVBlood, closeAllies);
        }

        private static void UpdateEXP(int mobLevel, bool isVBlood, List<Alliance.ClosePlayer> closeAllies) {
            // Calculate the modified player level
            var modifiedPlayerLevel = 0;
            switch (groupLevelScheme) {
                case GroupLevelScheme.Average:
                    modifiedPlayerLevel += (int)closeAllies.Average(x => x.playerLevel);
                    break;
                case GroupLevelScheme.Max:
                    modifiedPlayerLevel = closeAllies.Max(x => x.playerLevel);
                    break;
                case GroupLevelScheme.Killer:
                    // If the killer is not found, use MaxLevel
                    modifiedPlayerLevel = closeAllies.Where(x => x.isTrigger).Select(x => x.playerLevel).FirstOrDefault(MaxLevel);
                    break;
                case GroupLevelScheme.EachPlayer:
                case GroupLevelScheme.None:
                    // Do nothing to modify the player level
                    break;
                default:
                    Plugin.Log(LogSystem.Xp, LogLevel.Warning, $"Group level scheme unknown");
                    break;
            }

            Plugin.Log(LogSystem.Xp, LogLevel.Info, "Running Assign EXP for all close allied players");
            var isGroup = closeAllies.Count > 1 && groupLevelScheme != GroupLevelScheme.None;
            foreach (var teammate in closeAllies) {
                Plugin.Log(LogSystem.Xp, LogLevel.Info, "Assigning EXP to " + teammate.steamID);
                switch (groupLevelScheme) {
                    case GroupLevelScheme.Average:
                    case GroupLevelScheme.Max:
                    case GroupLevelScheme.Killer:
                        // modifiedPlayerLevel already up to date
                        break;
                    case GroupLevelScheme.EachPlayer:
                        modifiedPlayerLevel = teammate.playerLevel;
                        break;
                    case GroupLevelScheme.None:
                        modifiedPlayerLevel = teammate.playerLevel;
                        break;
                }
                Plugin.Log(LogSystem.Xp, LogLevel.Info, $"IsKiller: {teammate.isTrigger} LVL: {teammate.playerLevel} M_LVL: {modifiedPlayerLevel} {groupLevelScheme}");
                AssignExp(teammate, mobLevel, modifiedPlayerLevel, isVBlood, isGroup);
            }
        }

        private static void AssignExp(Alliance.ClosePlayer player, int mobLevel, int modifiedPlayerLevel, bool isVBlood, bool isGroup) {
            if (player.currentXp >= convertLevelToXp(MaxLevel)) return;

            var xpGained = CalculateXp(modifiedPlayerLevel, mobLevel, isVBlood);

            if (isGroup) {
                xpGained = (int)(xpGained * GroupModifier);
            }

            var newXp = Math.Max(player.currentXp, 0) + xpGained;
            Database.player_experience[player.steamID] = newXp;

            if (IsPlayerLoggingExperience(player.steamID))
            {
                GetLevelAndProgress(newXp, out var progress, out var earned, out var needed);
                Output.SendLore(player.userEntity, $"<color=#ffdd00>You gain {xpGained} XP by slaying a Lv.{mobLevel} enemy.</color> [ XP: <color=#ffffff>{earned}</color>/<color=#ffffff>{needed}</color> ]");
            }
            
            SetLevel(player.userComponent.LocalCharacter._Entity, player.userEntity, player.steamID);
        }

        private static int CalculateXp(int playerLevel, int mobLevel, bool isVBlood) {
            var xpGained = isVBlood ? (int)(mobLevel * VBloodMultiplier) : mobLevel;

            var levelDiff = mobLevel - playerLevel;
            
            if (levelDiff >= 0) xpGained = (int)(xpGained * (1 + levelDiff * 0.1) * EXPMultiplier);
            else{
                float xpMult = levelDiff * -1.0f;
                xpMult /= (xpMult + 10.0f);
                xpGained = (int)(xpGained * (1-xpMult));
            }

            return xpGained;
        }
        
        public static void deathXPLoss(Entity playerEntity, Entity killerEntity) {
            var player = entityManager.GetComponentData<PlayerCharacter>(playerEntity);
            var userEntity = player.UserEntity;
            var user = entityManager.GetComponentData<User>(userEntity);
            var steamID = user.PlatformId;

            Database.player_experience.TryGetValue(steamID, out var exp);
            var pLvl = convertXpToLevel(exp);
            var killerLvl = pLvl;
            var pvpKill = entityManager.TryGetComponentData<PlayerCharacter>(killerEntity, out _);
            if (pvpKill)
            {
                var killerGear = entityManager.GetComponentData<Equipment>(killerEntity);
                killerLvl = (int)(killerGear.ArmorLevel.Value + killerGear.WeaponLevel.Value + killerGear.SpellLevel.Value);
                Plugin.Log(LogSystem.Xp, LogLevel.Info, $"Killer: [{killerGear.ArmorLevel.Value:F1},{killerGear.WeaponLevel.Value:F1},{killerGear.SpellLevel.Value:F1}]");
            }
            else if (entityManager.TryGetComponentData<UnitLevel>(killerEntity, out var killerUnitLevel)) killerLvl = killerUnitLevel.Level;
            else {
                Plugin.Log(LogSystem.Xp, LogLevel.Info, $"Killer has no level to be found. Entity: {killerEntity}, Prefab: {Helper.GetPrefabGUID(killerEntity)}, Name {Helper.GetPrefabName(killerEntity)}");
            }
            
            var lvlDiff = pLvl - killerLvl;
            Plugin.Log(LogSystem.Xp, LogLevel.Info, $"Level Difference: {lvlDiff} (Victim: {pLvl}, Killer:{killerLvl})");
            
            var newPvEXp = Math.Max(exp * (1 - pveXPLossPercent), convertLevelToXp(convertXpToLevel(exp)));
            var newPvPXp = Math.Max(exp * (1 - pvpXPLossPercent * lvlDiff), convertLevelToXp(convertXpToLevel(exp)));
            var xpLost = pvpKill ? newPvPXp : newPvEXp;

            var currentXp = Math.Max(exp - (int)xpLost, 0);
            Plugin.Log(LogSystem.Xp, LogLevel.Info, "subtracting that from our " + exp + " we get " + currentXp);
            Database.player_experience[steamID] = currentXp;

            SetLevel(playerEntity, userEntity, steamID);
            GetLevelAndProgress(currentXp, out _, out var earned, out var needed);
            Output.SendLore(userEntity, $"You've been defeated, <color=#ffffff>{xpLost}</color> XP is lost. [ XP: <color=#ffffff>{earned}</color>/<color=#ffffff>{needed}</color> ]");
        }

        public static void BuffReceiver(Entity buffEntity)
        {
            PrefabGUID GUID = entityManager.GetComponentData<PrefabGUID>(buffEntity);
            if (GUID.Equals(Helper.LevelUp_Buff)) {
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
                    Helper.ApplyBuff(user, entity, Helper.LevelUp_Buff);

                    if (LevelRewardsOn)
                    {
                        //increases by level
                        for (var i = storedLevel+1; i <= level; i++)
                        {
                            //default rewards for leveling up
                            Database.player_abilityIncrease[SteamID] += 1;
                            Database.player_level_stats[SteamID][UnitStatType.MaxHealth] += .5f;

                            Helper.ApplyBuff(user, entity, Helper.AppliedBuff);

                            //extra ability point rewards to spend for achieve certain level milestones
                            switch (i)
                            {
                                case 1:
                                    Database.player_abilityIncrease[SteamID] += 1;
                                    break;
                            }
                        }
                    }

                    if (IsPlayerLoggingExperience(SteamID))
                    {
                        Output.SendLore(user, $"<color=#ffdd00>Level up! You're now level</color> <color=#ffffff>{level}</color><color=#ffdd00ff>!</color>");
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
                    if (WeaponMasterySystem.GetWeaponType(Owner) == WeaponType.None) multiplier = 2; 

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
                Plugin.Log(LogSystem.Xp, LogLevel.Error, $"Could not apply XP buff!\n{ex} ");
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
            return Database.player_experience.GetValueOrDefault(SteamID, 0);
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

        public static Dictionary<string, Dictionary<UnitStatType, float>> DefaultExperienceClassStats()
        {
            var classes = new Dictionary<string, Dictionary<UnitStatType, float>>();
            classes["health"] = new Dictionary<UnitStatType, float>() { { UnitStatType.MaxHealth, 0.5f } };
            classes["ppower"] = new Dictionary<UnitStatType, float>() { { UnitStatType.PhysicalPower, 0.75f } };
            classes["spower"] = new Dictionary<UnitStatType, float>() { { UnitStatType.SpellPower, 0.75f } };
            classes["presist"] = new Dictionary<UnitStatType, float>() { { UnitStatType.PhysicalResistance, 0.05f } };
            classes["sresist"] = new Dictionary<UnitStatType, float>() { { UnitStatType.SpellResistance, 0.05f } };
            classes["beasthunter"] = new Dictionary<UnitStatType, float>()
                { { UnitStatType.DamageVsBeasts, 0.04f }, { UnitStatType.ResistVsBeasts, 4f } };
            classes["undeadhunter"] = new Dictionary<UnitStatType, float>()
                { { UnitStatType.DamageVsUndeads, 0.02f }, { UnitStatType.ResistVsUndeads, 2 } };
            classes["manhunter"] = new Dictionary<UnitStatType, float>() { { UnitStatType.DamageVsHumans, 0.02f}, { UnitStatType.ResistVsHumans, 2 } };
            classes["demonhunter"] = new Dictionary<UnitStatType, float>() { { UnitStatType.DamageVsDemons, 0.02f}, { UnitStatType.ResistVsDemons, 2 } };
            classes["farmer"] = new Dictionary<UnitStatType, float>()
            {
                { UnitStatType.ResourceYield, 0.1f }, { UnitStatType.PhysicalPower, -1f },
                { UnitStatType.SpellPower, -0.5f }
            };
            return classes;
        }
    }
}
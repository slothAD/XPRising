using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using Unity.Entities;
using OpenRPG.Utils;
using System.Linq;
using BepInEx.Logging;
using OpenRPG.Models;
using OpenRPG.Utils.Prefabs;
using Cache = OpenRPG.Utils.Cache;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Systems
{
    public class ExperienceSystem
    {
        private static EntityManager _entityManager = Plugin.Server.EntityManager;

        public static float ExpMultiplier = 1;
        public static float VBloodMultiplier = 15;
        public static float ExpConstant = 0.1f;
        public static int ExpPower = 2;
        public static int MaxLevel = 80;
        public static float GroupMaxDistance = 50;
        public static bool ShouldAllowGearLevel = true;
        public static bool LevelRewardsOn = true;
        public static bool EasyLvl15 = true;

        public static float PvpXpLossPercent = 0;
        public static float PveXpLossPercent = 10;

        private static HashSet<Units> _noExpUnits = new HashSet<Units>(
            FactionUnits.farmNonHostile.Select(u => u.type).Union(FactionUnits.farmFood.Select(u => u.type)));
        
        // Encourage group play by buffing XP for groups
        private const double GroupXpBuff = 1.4;
        
        public enum GroupLevelScheme {
            None = 0,
            Average = 1,
            Max = 2,
            EachPlayer = 3,
            Killer = 4,
        }

        public static GroupLevelScheme CurrentGroupLevelScheme = GroupLevelScheme.Max;

        // We can add various mobs/groups/factions here to reduce or increase XP gain
        private static float ExpValueMultiplier(Entity entity, bool isVBlood)
        {
            if (isVBlood) return VBloodMultiplier;
            var unit = Helper.ConvertGuidToUnit(Helper.GetPrefabGUID(entity));
            if (_noExpUnits.Contains(unit)) return 0;
            
            return 1;
        }
        
        public static bool IsPlayerLoggingExperience(ulong steamId)
        {
            return Database.playerLogConfig[steamId].LoggingExp;
        }

        public static void ExpMonitor(List<Alliance.ClosePlayer> closeAllies, Entity victimEntity, bool isVBlood)
        {
            var unitLevel = _entityManager.GetComponentData<UnitLevel>(victimEntity);
            var multiplier = ExpValueMultiplier(victimEntity, isVBlood);
            UpdateExp(unitLevel.Level, multiplier, closeAllies);
        }

        private static void UpdateExp(int mobLevel, float multiplier, List<Alliance.ClosePlayer> closeAllies) {
            // Calculate the modified player level
            var modifiedPlayerLevel = 0;
            switch (CurrentGroupLevelScheme) {
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
            foreach (var teammate in closeAllies) {
                Plugin.Log(LogSystem.Xp, LogLevel.Info, "Assigning EXP to " + teammate.steamID);
                switch (CurrentGroupLevelScheme) {
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
                Plugin.Log(LogSystem.Xp, LogLevel.Info, $"IsKiller: {teammate.isTrigger} LVL: {teammate.playerLevel} M_LVL: {modifiedPlayerLevel}");
                AssignExp(teammate, mobLevel, modifiedPlayerLevel, multiplier, closeAllies.Count);
            }
        }

        private static void AssignExp(Alliance.ClosePlayer player, int mobLevel, int modifiedPlayerLevel, float multiplier, int groupSize) {
            if (player.currentXp >= ConvertLevelToXp(MaxLevel)) return;

            var xpGained = CalculateXp(modifiedPlayerLevel, mobLevel, multiplier);

            if (groupSize > 1)
            {
                xpGained = (int)(xpGained * GroupXpBuff / groupSize);
            }

            var newXp = Math.Max(player.currentXp, 0) + xpGained;
            Database.player_experience[player.steamID] = newXp;

            if (IsPlayerLoggingExperience(player.steamID))
            {
                GetLevelAndProgress(newXp, out _, out var earned, out var needed);
                Output.SendLore(player.userEntity, $"<color=#ffdd00>You gain {xpGained} XP by slaying a Lv.{mobLevel} enemy.</color> [ XP: <color=#ffffff>{earned}</color>/<color=#ffffff>{needed}</color> ]");
            }
            
            SetLevel(player.userComponent.LocalCharacter._Entity, player.userEntity, player.steamID);
        }

        private static int CalculateXp(int playerLevel, int mobLevel, float multiplier) {
            var xpGained = mobLevel * multiplier;

            var levelDiff = mobLevel - playerLevel;
            
            if (levelDiff >= 0) xpGained = xpGained * (1 + levelDiff * 0.1f);
            else{
                levelDiff = Math.Abs(levelDiff);
                xpGained = xpGained * (1-levelDiff/(levelDiff + 10.0f));
            }

            return (int)(xpGained * ExpMultiplier);
        }
        
        public static void DeathXpLoss(Entity playerEntity, Entity killerEntity) {
            var player = _entityManager.GetComponentData<PlayerCharacter>(playerEntity);
            var userEntity = player.UserEntity;
            var user = _entityManager.GetComponentData<User>(userEntity);
            var steamID = user.PlatformId;

            Database.player_experience.TryGetValue(steamID, out var exp);
            var pLvl = ConvertXpToLevel(exp);
            var killerLvl = pLvl;
            var pvpKill = _entityManager.TryGetComponentData<PlayerCharacter>(killerEntity, out _);
            if (pvpKill)
            {
                var killerGear = _entityManager.GetComponentData<Equipment>(killerEntity);
                killerLvl = (int)(killerGear.ArmorLevel.Value + killerGear.WeaponLevel.Value + killerGear.SpellLevel.Value);
                Plugin.Log(LogSystem.Xp, LogLevel.Info, $"Killer: [{killerGear.ArmorLevel.Value:F1},{killerGear.WeaponLevel.Value:F1},{killerGear.SpellLevel.Value:F1}]");
            }
            else if (_entityManager.TryGetComponentData<UnitLevel>(killerEntity, out var killerUnitLevel)) killerLvl = killerUnitLevel.Level;
            else {
                Plugin.Log(LogSystem.Xp, LogLevel.Info, $"Killer has no level to be found. Entity: {killerEntity}, Prefab: {Helper.GetPrefabGUID(killerEntity)}, Name {Helper.GetPrefabName(killerEntity)}");
            }
            
            var lvlDiff = pLvl - killerLvl;
            Plugin.Log(LogSystem.Xp, LogLevel.Info, $"Level Difference: {lvlDiff} (Victim: {pLvl}, Killer:{killerLvl})");
            
            var newPvEXp = Math.Max(exp * (1 - PveXpLossPercent), ConvertLevelToXp(ConvertXpToLevel(exp)));
            var newPvPXp = Math.Max(exp * (1 - PvpXpLossPercent * lvlDiff), ConvertLevelToXp(ConvertXpToLevel(exp)));
            var xpLost = pvpKill ? newPvPXp : newPvEXp;

            var currentXp = Math.Max(exp - (int)xpLost, 0);
            Plugin.Log(LogSystem.Xp, LogLevel.Info, "subtracting that from our " + exp + " we get " + currentXp);
            Database.player_experience[steamID] = currentXp;

            SetLevel(playerEntity, userEntity, steamID);
            GetLevelAndProgress(currentXp, out _, out var earned, out var needed);
            Output.SendLore(userEntity, $"You've been defeated, <color=#ffffff>{xpLost}</color> XP is lost. [ XP: <color=#ffffff>{earned}</color>/<color=#ffffff>{needed}</color> ]");
        }

        public static void SetLevel(Entity entity, Entity user, ulong steamID)
        {
            Database.player_experience.TryAdd(steamID, 0);
            Database.player_abilityIncrease.TryAdd(steamID, 0);

            float level = ConvertXpToLevel(Database.player_experience[steamID]);
            if (level < 0) return;
            if (level > MaxLevel){
                level = MaxLevel;
                Database.player_experience[steamID] = ConvertLevelToXp(MaxLevel);
            }

            bool levelDataExists = Cache.player_level.TryGetValue(steamID, out var storedLevel);
            if (levelDataExists){
                if (storedLevel < level) 
                {
                    Cache.player_level[steamID] = level;
                    Helper.ApplyBuff(user, entity, Helper.LevelUp_Buff);

                    if (LevelRewardsOn)
                    {
                        //increases by level
                        for (var i = storedLevel+1; i <= level; i++)
                        {
                            //default rewards for leveling up
                            Database.player_abilityIncrease[steamID] += 1;
                            Database.player_level_stats[steamID][UnitStatType.MaxHealth] += .5f;

                            Helper.ApplyBuff(user, entity, Helper.AppliedBuff);

                            //extra ability point rewards to spend for achieve certain level milestones
                            switch (i)
                            {
                                case 1:
                                    Database.player_abilityIncrease[steamID] += 1;
                                    break;
                            }
                        }
                    }

                    if (IsPlayerLoggingExperience(steamID))
                    {
                        Output.SendLore(user, $"<color=#ffdd00>Level up! You're now level</color> <color=#ffffff>{level}</color><color=#ffdd00ff>!</color>");
                    }
                    
                }
            }
            else
            {
                Cache.player_level[steamID] = level;
            }
            Equipment equipment = _entityManager.GetComponentData<Equipment>(entity);
            
            equipment.SpellLevel._Value = level;

            _entityManager.SetComponentData(entity, equipment);
        }

        /// <summary>
        /// For use with the LevelUpRewards buffing system.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="owner"></param>
        /// <param name="steamID"></param>
        public static void BuffReceiver(DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer, Entity owner, ulong steamID)
        {
            if (!LevelRewardsOn) return;
            float multiplier = 1;
            try
            {
                foreach (var gearType in Database.player_level_stats[steamID])
                {
                    //we have to hack unequipped players and give them double bonus because the buffer array does not contain the buff, but they get an additional 
                    //buff of the same type when they are equipped! This will make them effectively the same effect, equipped or not.
                    //Maybe im just dumb, but I checked the array and tried that approach thinking i was double buffing due to logical error                    
                    if (WeaponMasterySystem.GetWeaponType(owner) == WeaponType.None) multiplier = 2; 

                    buffer.Add(new ModifyUnitStatBuff_DOTS()
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

        public static int ConvertXpToLevel(int xp)
        {
            // Level = 0.05 * sqrt(xp)
            int lvl = (int)Math.Floor(ExpConstant * Math.Sqrt(xp));
            if(lvl < 15 && EasyLvl15){
                lvl = Math.Min(15, (int)Math.Sqrt(xp));
            }
            return lvl;
        }

        public static int ConvertLevelToXp(int level)
        {
            // XP = (Level / 0.05) ^ 2
            int xp = (int)Math.Pow(level / ExpConstant, ExpPower);
            if (level <= 15 && EasyLvl15)
            {
                xp = level * level;
            }
            return xp;
        }

        public static int GetXp(ulong steamID)
        {
            return Database.player_experience.GetValueOrDefault(steamID, 0);
        }

        public static int GetLevel(ulong steamID)
        {
            return ConvertXpToLevel(GetXp(steamID));
        }

        public static void GetLevelAndProgress(int currentXp, out int progressPercent, out int earnedXp, out int neededXp) {
            var currentLevel = ConvertXpToLevel(currentXp);
            var currentLevelXp = ConvertLevelToXp(currentLevel);
            var nextLevelXp = ConvertLevelToXp(currentLevel + 1);

            neededXp = nextLevelXp - currentLevelXp;
            earnedXp = currentXp - currentLevelXp;
            
            progressPercent = (int)Math.Floor((double)earnedXp / neededXp * 100.0);
        }

        public static LazyDictionary<string, LazyDictionary<UnitStatType, float>> DefaultExperienceClassStats()
        {
            var classes = new LazyDictionary<string, LazyDictionary<UnitStatType, float>>();
            classes["health"] = new LazyDictionary<UnitStatType, float>() { { UnitStatType.MaxHealth, 0.5f } };
            classes["ppower"] = new LazyDictionary<UnitStatType, float>() { { UnitStatType.PhysicalPower, 0.75f } };
            classes["spower"] = new LazyDictionary<UnitStatType, float>() { { UnitStatType.SpellPower, 0.75f } };
            classes["presist"] = new LazyDictionary<UnitStatType, float>() { { UnitStatType.PhysicalResistance, 0.05f } };
            classes["sresist"] = new LazyDictionary<UnitStatType, float>() { { UnitStatType.SpellResistance, 0.05f } };
            classes["beasthunter"] = new LazyDictionary<UnitStatType, float>()
                { { UnitStatType.DamageVsBeasts, 0.04f }, { UnitStatType.ResistVsBeasts, 4f } };
            classes["undeadhunter"] = new LazyDictionary<UnitStatType, float>()
                { { UnitStatType.DamageVsUndeads, 0.02f }, { UnitStatType.ResistVsUndeads, 2 } };
            classes["manhunter"] = new LazyDictionary<UnitStatType, float>() { { UnitStatType.DamageVsHumans, 0.02f}, { UnitStatType.ResistVsHumans, 2 } };
            classes["demonhunter"] = new LazyDictionary<UnitStatType, float>() { { UnitStatType.DamageVsDemons, 0.02f}, { UnitStatType.ResistVsDemons, 2 } };
            classes["farmer"] = new LazyDictionary<UnitStatType, float>()
            {
                { UnitStatType.ResourceYield, 0.1f }, { UnitStatType.PhysicalPower, -1f },
                { UnitStatType.SpellPower, -0.5f }
            };
            return classes;
        }
    }
}
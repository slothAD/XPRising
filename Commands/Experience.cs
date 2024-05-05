using System;
using System.Linq;
using BepInEx.Logging;
using OpenRPG.Models;
using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Unity.Entities;
using VampireCommandFramework;
using LogSystem = OpenRPG.Plugin.LogSystem;

namespace OpenRPG.Commands
{

    [CommandGroup("experience","xp")]
    public static class Experience{
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        [Command("get", "g", "", "Display your current xp", adminOnly: false)]
        public static void getXP(ChatCommandContext ctx) {
            if (!ExperienceSystem.isEXPActive)
            {
                ctx.Reply("Experience system is not enabled.");
                return;
            }
            var user = ctx.Event.User;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            int userXP = ExperienceSystem.getXp(SteamID);
            ExperienceSystem.GetLevelAndProgress(userXP, out int progress, out int earnedXp, out int neededXp);
            int userLevel = ExperienceSystem.convertXpToLevel(userXP);
            string response = "-- <color=#ffffff>" + CharName + "</color> --\n";
            response += $"Level:<color=#ffffff> {userLevel}</color> (<color=#ffffff>{progress}%</color>) ";
            response += $" [ XP:<color=#ffffff> {earnedXp}</color>/<color=#ffffff>{neededXp}</color> ]";
            if (ExperienceSystem.LevelRewardsOn) response += $" You have {(Database.player_abilityIncrease.ContainsKey(SteamID) ? Database.player_abilityIncrease[SteamID] : 0)} ability points to spend.";
            ctx.Reply(response);
        }


        [Command("set", "s", "<playerName> <XP>", "Sets the specified player's current xp to the given value", adminOnly: true)]
        public static void setXP(ChatCommandContext ctx, string name, int xp){
            if (!ExperienceSystem.isEXPActive){
                ctx.Reply("Experience system is not enabled.");
                return;
            }
            ulong SteamID;

            if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity)){
                SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
            }
            else
            {
                ctx.Reply($"Could not find specified player \"{name}\".");
                return;
            }
            Database.player_experience[SteamID] = xp;
            ExperienceSystem.SetLevel(targetEntity, targetUserEntity, SteamID);
            ctx.Reply($"Player \"{name}\" Experience is now set to be<color=#ffffff> {ExperienceSystem.getXp(SteamID)}</color>");
        }

        [Command("log", "l", "", "Toggles logging of xp gain.", adminOnly: false)]
        public static void logExperience(ChatCommandContext ctx){
            if (!ExperienceSystem.isEXPActive){
                ctx.Reply("Experience system is not enabled.");
                return;
            }
            ulong SteamID;
            var UserEntity = ctx.Event.SenderUserEntity;
            SteamID = entityManager.GetComponentData<User>(UserEntity).PlatformId;
            var currentValue = ExperienceSystem.IsPlayerLoggingExperience(SteamID);
            Database.player_log_exp.Remove(SteamID);
            if (currentValue)
            {
                Database.player_log_exp.Add(SteamID, false);
                ctx.Reply($"Experience gain is no longer being logged.");
            }
            else
            {
                Database.player_log_exp.Add(SteamID, true);
                ctx.Reply("Experience gain is now being logged.");
            }
        }

        [Command("ability", "a", "<AbilityName> <amount>", "Spend given points on given ability", adminOnly: false)]
        public static void addClassAbility(ChatCommandContext ctx, string name, int amount){
            if (!ExperienceSystem.isEXPActive){
                ctx.Reply("Experience system is not enabled.");
                return;
            }
            if (!ExperienceSystem.LevelRewardsOn){
                ctx.Reply("Experience Class system is not enabled.");
                return;
            }
            
            ulong SteamID = ctx.Event.User.PlatformId;
            var UserEntity = ctx.Event.SenderUserEntity;
            var PlayerCharacter = ctx.Event.SenderCharacterEntity;
            
            try{
                int spendPoints = amount;
                string abilityName = name;

                if (!Database.player_abilityIncrease.ContainsKey(SteamID)) Database.player_abilityIncrease[SteamID] = 0;

                if (Database.player_abilityIncrease[SteamID] < spendPoints){
                    ctx.Reply("Not enough points!");
                    return;
                }

                if (Database.experience_class_stats.ContainsKey(abilityName.ToLower())){
                    foreach (var buff in Database.experience_class_stats[abilityName.ToLower()]){
                        Database.player_level_stats[SteamID][buff.Key] += buff.Value * spendPoints;
                    }

                    Database.player_abilityIncrease[SteamID] -= spendPoints;
                    Helper.ApplyBuff(UserEntity, PlayerCharacter, Helper.AppliedBuff);
                    ctx.Reply($"Spent {spendPoints}. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                    foreach (var buff in Database.player_level_stats[SteamID]){
                        ctx.Reply($"{buff.Key} : {buff.Value}");
                    }
                }
                else {
                    ctx.Reply("Type \".xp ability show\" to see current buffs.");
                    ctx.Reply($"Type .xp ability <ability> to spend ability points. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                    ctx.Reply("You can spend ability points on:");
                    ctx.Reply(string.Join(", ", Database.experience_class_stats.Keys.ToList()));
                }

            }
            catch (Exception ex){
                Plugin.Log(LogSystem.Xp, LogLevel.Error, $"Could not spend point! {ex}");
                ctx.Reply($"Could not spend point! {ex.Message}");
            }
        }
        
        [Command("ability show", "as", "", "Display the buffs provided by the XP class system", adminOnly: false)]
        public static void showClassAbility(ChatCommandContext ctx){
            if (!ExperienceSystem.isEXPActive){
                ctx.Reply("Experience system is not enabled.");
                return;
            }
            if (!ExperienceSystem.LevelRewardsOn){
                ctx.Reply("Experience Class system is not enabled.");
                return;
            }
            
            ulong SteamID;
            var UserEntity = ctx.Event.SenderUserEntity;
            var PlayerCharacter = ctx.Event.SenderCharacterEntity;
            SteamID = entityManager.GetComponentData<User>(UserEntity).PlatformId;
            
            foreach (var buff in Database.player_level_stats[SteamID]){
                ctx.Reply($"{buff.Key} : {buff.Value}");
            }
        }
        
        [Command("ability reset", "ar", "", "Reset your spent ability points", adminOnly: false)]
        public static void resetClassAbility(ChatCommandContext ctx){
            if (!ExperienceSystem.isEXPActive){
                ctx.Reply("Experience system is not enabled.");
                return;
            }
            if (!ExperienceSystem.LevelRewardsOn){
                ctx.Reply("Experience Class system is not enabled.");
                return;
            }
            
            ulong SteamID;
            var UserEntity = ctx.Event.SenderUserEntity;
            var PlayerCharacter = ctx.Event.SenderCharacterEntity;
            SteamID = entityManager.GetComponentData<User>(UserEntity).PlatformId;
            
            Database.player_level_stats[SteamID] = new LazyDictionary<ProjectM.UnitStatType, float>();
            Database.player_abilityIncrease[SteamID] = 0;
            Cache.player_level[SteamID] = 0;
            ExperienceSystem.SetLevel(PlayerCharacter, UserEntity, SteamID);
            ctx.Reply("Ability level up points reset.");
        }
    }
}

using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Entities;
using VampireCommandFramework;

namespace RPGMods.Commands
{

    [CommandGroup("experience","xp")]
    public static class Experience{
        private static EntityManager entityManager = Plugin.Server.EntityManager;

        [Command("get", "g", "", "Display your current bloodline progression", adminOnly: false)]
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
            string response = "-- <color=#fffffffe>" + CharName + "</color> --\n";
            response += $"Level:<color=#fffffffe> {userLevel}</color> (<color=#fffffffe>{progress}%</color>) ";
            response += $" [ XP:<color=#fffffffe> {earnedXp}</color>/<color=#fffffffe>{neededXp}</color> ]";
            if (ExperienceSystem.LevelRewardsOn) response += $" You have {(Database.player_abilityIncrease.ContainsKey(SteamID) ? Database.player_abilityIncrease[SteamID] : 0)} ability points to spend.";
            ctx.Reply(response);
        }


        [Command("set", "s", "[playerName, XP]", "Sets the specified players current xp to a specific value", adminOnly: true)]
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
            ctx.Reply($"Player \"{name}\" Experience is now set to be<color=#fffffffe> {ExperienceSystem.getXp(SteamID)}</color>");
        }

        [Command("log", "l", "<On, Off>", "Turns on or off logging of xp gain.", adminOnly: false)]
        public static void logExperience(ChatCommandContext ctx, string flag){
            if (!ExperienceSystem.isEXPActive){
                ctx.Reply("Experience system is not enabled.");
                return;
            }
            ulong SteamID;
            var UserEntity = ctx.Event.SenderUserEntity;
            SteamID = entityManager.GetComponentData<User>(UserEntity).PlatformId;
            if (flag.ToLower().Equals("on"))
            {
                Database.player_log_exp.Remove(SteamID);
                Database.player_log_exp.Add(SteamID, true);
                ctx.Reply("Experience gain is now being logged.");
                return;
            }
            else if (flag.ToLower().Equals("off"))
            {
                Database.player_log_exp.Remove(SteamID);
                Database.player_log_exp.Add(SteamID, false);
                ctx.Reply($"Experience gain is no longer being logged.");
                return;
            }
        }

        [Command("ability", "a", "[AbilityName, amount]", "spend amount ability points in AbilityName", adminOnly: false)]
        public static void classAbility(ChatCommandContext ctx, string name, int amount){
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
            
            //set arbitrary to 50;
            bool isAllowed = Database.user_permission[SteamID] >= 50;
            
            try{
                int spendPoints = amount;
                string abilityName = name;

                if (!Database.player_abilityIncrease.ContainsKey(SteamID)) Database.player_abilityIncrease[SteamID] = 0;

                if (Database.player_abilityIncrease[SteamID] < spendPoints && name.ToLower() != "show" && name.ToLower() != "reset"){
                    ctx.Reply("Not enough points!");
                    return;
                }

                if (Database.experience_class_stats.ContainsKey(abilityName.ToLower())){
                    foreach (var buff in Database.experience_class_stats[abilityName.ToLower()]){
                        Database.player_level_stats[SteamID][buff.Key] += buff.Value * spendPoints;
                    }

                    Database.player_abilityIncrease[SteamID] -= spendPoints;
                    Helper.ApplyBuff(UserEntity, PlayerCharacter, Helper.appliedBuff);
                    ctx.Reply($"Spent {spendPoints}. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                    foreach (var buff in Database.player_level_stats[SteamID]){
                        ctx.Reply($"{buff.Key} : {buff.Value}");
                    }
                }
                else switch (abilityName.ToLower()){
                    case "show":
                        foreach (var buff in Database.player_level_stats[SteamID]){
                            ctx.Reply($"{buff.Key} : {buff.Value}");
                        }
                        break;
                    case "reset":
                        if (!isAllowed) return;
                        Database.player_level_stats[SteamID] = new LazyDictionary<ProjectM.UnitStatType, float>();
                        Database.player_abilityIncrease[SteamID] = 0;
                        Cache.player_level[SteamID] = 0;
                        ExperienceSystem.SetLevel(PlayerCharacter, UserEntity, SteamID);
                        ctx.Reply("Ability level up points reset.");
                        break;
                    default:
                        ctx.Reply("Type \".xp ability show\" to see current buffs.");
                        ctx.Reply($"Type .xp ability <ability> to spend ability points. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                        ctx.Reply("You can spend ability points on:");
                        ctx.Reply("health, spower, ppower, presist, sresist, beasthunter, demonhunter, manhunter, undeadhunter, farmer");
                        break;
                }

            }
            catch (System.Exception ex){
                Plugin.Logger.LogError($"Could not spend point! {ex.ToString()}");
                ctx.Reply($"Could not spend point! {ex.Message}");
            }
        }

    }
}

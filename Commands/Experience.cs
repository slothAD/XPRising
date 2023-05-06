using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Entities;

namespace RPGMods.Commands
{
    [Command("experience, exp, xp", Usage = "experience [<log> <on>|<off>] [<ability> \"ability\"", Description = "Shows your currect experience and progression to next level, toggle the exp gain notification, or spend earned ability points.")]
    public static class Experience
    {
        private static EntityManager entityManager = Plugin.Server.EntityManager;
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            var PlayerCharacter = ctx.Event.SenderCharacterEntity;
            var UserEntity = ctx.Event.SenderUserEntity;

            if (!ExperienceSystem.isEXPActive)
            {
                Output.CustomErrorMessage(ctx, "Experience system is not enabled.");
                return;
            }

            if (ctx.Args.Length >= 2 )
            {
                bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "experience_args");
                if (ctx.Args[0].Equals("set") && isAllowed && int.TryParse(ctx.Args[1], out int value))
                {
                    if (ctx.Args.Length == 3)
                    {
                        string name = ctx.Args[2];
                        if(Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                        {
                            CharName = name;
                            SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                            PlayerCharacter = targetEntity;
                            UserEntity = targetUserEntity;
                        }
                        else
                        {
                            Output.CustomErrorMessage(ctx, $"Could not find specified player \"{name}\".");
                            return;
                        }
                    }
                    Database.player_experience[SteamID] = value;
                    ExperienceSystem.SetLevel(PlayerCharacter, UserEntity, SteamID);
                    Output.SendSystemMessage(ctx, $"Player \"{CharName}\" Experience is now set to be<color=#fffffffe> {ExperienceSystem.getXp(SteamID)}</color>");
                }
                else if (ctx.Args[0].ToLower().Equals("log"))
                {
                    if (ctx.Args[1].ToLower().Equals("on"))
                    {
                        Database.player_log_exp[SteamID] = true;
                        Output.SendSystemMessage(ctx, $"Experience gain is now logged.");
                        return;
                    }
                    else if (ctx.Args[1].ToLower().Equals("off"))
                    {
                        Database.player_log_exp[SteamID] = false;
                        Output.SendSystemMessage(ctx, $"Experience gain is no longer being logged.");
                        return;
                    }
                }
                else if (ctx.Args[0].ToLower().Equals("ability") && ExperienceSystem.LevelRewardsOn)
                {
                    try
                    {
                        int spendPoints = 1;
                        string abilityName = "help";
                        
                        if (ctx.Args.Length >= 2) abilityName = ctx.Args[1];
                        if (ctx.Args.Length > 2 && !int.TryParse(ctx.Args[2], out spendPoints)) spendPoints = 1;


                        if (!Database.player_abilityIncrease.ContainsKey(SteamID)) Database.player_abilityIncrease[SteamID] = 0;

                        if (Database.player_abilityIncrease[SteamID] < spendPoints &&
                            ctx.Args[1].ToLower() != "show" && ctx.Args[1].ToLower() != "reset")
                        {
                            Output.SendSystemMessage(ctx, "Not enough points!");
                            return;
                        }

                        switch (abilityName.ToLower())
                        {
                            case "health":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.MaxHealth] += 0.5f * spendPoints;
                                Database.player_abilityIncrease[SteamID] -= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on MaxHealth. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "ppower":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.PhysicalPower] += .25f * spendPoints;
                                Database.player_abilityIncrease[SteamID] -= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on PhysicalPower. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "spower":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.SpellPower] += .25f * spendPoints;
                                Database.player_abilityIncrease[SteamID] -= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on SpellPower. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "presist":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.PhysicalResistance] += 0.05f * spendPoints;
                                Database.player_abilityIncrease[SteamID] -= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on PhysicalResistance. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "sresist":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.SpellResistance] += 0.05f * spendPoints;
                                Database.player_abilityIncrease[SteamID] -= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on SpellResistance. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "beasthunter":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.DamageVsBeasts] += .25f * spendPoints;
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.ResistVsBeasts] += 0.025f * spendPoints;
                                Database.player_abilityIncrease[SteamID] -= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on BeastHunter. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "demonhunter":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.DamageVsDemons] += .25f * spendPoints;
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.ResistVsDemons] += 0.025f * spendPoints;
                                Database.player_abilityIncrease[SteamID] -= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on DemonHunter. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "manhunter":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.ResistVsHumans] += 0.025f * spendPoints;
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.DamageVsHumans] += .25f * spendPoints;
                                Database.player_abilityIncrease[SteamID] -= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on ManHunter. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "undeadhunter":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.DamageVsUndeads] += 0.25f * spendPoints;
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.ResistVsUndeads] += .025f * spendPoints;
                                Database.player_abilityIncrease[SteamID] -= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on UndeadHunter. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "farmer":
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.ResourceYield] += .1f * spendPoints;
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.PhysicalPower] -= .5f * spendPoints;
                                Database.player_level_stats[SteamID][ProjectM.UnitStatType.SpellPower] -= .25f * spendPoints;
                                Database.player_abilityIncrease[SteamID]-= spendPoints;
                                Helper.ApplyBuff(UserEntity, PlayerCharacter, Database.Buff.Buff_VBlood_Perk_Moose);
                                Output.SendSystemMessage(ctx, $"Spent {spendPoints} on Farmer. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                break;
                            case "show":
                                foreach (var buff in Database.player_level_stats[SteamID])
                                {                                    
                                    Output.SendSystemMessage(ctx, $"{buff.Key} : {buff.Value}");
                                }
                                break;
                            case "reset":
                                if (!isAllowed) return;
                                Database.player_level_stats[SteamID] = new LazyDictionary<ProjectM.UnitStatType, float>();
                                Database.player_abilityIncrease[SteamID] = 0;
                                Cache.player_level[SteamID] = 0;
                                ExperienceSystem.SetLevel(PlayerCharacter, UserEntity, SteamID);
                                Output.SendSystemMessage(ctx, "Ability level up points reset.");
                                break;
                            default:
                                Output.SendSystemMessage(ctx, "Type \".xp ability show\" to see current buffs.");
                                Output.SendSystemMessage(ctx, $"Type .xp ability <ability> to sepend ability points. You have {Database.player_abilityIncrease[SteamID]} points left to spend.");
                                Output.SendSystemMessage(ctx, "You can spend ability points on:");
                                Output.SendSystemMessage(ctx, "health, spower, ppower, presist, sresist, beasthunter, demonhunter, manhunter, undeadhunter, farmer");
                                break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Plugin.Logger.LogError($"Could not spend point! {ex.ToString()}");
                        Output.CustomErrorMessage(ctx, $"Could not spend point! {ex.Message}");
                    }
                }
                else
                {
                    Output.InvalidArguments(ctx);
                    return;
                }
            }
            else
            {
                int userLevel = ExperienceSystem.getLevel(SteamID);
                Output.SendSystemMessage(ctx, $"-- <color=#fffffffe>{CharName}</color> --");
                Output.SendSystemMessage(ctx,
                    $"Level:<color=#fffffffe> {userLevel}</color> (<color=#fffffffe>{ExperienceSystem.getLevelProgress(SteamID)}%</color>) " +
                    $" [ XP:<color=#fffffffe> {ExperienceSystem.getXp(SteamID)}</color>/<color=#fffffffe>{ExperienceSystem.convertLevelToXp(userLevel + 1)}</color> ]");
                if (ExperienceSystem.LevelRewardsOn) Output.SendSystemMessage(ctx, $"You have {(Database.player_abilityIncrease.ContainsKey(SteamID) ? Database.player_abilityIncrease[SteamID] : 0)} ability points to spend.");
            }
        }
    }
}

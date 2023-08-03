using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using Unity.Entities;
using ProjectM;
using VampireCommandFramework;
using Bloodstone.API;

namespace OpenRPG.Commands
{

    [CommandGroup("rpg")]
    public static class Experience
    {
        private static EntityManager entityManager = VWorld.Server.EntityManager;

        [Command("experience log", usage: "<1|0>", description:"Toggle the exp gain notification.")]
        public static void ExperienceLogCommand(ChatCommandContext ctx, bool log)
        {
            var user = ctx.Event.User;
            var SteamID = user.PlatformId;

            if (!ExperienceSystem.isEXPActive)
            {
                throw ctx.Error("Experience system is not enabled.");
            }

            if (log)
            {
                Database.player_log_exp[SteamID] = true;
                ctx.Reply($"Experience gain is now logged.");
            }
            else
            {
                Database.player_log_exp[SteamID] = false;
                ctx.Reply($"Experience gain is no longer being logged.");
            }
        }

        [Command("experience set", usage: "<PlayerName> <Value>", description: "Sets the specified players current xp to a specific value")]
        public static void ExperienceSetCommand(ChatCommandContext ctx, string playerName, int value)
        {
            var user = ctx.Event.User;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            var PlayerCharacter = ctx.Event.SenderCharacterEntity;
            var UserEntity = ctx.Event.SenderUserEntity;

            if (!ExperienceSystem.isEXPActive)
            {
                throw ctx.Error("Experience system is not enabled.");
            }


            if (Helper.FindPlayer(playerName, true, out var targetEntity, out var targetUserEntity))
            {
                SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                PlayerCharacter = targetEntity;
                UserEntity = targetUserEntity;
            }
            else
            {
                throw ctx.Error($"Could not find specified player \"{playerName}\".");
            }

            Database.player_experience[SteamID] = value;
            ExperienceSystem.SetLevel(PlayerCharacter, UserEntity, SteamID);
            ctx.Reply($"Player \"{CharName}\" Experience is now set to be<color=#fffffffe> {ExperienceSystem.getXp(SteamID)}</color>");
            
        }

        [Command("experience", usage: "", description: "Shows your currect experience and progression to next level")]
        public static void ExperienceCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            var CharName = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            int userLevel = ExperienceSystem.getLevel(SteamID);
            ctx.Reply($"-- <color=#fffffffe>{CharName}</color> --");
            ctx.Reply(
                $"Level:<color=#fffffffe> {userLevel}</color> (<color=#fffffffe>{ExperienceSystem.getLevelProgress(SteamID)}%</color>) " +
                $" [ XP:<color=#fffffffe> {ExperienceSystem.getXp(SteamID)}</color>/<color=#fffffffe>{ExperienceSystem.convertLevelToXp(userLevel + 1)}</color> ]");
        }
    }
}

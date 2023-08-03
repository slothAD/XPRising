/*using ProjectM;
using ProjectM.Scripting;
using OpenRPG.Utils;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;
using Bloodstone.API;

namespace OpenRPG.Commands
{
    
    public static class Teleport
    {
        [Command("teleport", "<Name>", "Teleport you to another online player within your clan.")]
        public static void Initialize(ChatCommandContext ctx, string name)
        {
            var eventUser = ctx.Event.User;
            var UserCharacter = ctx.Event.SenderCharacterEntity;
            var UserEntity = ctx.Event.SenderUserEntity;
            EntityManager entityManager = VWorld.Server.EntityManager;

            if (Helper.IsPlayerInCombat(UserCharacter))
            {
                throw ctx.Error( "Unable to use command! You're in combat!");
            }

            Team user_TeamComponent = entityManager.GetComponentData<Team>(UserCharacter);

            LocalToWorld target_WorldComponent;
            Team target_TeamComponent;

            if (Helper.FindPlayer(name, true, out Entity TargetChar, out Entity TargetUserEntity))
            {
                target_TeamComponent = entityManager.GetComponentData<Team>(TargetUserEntity);
                target_WorldComponent = entityManager.GetComponentData<LocalToWorld>(TargetChar);
            }
            else
            {
                throw ctx.Error( "Target player not found.");
            }

            var serverGameManager = Plugin.Server.GetExistingSystem<ServerScriptMapper>()?._ServerGameManager;
            if (!serverGameManager._TeamChecker.IsAllies(user_TeamComponent, target_TeamComponent))
            {
                throw ctx.Error( "Unable to teleport to player from another Clan!");
                return;
            }

            if (Helper.IsPlayerInCombat(TargetChar))
            {
                throw ctx.Error( $"Unable to teleport! Player \"{name}\" is in combat!");
            }

            Helper.TeleportTo(ctx, new(target_WorldComponent.Position.x, target_WorldComponent.Position.z));
        }
    }
}*/

using ProjectM;
using OpenRPG.Utils;
using Unity.Entities;
using VampireCommandFramework;

namespace OpenRPG.Commands
{

    [CommandGroup("rpg")]
    public static class ResetCooldown
    {
        [Command("resetcooldown", usage: "[<Player Name>]", description: "Instantly cooldown all ability & skills for the player.")]
        public static void ResetCooldownCommand(ChatCommandContext ctx, string playerName = null)
        {
            Entity PlayerCharacter = ctx.Event.SenderCharacterEntity;
            string CharName = ctx.Event.User.CharacterName.ToString();
            EntityManager entityManager = Plugin.Server.EntityManager;

            if (playerName != null && ctx.User.IsAdmin)
            {
                string name = playerName;
                if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                {
                    PlayerCharacter = targetEntity;
                    CharName = name;
                }
                else
                {
                    throw ctx.Error($"Could not find the specified player \"{name}\".");
                }
            }
            else
            {
                throw ctx.Error($"You don't have access for this operation..");
            }

            var AbilityBuffer = entityManager.GetBuffer<AbilityGroupSlotBuffer>(PlayerCharacter);
            foreach (var ability in AbilityBuffer)
            {
                var AbilitySlot = ability.GroupSlotEntity._Entity;
                var ActiveAbility = entityManager.GetComponentData<AbilityGroupSlot>(AbilitySlot);
                var ActiveAbility_Entity = ActiveAbility.StateEntity._Entity;

                var b = Helper.GetPrefabGUID(ActiveAbility_Entity);
                if (b.GuidHash == 0) continue;

                var AbilityStateBuffer = entityManager.GetBuffer<AbilityStateBuffer>(ActiveAbility_Entity);
                foreach (var state in AbilityStateBuffer)
                {
                    var abilityState = state.StateEntity._Entity;
                    var abilityCooldownState = entityManager.GetComponentData<AbilityCooldownState>(abilityState);
                    abilityCooldownState.CooldownEndTime = 0;
                    entityManager.SetComponentData(abilityState, abilityCooldownState);
                }
            }
            ctx.Reply($"Player \"{CharName}\" cooldown resetted.");
        }
    }
}
using ProjectM;
using ProjectM.Network;
using OpenRPG.Utils;
using VampireCommandFramework;
using Bloodstone.API;

namespace OpenRPG.Commands
{

    [CommandGroup("rpg")]
    public static class Health
    {
        [Command("health", usage:"<percentage> [<player name>]", description:"Sets your current Health")]
        public static void HealthCommmand(ChatCommandContext ctx, int percentage, string playerName = null)
        {
            var PlayerName = ctx.Event.User.CharacterName;
            var UserIndex = ctx.Event.User.Index;
            var component = VWorld.Server.EntityManager.GetComponentData<ProjectM.Health>(ctx.Event.SenderCharacterEntity);
            int Value = 100;

            if (playerName != null)
            {
                var targetName = playerName;
                if (Helper.FindPlayer(targetName, false, out var targetEntity, out var targetUserEntity))
                {
                    PlayerName = targetName;
                    UserIndex = Plugin.Server.EntityManager.GetComponentData<User>(targetUserEntity).Index;
                    component = Plugin.Server.EntityManager.GetComponentData<ProjectM.Health>(targetEntity);
                }
                else
                {
                    throw ctx.Error($"Player \"{targetName}\" not found.");
                }
            }

            float restore_hp = ((component.MaxHealth / 100) * Value) - component.Value;

            var HealthEvent = new ChangeHealthDebugEvent()
            {
                Amount = (int)restore_hp
            };
            Plugin.Server.GetExistingSystem<DebugEventsSystem>().ChangeHealthEvent(UserIndex, ref HealthEvent);

            ctx.Reply($"Player \"{PlayerName}\" Health set to <color=#ffff00>{Value}%</color>");
        }
    }
}

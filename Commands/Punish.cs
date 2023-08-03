using ProjectM.Network;
using OpenRPG.Utils;
using ProjectM;
using VampireCommandFramework;

namespace OpenRPG.Commands
{

    [CommandGroup("rpg")]
    public static class Punish
    {
        [Command("punish", usage: "punish <playername> [<remove(True/False>]", description: "Manually punish someone or lift their debuff.")]
        public static void PunishCommand(ChatCommandContext ctx, string playerName , bool value = false)
        {

                if (Helper.FindPlayer(playerName, true, out var CharEntity, out var UserEntity))
                {
 
                        if (!value)
                        {
                            Helper.RemoveBuff(CharEntity, Database.Buff.Severe_GarlicDebuff);
                            ctx.Reply($"Punishment debuff removed from player \"{playerName}\"");
                        }
                        else
                        {
                            Helper.ApplyBuff(UserEntity, CharEntity, Database.Buff.Severe_GarlicDebuff);
                            ctx.Reply($"Applied punishment debuff to player \"{playerName}\"");
                        }
                }
                else
                {
                    throw ctx.Error("Player not found.");
                  
                }
            
        }
    }
}

using ProjectM.Network;
using OpenRPG.Utils;
using VampireCommandFramework;
using Bloodstone.API;

namespace OpenRPG.Commands
{
    [CommandGroup("rpg")]
    public static class Ping
    {
        [Command("ping", usage: "ping", description: "Shows your latency.")]
        public static void PingCommand(ChatCommandContext ctx)
        {
            var ping = VWorld.Server.EntityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
            ctx.Reply($"Your latency is <color=#ffff00>{ping}</color>s");
        }
    }
}

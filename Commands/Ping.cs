using ProjectM.Network;
using OpenRPG.Utils;
using VampireCommandFramework;
using Bloodstone.API;

namespace OpenRPG.Commands
{
    
    public static class Ping
    {
        [Command(name: "ping", adminOnly: false, usage: "ping", description: "Shows your latency.")]
        public static void PingCommand(ChatCommandContext ctx)
        {
            var ping = VWorld.Server.EntityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
            ctx.Reply($"Your latency is <color=#ffff00>{ping}</color>s");
        }
    }
}

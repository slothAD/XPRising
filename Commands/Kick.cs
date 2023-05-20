using Il2CppSystem.Runtime.Remoting.Messaging;
using RPGMods.Utils;
using VampireCommandFramework;

namespace RPGMods.Commands{
    public static class Kick{
        [Command("kick", "kick <playername>", "Kick the specified player out of the server.", adminOnly: false)]
        public static void Initialize(ICommandContext ctx, string name){


            if (Helper.FindPlayer(name, true, out _, out var targetUserEntity)){
                Helper.KickPlayer(targetUserEntity);
                ctx.Reply($"Player \"{name}\" has been kicked from server.");
            }
            else{
                ctx.Reply("Specified player not found.");
            }
        }
    }
}

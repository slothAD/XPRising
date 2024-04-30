using OpenRPG.Utils;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    public static class Kick
    {
        [Command("kick", "kick", "<playername>", "Kick the specified player out of the server.", adminOnly: true)]
        public static void KickCommand(ChatCommandContext ctx, string name)
        {
            if (Helper.FindPlayer(name, true, out _, out var targetUserEntity))
            {
                Helper.KickPlayer(targetUserEntity);
                ctx.Reply($"Player \"{name}\" has been kicked from server.");
            }
            else
            {
                throw ctx.Error("Specified player not found.");
            }
        }
    }
}

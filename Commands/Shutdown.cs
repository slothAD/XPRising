using OpenRPG.Utils;
using ProjectM;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    [CommandGroup("rpg")]
    public static class Shutdown
    {
        [Command("shutdown", usage: "", description: "Trigger the exit signal & shutdown the server.")]
        public static void ShutdownCommand(ChatCommandContext ctx)
        {
            UnityEngine.Application.Quit();
        }
    }
}

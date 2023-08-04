using OpenRPG.Utils;
using ProjectM;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    
    public static class Shutdown
    {
        [Command("shutdown", shortHand: "shutdown", adminOnly: false, usage: "", description: "Trigger the exit signal & shutdown the server.")]
        public static void ShutdownCommand(ChatCommandContext ctx)
        {
            UnityEngine.Application.Quit();
        }
    }
}

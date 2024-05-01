using OpenRPG.Utils;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    public static class Save {
        [Command("save", description: "Force the server to write OpenRPG DB to file.", adminOnly: true)]
        public static void SaveCommand(ChatCommandContext ctx){
            ctx.Reply($"Saving data....");
            AutoSaveSystem.SaveDatabase();
            ctx.Reply($"Data save complete.");
        }
    }
}

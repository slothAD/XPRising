using OpenRPG.Utils;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    public static class CacheCommands {
        [Command("save", description: "Force the server to write OpenRPG DB to file.", adminOnly: true)]
        public static void SaveCommand(ChatCommandContext ctx){
            ctx.Reply($"Saving data....");
            AutoSaveSystem.SaveDatabase();
            ctx.Reply($"Data save complete.");
        }
        
        [Command("load", description: "Force the server to load OpenRPG DB from file.", adminOnly: true)]
        public static void LoadCommand(ChatCommandContext ctx){
            ctx.Reply($"Loading data....");
            AutoSaveSystem.LoadDatabase();
            ctx.Reply($"Data load complete.");
        }
    }
}

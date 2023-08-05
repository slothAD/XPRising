using ProjectM;
using OpenRPG.Utils;
using System.Text.RegularExpressions;
using VampireCommandFramework;
using Bloodstone.API;

namespace OpenRPG.Commands
{
    
    public static class Save
    {
        [Command(name: "save", shortHand: "sv", adminOnly: false, usage: "[\"<name>\"]", description: "Force the server to save the game as well as write OpenRPG DB to a json file.")]
        public static void SaveCommand(ChatCommandContext ctx, string name = "Manual Save")
        {

            if (name.Length > 50)
            {
                throw ctx.Error("Name is too long!");
            }
            if (Regex.IsMatch(name, @"[^a-zA-Z0-9\x20]"))
            {
                throw ctx.Error("Name can only contain alphanumeric & space!");
            }


            ctx.Reply($"Saving data....");
            AutoSaveSystem.SaveDatabase();
            //VWorld.Server.GetExistingSystem<TriggerPersistenceSaveSystem>().TriggerSave(SaveReason.ManualSave, name, ServerRuntimeSettings.Save);
            ctx.Reply($"Data save complete.");
        }
    }
}

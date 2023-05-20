using ProjectM;
using RPGMods.Utils;
using System.Text.RegularExpressions;

namespace RPGMods.Commands
{/*
    [Command("load", Usage = "load", Description = "Force the server to load RPGMods DB from a json file.")]
    public static class Load
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;
            string name = "Manual Save";
            if (args.Length >= 1)
            {
                name = string.Join(' ', ctx.Args);
                if (name.Length > 50)
                {
                    Output.CustomErrorMessage(ctx, "Name is too long!");
                    return;
                }
                if (Regex.IsMatch(name, @"[^a-zA-Z0-9\x20]"))
                {
                    Output.CustomErrorMessage(ctx, "Name can only contain alphanumeric & space!");
                    return;
                }
            }

            Output.SendSystemMessage(ctx, $"Loading data....");
            AutoSaveSystem.LoadDatabase();
            //Plugin.Server.GetExistingSystem<TriggerPersistenceSaveSystem>().TriggerSave(SaveReason.ManualSave, name);
            Output.SendSystemMessage(ctx, $"Data load complete.");
        }
    }*/
}

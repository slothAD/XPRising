using ProjectM;
using RPGMods.Utils;
using System.Text.RegularExpressions;

namespace RPGMods.Commands
{/*
    [Command("save", Usage = "save [<name>]", Description = "Force the server to write RPGMods DB to a json file.")]
    public static class Save
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
                    ctx.Reply("Name is too long!");
                    return;
                }
                if (Regex.IsMatch(name, @"[^a-zA-Z0-9\x20]"))
                {
                    ctx.Reply("Name can only contain alphanumeric & space!");
                    return;
                }
            }

            ctx.Reply($"Saving data....");
            AutoSaveSystem.SaveDatabase();
            //Plugin.Server.GetExistingSystem<TriggerPersistenceSaveSystem>().TriggerSave(SaveReason.ManualSave, name);
            ctx.Reply($"Data save complete.");
        }
    }*/
}

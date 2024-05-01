using System;
using OpenRPG.Systems;
using OpenRPG.Utils;
using VampireCommandFramework;
using ProjectM;
using Prefabs = OpenRPG.Utils.Prefabs;

namespace OpenRPG.Commands
{
    public static class WorldDynamics
    {

        [Command(name: "worlddynamics info", shortHand: "wd info", adminOnly: false, usage: "[faction]", description: "List faction stats of all active factions or given faction")]
        public static void WorldDynamicsCommand(ChatCommandContext ctx, string faction = "all")
        {
            if (WorldDynamicsSystem.isFactionDynamic == false)
            {
                throw ctx.Error("World dynamics system is not enabled.");
            }
            var factionList = Database.FactionStats;
            var useSpecificFaction = faction.ToLower() != "all";
            var sentReply = false;
            foreach (var item in factionList)
            {
                if (!item.Value.Active) continue;
                if (useSpecificFaction && !item.Value.Name.Contains(faction)) continue;
                ctx.Reply($"Name: {Utils.Color.Green(item.Value.Name)} [Lv.{Utils.Color.Yellow(item.Value.Level.ToString())}]");
                ctx.Reply($"Active Pwr: [{Utils.Color.White(item.Value.ActivePower.ToString())}] Stored Pwr: [{Utils.Color.Yellow(item.Value.StoredPower.ToString())}]");
                ctx.Reply($"Daily Pwr: [{Utils.Color.Teal(item.Value.DailyPower.ToString())}] Req. Pwr: [{Utils.Color.SoftRed(item.Value.RequiredPower.ToString())}]");
                sentReply = true;
            }

            if (!sentReply)
            {
                if (useSpecificFaction) ctx.Reply($"{faction} is not active.");
                else ctx.Reply("No active faction.");
            }
        }

        [Command(name: "worlddynamics ignore", shortHand: "wd ignore", adminOnly: true, usage: "<npc prefab name>", description: "Ignores a specified mob for buffing.")]
        public static void WorldDynamicsIgnoreCommand(ChatCommandContext ctx, string mobName)
        {
            if (Enum.TryParse(mobName, true, out Prefabs.Units unit))
            {
                Database.IgnoredMonstersGUID.Add(new PrefabGUID((int)unit));
                ctx.Reply($"NPC \"{mobName}\" is now ignored for faction buffing.");
            }
            else
            {
                throw ctx.Error("Specified NPC not found.");
            }
        }

        [Command(name: "worlddynamics unignore", shortHand: "wd unignore", adminOnly: true, usage: "<npc prefab name>", description: "Removes a mob from the world dynamics ignore list.")]
        public static void WorldDynamicsUnIgnoreCommand(ChatCommandContext ctx, string mobName)
        {
            if (Enum.TryParse(mobName, true, out Prefabs.Units unit))
            {
                Database.IgnoredMonstersGUID.Remove(new PrefabGUID((int)unit));
                ctx.Reply($"NPC \"{mobName}\" is removed from faction buff ignore list.");
            }
            else
            {
                throw ctx.Error("Specified NPC not found.");
            }
        }

        [Command(name: "worlddynamics save", shortHand: "wd save", adminOnly: true, usage: "", description: "Save to the json file.")]
        public static void WorldDynamicsSaveCommand(ChatCommandContext ctx)
        {
            AutoSaveSystem.SaveDatabase();
            ctx.Reply($"Factions data & ignored mobs saved.");
        }

        [Command(name: "worlddynamics load", shortHand: "wd load", adminOnly: true, usage: "", description: "Load from the json file.")]
        public static void WorldDynamicsLoadCommand(ChatCommandContext ctx)
        {
            AutoSaveSystem.LoadDatabase();
            ctx.Reply($"Factions & ignored mobs json data loaded.");
        }
    }
}

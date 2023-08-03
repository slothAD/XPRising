using ProjectM.Network;
using OpenRPG.Systems;
using OpenRPG.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using ProjectM;
using VampireCommandFramework;
using System.Linq;

namespace OpenRPG.Commands
{

    public static class WorldDynamics
    {

        [Command("worlddynamics", usage: "<faction>", description: "List all faction stats. Save them, or load from the json file.")]
        public static void WorldDynamicsCommand(ChatCommandContext ctx, string faction = "all")
        {
            if (WorldDynamicsSystem.isFactionDynamic == false)
            {
                throw ctx.Error("World dynamics system is not enabled.");

            }

            int i = 0;
            var factionList = Database.FactionStats;
            if (faction.ToLower() != "all")
            {
                factionList = (System.Collections.Concurrent.ConcurrentDictionary<int, FactionData>) factionList.Where(factionItem => factionItem.Value.Name.Contains(faction));
            }
            foreach (var item in Database.FactionStats)
            {
                if (!item.Value.Active) continue;
                i++;
                ctx.Reply($"Name: {Utils.Color.Green(item.Value.Name)} [Lv.{Utils.Color.Yellow(item.Value.Level.ToString())}]");
                ctx.Reply($"Active Pwr: [{Utils.Color.White(item.Value.ActivePower.ToString())}] Stored Pwr: [{Utils.Color.Yellow(item.Value.StoredPower.ToString())}]");
                ctx.Reply($"Daily Pwr: [{Utils.Color.Teal(item.Value.DailyPower.ToString())}] Req. Pwr: [{Utils.Color.SoftRed(item.Value.RequiredPower.ToString())}]");
            }
            if (i == 0) ctx.Reply("No active facton.");


        }

        [Command("worlddynamics ignore", usage: "<npc prefab name>", description: "Ignores a specified mob for buffing.")]
        public static void WorldDynamicsIgnoreCommand(ChatCommandContext ctx, string mobName)
        {
            if (Database.database_units.TryGetValue(mobName, out var mobGUID))
            {
                Database.IgnoredMonsters.Add(mobName);
                Database.IgnoredMonstersGUID.Add(mobGUID);
                ctx.Reply($"NPC \"{mobName}\" is now ignored for faction buffing.");
            }
            else
            {
                throw ctx.Error("Specified NPC not found.");
            }
        }

        [Command("worlddynamics unignore", usage: "<npc prefab name>", description: "Removes a mob from the world dynamics ignore list.")]
        public static void WorldDynamicsUnIgnoreCommand(ChatCommandContext ctx, string mobName)
        {
            if (Database.database_units.TryGetValue(mobName, out var mobGUID))
            {
                Database.IgnoredMonsters.Remove(mobName);
                Database.IgnoredMonstersGUID.Remove(mobGUID);
                ctx.Reply($"NPC \"{mobName}\" is removed from faction buff ignore list.");
            }
            else
            {
                throw ctx.Error("Specified NPC not found.");
            }
        }

        [Command("worlddynamics save", usage: "wd [<faction>] [<stats>|<save>|<load>|<ignore>|<unignore>] [<npc prefab name>]", description: "List all faction stats. Save them, or load from the json file.")]
        public static void WorldDynamicsSaveCommand(ChatCommandContext ctx)
        {
            WorldDynamicsSystem.SaveFactionStats();
            WorldDynamicsSystem.SaveIgnoredMobs();
            ctx.Reply($"Factions data & ignored mobs saved.");
        }

        [Command("worlddynamics load", usage: "wd [<faction>] [<stats>|<save>|<load>|<ignore>|<unignore>] [<npc prefab name>]", description: "List all faction stats. Save them, or load from the json file.")]
        public static void WorldDynamicsLoadCommand(ChatCommandContext ctx)
        {
            WorldDynamicsSystem.LoadFactionStats();
            WorldDynamicsSystem.LoadIgnoredMobs();
            ctx.Reply($"Factions & ignored mobs json data loaded.");
        }
    }
}

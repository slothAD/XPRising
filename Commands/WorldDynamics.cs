using OpenRPG.Systems;
using OpenRPG.Utils;
using VampireCommandFramework;
using System.Linq;

namespace OpenRPG.Commands
{
    
    public static class WorldDynamics
    {

        [Command(name: "worlddynamics", shortHand: "wd", adminOnly: false, usage: "<faction>", description: "List all faction stats")]
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

        [Command(name: "worlddynamics ignore", shortHand: "wd ignore", adminOnly: false, usage: "<npc prefab name>", description: "Ignores a specified mob for buffing.")]
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

        [Command(name: "worlddynamics unignore", shortHand: "wd unignore", adminOnly: false, usage: "<npc prefab name>", description: "Removes a mob from the world dynamics ignore list.")]
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

        [Command(name: "worlddynamics save", shortHand: "wd save", adminOnly: false, usage: "", description: "Save to the json file.")]
        public static void WorldDynamicsSaveCommand(ChatCommandContext ctx)
        {
            WorldDynamicsSystem.SaveFactionStats();
            WorldDynamicsSystem.SaveIgnoredMobs();
            ctx.Reply($"Factions data & ignored mobs saved.");
        }

        [Command(name: "worlddynamics load", shortHand: "wd load", adminOnly: false, usage: "", description: "Load from the json file.")]
        public static void WorldDynamicsLoadCommand(ChatCommandContext ctx)
        {
            WorldDynamicsSystem.LoadFactionStats();
            WorldDynamicsSystem.LoadIgnoredMobs();
            ctx.Reply($"Factions & ignored mobs json data loaded.");
        }
    }
}

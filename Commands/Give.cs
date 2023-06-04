using ProjectM;
using RPGMods.Utils;
using System.Globalization;
using System.Linq;
using VampireCommandFramework;

namespace RPGMods.Commands{
    //[Command("give, g", Usage = "give <itemname> [<amount>]", Description = "Adds specified items to your inventory")]
    //[CommandGroup("Give", "g")]
    public static class Give{

        [Command("Give", "g", "\"<itemName>\" <amount>", "Adds specified items to your inventory", adminOnly: true)]
        public static void Initialize(ChatCommandContext ctx, string itemName, int amount = 1)
        {

            string name = itemName;

            PrefabGUID guid = Helper.GetGUIDFromName(name);
            if (guid.GuidHash == 0)
            {
                ctx.Reply("Could not find specified item name.");
                return;
            }

            Helper.AddItemToInventory(ctx, guid, amount);
            ctx.Reply($"You got <color=#ffff00>{amount} {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name)}</color>");
        }

        [Command("List All Items", "LAI", adminOnly: true)]
        public static void listAllItems(ChatCommandContext ctx)
        {
            var gameDataSystem = Plugin.Server.GetExistingSystem<GameDataSystem>();
            var managed = gameDataSystem.ManagedDataRegistry;

            foreach (var entry in gameDataSystem.ItemHashLookupMap)
            {
                try
                {
                    var item = managed.GetOrDefault<ManagedItemData>(entry.Key);
                    if (item.PrefabName.StartsWith("Item_VBloodSource") || item.PrefabName.StartsWith("GM_Unit_Creature_Base") || item.PrefabName == "Item_Cloak_ShadowPriest") continue;
                    ctx.Reply(item.Name.ToString());

                }
                catch { }
            }

        }
    }
}

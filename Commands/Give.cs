using ProjectM;
using RPGMods.Utils;
using System.Globalization;
using System.Linq;
using VampireCommandFramework;

namespace RPGMods.Commands{
    public static class Give{

        [Command("Give", "g", "\"<itemName>\" <amount>", "Adds specified items to your inventory", adminOnly: true)]
        public static void GiveItem(ChatCommandContext ctx, string itemName, int amount = 1)
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
        public static void ListAllItems(ChatCommandContext ctx)
        {
            var gameDataSystem = Plugin.Server.GetExistingSystem<GameDataSystem>();
            var managed = gameDataSystem.ManagedDataRegistry;

            foreach (var entry in gameDataSystem.ItemHashLookupMap)
            {
                try
                {
                    var item = managed.GetOrDefault<ManagedItemData>(entry.Key);
                    //if (item.PrefabName.StartsWith("Item_VBloodSource") || item.PrefabName.StartsWith("GM_Unit_Creature_Base") || item.PrefabName == "Item_Cloak_ShadowPriest" || item.PrefabName.StartsWith("FakeItem")) continue;
                    //ctx.Reply(item.Name.ToString());
                    ctx.Reply(item.PrefabName);
                    Plugin.Logger.LogInfo("Prefab Name: " + item.PrefabName);
                    //Plugin.Logger.LogInfo("Item Name: " + item.Name);
                    //Plugin.Logger.LogInfo("Item Name to string: " + item.Name.ToString());

                }
                catch { }
            }

        }
    }
}

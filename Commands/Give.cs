using ProjectM;
using OpenRPG.Utils;
using System.Globalization;
using System.Linq;
using VampireCommandFramework;

namespace OpenRPG.Commands
{
    [CommandGroup("rpg")]
    public static class Give
    {
        [Command("give", usage: "<itemname> [<amount>]", description: "Adds specified items to your inventory")]
        public static void GiveCommand(ChatCommandContext ctx, string name, int amount = 1)
        {
            PrefabGUID guid = Helper.GetGUIDFromName(name);
            if (guid.GuidHash == 0)
            {
                throw ctx.Error("Could not find specified item name.");
            }

            Helper.AddItemToInventory(ctx, guid, amount);
            ctx.Reply($"You got <color=#ffff00>{amount} {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name)}</color>");
        }
    }
}

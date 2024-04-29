using ProjectM;
using OpenRPG.Utils;
using System.Globalization;
using System.Linq;
using VampireCommandFramework;

namespace OpenRPG.Commands {
    public static class Kit {
        [Command("kit", usage:"kit <Name>", description:"Gives you a previously specified set of items.")]
        public static void KitCommand(ChatCommandContext ctx, string name) {

            try {
                ItemKit kit = Database.kits.First(x => x.Name.ToLower() == name.ToLower());
                foreach (var guid in kit.PrefabGUIDs) {
                    Helper.AddItemToInventory(ctx, new PrefabGUID(guid.Key), guid.Value);
                }
                ctx.Reply($"You got the kit: <color=#ffff00>{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name)}</color>");
            } catch {
                ctx.Reply($"Kit doesn't exist.");
                return;
            }
        }
    }
}
